using System;
using System.IO;
using System.Linq;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.Engine.Persistence
{
    public static class PositionFile
    {
        public const String BinaryExtension = ".TgPosition";
        public const String FenExtension = ".FEN";
        private static readonly UTF8Encoding Utf8 = new(false, true);

        private static Boolean IsBinary(String path)
        {
            String extension = Path.GetExtension(path);
            return extension.Equals(BinaryExtension, StringComparison.OrdinalIgnoreCase)
                ? true
                : extension.Equals(FenExtension, StringComparison.OrdinalIgnoreCase)
                ? false
                : throw new NotSupportedException("Choose a .TgPosition or .FEN file.");
        }

        public static void Save(String path, Position position)
        {
            Boolean binary = IsBinary(path);
            Validate(position.PositionDataSerializable);
            Position snapshot = position.Clone();
            AtomicFile.Write(path, stream =>
            {
                if (binary)
                {
                    snapshot.Save(stream);
                }
                else
                {
                    using StreamWriter writer = new(stream, Utf8, leaveOpen: true);
                    writer.WriteLine(snapshot.FEN_Notation);
                }
            });
        }

        public static Position Load(String path)
        {
            Boolean binary = IsBinary(path);
            Byte[] bytes = File.ReadAllBytes(path);
            if (bytes.Length == 0)
            {
                throw new InvalidDataException("The position file is empty.");
            }
            try
            {
                // Older editor versions wrote FEN text under the binary extension.
                Int32 first = bytes.AsSpan().StartsWith(new Byte[] { 0xEF, 0xBB, 0xBF }) ? 3 : 0;
                while (first < bytes.Length && Char.IsWhiteSpace((Char)bytes[first]))
                {
                    first++;
                }
                Boolean legacyText = first < bytes.Length && "12345678rnbqkpRNBQKP".Contains((Char)bytes[first]);
                if (!binary || legacyText)
                {
                    String fen = Utf8.GetString(bytes).TrimStart('﻿').Trim();
                    return ParseFen(fen);
                }
                using MemoryStream stream = new(bytes, writable: false);
                PositionDataSerializable data = ProtoBuf.Serializer.Deserialize<PositionDataSerializable>(stream);
                Validate(data);
                return Position.GetPosition(data);
            }
            catch (Exception error) when (error is ProtoBuf.ProtoException or EndOfStreamException or FormatException or ArgumentException or OverflowException)
            {
                throw new InvalidDataException("The position file is malformed or unsupported.", error);
            }
        }

        public static Position ParseFen(String fen)
        {
            ArgumentNullException.ThrowIfNull(fen);
            try
            {
                ValidateFen(fen);
                Position result = Position.ReadFEN(fen);
                Validate(result.PositionDataSerializable);
                String rights = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[2];
                foreach (Char right in rights.Where(static c => c != '-'))
                {
                    String home = right switch
                    {
                        'K' => "e1",
                        'k' => "e8",
                        'Q' => "d1",
                        _ => "d8"
                    };
                    Colours colour = Char.IsUpper(right) ? Colours.White : Colours.Black;
                    Pieces piece = Char.ToUpperInvariant(right) == 'K' ? Pieces.King : Pieces.Queen;
                    if (!result.IsFieldTaken(home) || result.GetPieceColour(home) != colour || result.GetPiece(home) != piece)
                    {
                        throw new InvalidDataException("A first-move right requires the matching king or queen on its starting square.");
                    }
                }
                return result;
            }
            catch (Exception error) when (error is FormatException or ArgumentException or OverflowException)
            {
                throw new InvalidDataException("The FEN is malformed or unsupported.", error);
            }
        }

        internal static void Validate(PositionDataSerializable data)
        {
            if (data == null || (data.OnMove != (Int32)Colours.White && data.OnMove != (Int32)Colours.Black)
                || data.HalfmoveClock < 0 || data.FullMoveCounter < 1
                || ((data.PieceColours | data.Pieces_0 | data.Pieces_1 | data.Pieces_2) & ~data.TakenFields) != 0)
            {
                throw new InvalidDataException("Invalid position metadata or piece data.");
            }
            Position raw = new(data.PositionData, (Colours)data.OnMove);
            foreach (Colours colour in new[] { Colours.White, Colours.Black })
            {
                PieceDataExtended[] kings = [.. raw.ToArray().Where(x => x.Colour == colour && (x.Piece == Pieces.King || x.Piece == Pieces.KingMoved))];
                if (kings.Length != 1 || (kings[0].Piece == Pieces.King && kings[0].Location.HumanReadableCoordinates != (colour == Colours.White ? "e1" : "e8")))
                {
                    throw new InvalidDataException("Each side needs one king; an unmoved king must be on its starting square.");
                }
            }
        }

        private static void ValidateFen(String fen)
        {
            String[] fields = fen.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (fields.Length != 6 || (fields[1] != "w" && fields[1] != "b")
                || !Int32.TryParse(fields[4], out Int32 half) || half < 0 || !Int32.TryParse(fields[5], out Int32 full) || full < 1
                || (fields[2] != "-" && (fields[2].Length == 0 || fields[2].Any(static c => !"QKkq".Contains(c)) || fields[2].Distinct().Count() != fields[2].Length)))
            {
                throw new InvalidDataException("Expected six medieval FEN fields with valid side, rights and counters.");
            }
            String[] ranks = fields[0].Split('/');
            if (ranks.Length != 8)
            {
                throw new InvalidDataException("FEN must contain eight ranks.");
            }
            foreach (String rank in ranks)
            {
                Int32 width = 0;
                foreach (Char symbol in rank)
                {
                    if (symbol is >= '1' and <= '8')
                    {
                        width += symbol - '0';
                    }
                    else if ("rnbqkpRNBQKP".Contains(symbol))
                    {
                        width++;
                    }
                    else
                    {
                        throw new InvalidDataException("Unknown FEN piece or empty-square count.");
                    }
                }
                if (width != 8)
                {
                    throw new InvalidDataException("Every FEN rank must contain eight squares.");
                }
            }

        }
    }
}
