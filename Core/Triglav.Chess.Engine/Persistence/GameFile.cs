using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;

namespace Triglav.Chess.Engine.Persistence
{
    public static class GameFile
    {
        public const String Extension = ".TgGame";

        private static void Validate(GameDataSerializable game)
        {
            ArgumentNullException.ThrowIfNull(game);
            PositionFile.Validate(game.StartingPosition);
            if (game.Moves == null || !Enum.IsDefined(game.Result))
            {
                throw new InvalidDataException("Invalid game metadata.");
            }
            foreach (MoveSerializable move in game.Moves)
            {
                if (move == null || move.From < 0 || move.From > 63 || move.To < 0 || move.To > 63 || !move.Move.IsValid)
                {
                    throw new InvalidDataException("Invalid saved move.");
                }
                PositionFile.Validate(move.ResultingPosition);
            }
        }

        public static void Save(String path, GameDataSerializable game)
        {
            CheckExtension(path);
            Validate(game);
            AtomicFile.Write(path, stream => ProtoBuf.Serializer.Serialize(stream, game));
        }

        public static GameDataSerializable Load(String path)
        {
            CheckExtension(path);
            using FileStream stream = File.OpenRead(path);
            try
            {
                GameDataSerializable game = ProtoBuf.Serializer.Deserialize<GameDataSerializable>(stream);
                Validate(game);
                return game;
            }
            catch (Exception error) when (error is ProtoBuf.ProtoException or EndOfStreamException)
            {
                throw new InvalidDataException("The game file is malformed.", error);
            }
        }

        public static GameImportResult Import(String path, GameImportOptions options = null)
        {
            options ??= new GameImportOptions();
            if (options.MaxBytes < 1 || options.MaxPlies < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options));
            }
            String extension = Path.GetExtension(path);
            if (extension.Equals(".PGN", StringComparison.OrdinalIgnoreCase))
            {
                PgnImportResult pgn = ImportPgn(path, new PgnImportOptions
                {
                    MaxBytes = options.MaxBytes,
                    MaxPlies = options.MaxPlies,
                    AssumeTriglavVariant = options.AssumeTriglavVariant,
                    AllowLegacyTriglav = options.AllowLegacyPgn,
                });
                return new GameImportResult(pgn.Game, pgn.FinalPosition,
                    pgn.Warnings.Select(static warning => new GameImportWarning(GameImportWarningCode.Pgn, warning)).ToArray());
            }
            if (!extension.Equals(Extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Choose a .TgGame or .PGN file.");
            }
            using FileStream input = File.OpenRead(path);
            using MemoryStream bytes = new();
            Byte[] buffer = new Byte[8192];
            Int32 read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bytes.Length + read > options.MaxBytes)
                {
                    throw new InvalidDataException("The game file exceeds the import limit.");
                }
                bytes.Write(buffer, 0, read);
            }
            bytes.Position = 0;
            GameDataSerializable game;
            try
            {
                game = ProtoBuf.Serializer.Deserialize<GameDataSerializable>(bytes);
            }
            catch (Exception error) when (error is ProtoBuf.ProtoException or EndOfStreamException)
            {
                throw new InvalidDataException("The game file is malformed.", error);
            }
            Validate(game);
            if (game.Moves.Count > options.MaxPlies)
            {
                throw new InvalidDataException("The game exceeds the move import limit.");
            }
            GameReplayResult replay = GameReplay.Replay(game);
            GameDataSerializable normalized = new()
            {
                StartingPosition = game.StartingPosition,
                WhitePlayer = game.WhitePlayer,
                BlackPlayer = game.BlackPlayer,
                Result = game.Result,
                Metadata = game.Metadata,
                Moves = replay.Moves,
            };
            IReadOnlyList<GameImportWarning> warnings = replay.ReconstructedCounters
                ? new[] { new GameImportWarning(GameImportWarningCode.ReconstructedCounters) }
                : Array.Empty<GameImportWarning>();
            return new GameImportResult(normalized, replay.FinalPosition, warnings);
        }

        public static PgnImportResult ImportPgn(String path, PgnImportOptions options = null)
        {
            if (!Path.GetExtension(path).Equals(".PGN", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Choose a .PGN file.");
            }
            using FileStream stream = File.OpenRead(path);
            return PgnReader.Read(stream, options);
        }

        public static void ExportPgn(String path, GameDataSerializable game)
        {
            if (!Path.GetExtension(path).Equals(".PGN", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Choose a .PGN file.");
            }
            Validate(game);
            String pgn = game.PGN;
            AtomicFile.Write(path, stream =>
            {
                using StreamWriter writer = new(stream, new UTF8Encoding(false, true), leaveOpen: true);
                writer.Write(pgn);
            });
        }

        private static void CheckExtension(String path)
        {
            if (!Path.GetExtension(path).Equals(Extension, StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException("Choose a .TgGame file.");
            }
        }
    }
}
