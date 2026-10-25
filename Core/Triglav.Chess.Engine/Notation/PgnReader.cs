using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Notation
{
    public static class PgnReader
    {
        private static readonly UTF8Encoding Utf8 = new(false, true);

        private static PgnImportOptions Options(PgnImportOptions options)
        {
            options ??= new PgnImportOptions();
            if (options.MaxBytes < 1 || options.MaxPlies < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(options));
            }
            return options;
        }

        public static PgnImportResult Read(Stream stream, PgnImportOptions options = null)
        {
            ArgumentNullException.ThrowIfNull(stream);
            options = Options(options);
            using MemoryStream bytes = new();
            Byte[] buffer = new Byte[8192];
            Int32 count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                if (bytes.Length + count > options.MaxBytes)
                {
                    throw new PgnException(PgnDiagnosticCode.LimitExceeded);
                }
                bytes.Write(buffer, 0, count);
            }
            try
            {
                return Read(Utf8.GetString(bytes.GetBuffer(), 0, (Int32)bytes.Length), options);
            }
            catch (DecoderFallbackException)
            {
                throw new PgnException(PgnDiagnosticCode.InvalidEncoding);
            }
        }

        public static PgnImportResult Read(String text, PgnImportOptions options = null)
        {
            ArgumentNullException.ThrowIfNull(text);
            options = Options(options);
            try
            {
                if (Utf8.GetByteCount(text) > options.MaxBytes)
                {
                    throw new PgnException(PgnDiagnosticCode.LimitExceeded);
                }
            }
            catch (EncoderFallbackException)
            {
                throw new PgnException(PgnDiagnosticCode.InvalidEncoding);
            }
            return new Parser(text, options).Read();
        }

        private sealed class Parser
        {
            private readonly PgnImportOptions options;
            private readonly List<PgnDiagnostic> warnings = new();
            private readonly Dictionary<String, String> headers = new(StringComparer.Ordinal);
            private readonly Dictionary<String, PgnToken> locations = new(StringComparer.Ordinal);
            private readonly PgnLexer lexer;
            private PgnToken token;
            private Int32 ply;

            internal Parser(String text, PgnImportOptions options)
            {
                this.options = options;
                this.lexer = new PgnLexer(text, this.warnings);
                this.Advance();
            }

            private void Advance()
            {
                try
                {
                    this.token = this.lexer.Next();
                }
                catch (PgnException error)
                {
                    throw new PgnException(error.Diagnostic.Code, error.Diagnostic.Line, error.Diagnostic.Column, this.ply + 1, error.Diagnostic.Token);
                }
            }
            private PgnException Error(PgnDiagnosticCode code) => new(code, this.token.Line, this.token.Column, this.ply + 1, this.token.Value);
            private PgnException HeaderError(PgnDiagnosticCode code, String name)
            {
                PgnToken location = this.locations.GetValueOrDefault(name, this.token);
                return new PgnException(code, location.Line, location.Column, this.ply, this.Header(name, name));
            }

            private String Header(String name, String fallback = null) => this.headers.GetValueOrDefault(name, fallback);
            private void Warning(PgnDiagnosticCode code, PgnToken location) => this.warnings.Add(new PgnDiagnostic(code, location.Line, location.Column, this.ply, location.Value));

            private void ReadHeaders()
            {
                while (this.token.Kind == PgnTokenKind.Open)
                {
                    this.Advance();
                    PgnToken name = this.token;
                    if (name.Kind != PgnTokenKind.Word || !Regex.IsMatch(name.Value, @"^[A-Za-z][A-Za-z0-9_]*$"))
                    {
                        throw this.Error(PgnDiagnosticCode.Syntax);
                    }
                    this.Advance();
                    String value;
                    if (this.token.Kind == PgnTokenKind.Quoted)
                    {
                        value = this.token.Value;
                        this.Advance();
                    }
                    else if (this.options.AllowLegacyTriglav && name.Value == "Result"
                        && (this.token.Kind == PgnTokenKind.Close || this.token.Kind == PgnTokenKind.Word))
                    {
                        value = this.token.Kind == PgnTokenKind.Close ? String.Empty : this.token.Value;
                        if (this.token.Kind != PgnTokenKind.Close)
                        {
                            this.Advance();
                        }
                        this.Warning(PgnDiagnosticCode.LegacyRepair, name);
                    }
                    else
                    {
                        throw this.Error(PgnDiagnosticCode.Syntax);
                    }
                    if (this.token.Kind != PgnTokenKind.Close)
                    {
                        throw this.Error(PgnDiagnosticCode.Syntax);
                    }
                    PgnProfile.ValidateTag(name.Value, value);
                    if (PgnProfile.ReservedTags.Contains(name.Value) && !PgnProfile.ReservedTags.Any(key => key == name.Value))
                    {
                        throw new PgnException(PgnDiagnosticCode.InvalidMetadata, name.Line, name.Column, token: name.Value);
                    }
                    if (this.headers.ContainsKey(name.Value))
                    {
                        throw new PgnException(PgnDiagnosticCode.DuplicateHeader, name.Line, name.Column, token: name.Value);
                    }
                    if (this.headers.Count >= 256)
                    {
                        throw this.Error(PgnDiagnosticCode.LimitExceeded);
                    }
                    this.headers.Add(name.Value, value);
                    this.locations.Add(name.Value, name);
                    this.Advance();
                }
            }

            private void ValidateProfile()
            {
                String variant = this.Header("Variant");
                if (variant == null)
                {
                    if (!this.options.AssumeTriglavVariant && !this.options.AllowLegacyTriglav)
                    {
                        throw this.Error(PgnDiagnosticCode.MissingVariant);
                    }
                    this.Warning(PgnDiagnosticCode.UnmarkedVariant, this.token);
                }
                else if (variant != PgnProfile.Variant)
                {
                    PgnToken location = this.locations["Variant"];
                    throw new PgnException(PgnDiagnosticCode.UnknownVariant, location.Line, location.Column, token: variant);
                }
                if (this.Header("TriglavVariantVersion", PgnProfile.Version) != PgnProfile.Version)
                {
                    throw this.HeaderError(PgnDiagnosticCode.UnsupportedVersion, "TriglavVariantVersion");
                }
            }

            private Position InitialPosition()
            {
                String fen = this.Header("FEN");
                String setup = this.Header("SetUp");
                if ((fen != null && setup != "1") || (setup == "1" && fen == null) || (setup != null && setup != "0" && setup != "1"))
                {
                    throw this.HeaderError(PgnDiagnosticCode.InvalidFen, setup != null ? "SetUp" : "FEN");
                }
                if (fen == null)
                {
                    return Position.GetStartingPosition();
                }
                try
                {
                    return PositionFile.ParseFen(fen);
                }
                catch (InvalidDataException)
                {
                    PgnToken location = this.locations["FEN"];
                    throw new PgnException(PgnDiagnosticCode.InvalidFen, location.Line, location.Column, token: fen);
                }
            }

            private PgnMetadata Metadata()
            {
                String date = this.Header("Date", "????.??.??");
                if (date == "?" && this.options.AllowLegacyTriglav)
                {
                    date = "????.??.??";
                    this.Warning(PgnDiagnosticCode.LegacyRepair, this.locations["Date"]);
                }
                if (!PgnProfile.ValidDate(date))
                {
                    throw this.HeaderError(PgnDiagnosticCode.InvalidMetadata, "Date");
                }
                return new PgnMetadata
                {
                    Event = this.Header("Event", "?"), Site = this.Header("Site", "?"), Date = date,
                    Round = this.Header("Round", "?"), White = this.Header("White", "?"), Black = this.Header("Black", "?"),
                    ExtraTags = this.headers.Where(static pair => !PgnProfile.ReservedTags.Contains(pair.Key)).ToDictionary(static pair => pair.Key, static pair => pair.Value, StringComparer.Ordinal),
                };
            }

            private PlayerType Player(String side, String name)
            {
                String kind = this.Header(side + "Type");
                if (kind != null && kind != "human" && kind != "program")
                {
                    throw this.HeaderError(PgnDiagnosticCode.InvalidMetadata, side + "Type");
                }
                return kind == "program" || (kind == null && name == "Triglav") ? PlayerType.Triglav : PlayerType.Player;
            }

            private void ReadNumber(Position position)
            {
                if (!Int32.TryParse(this.token.Value, NumberStyles.None, CultureInfo.InvariantCulture, out Int32 number) || number != position.FullMoveCounter)
                {
                    throw this.Error(PgnDiagnosticCode.InvalidNumber);
                }
                this.Advance();
                Int32 dots = 0;
                while (this.token.Kind == PgnTokenKind.Dot)
                {
                    dots++;
                    this.Advance();
                }
                if (dots != (position.OnMove == Colours.White ? 1 : 3) || this.token.Kind != PgnTokenKind.Word || PgnProfile.IsScore(this.token.Value))
                {
                    throw this.Error(PgnDiagnosticCode.InvalidNumber);
                }
            }

            private MoveStruct ResolveMove(Position position)
            {
                String san = this.token.Value;
                if (san.Contains('!') || san.Contains('?') || san.StartsWith("O-O", StringComparison.Ordinal) || san.StartsWith("0-0", StringComparison.Ordinal)
                    || Regex.IsMatch(san, @"=[RBNK]") || san == "e.p")
                {
                    throw this.Error(PgnDiagnosticCode.UnsupportedConstruct);
                }
                Match parsed = Regex.Match(san, @"^(?<piece>[KQRBN])?(?<from>[a-h]?[1-8]?)(?<capture>x?)(?<to>[a-h][1-8])(?<promotion>=Q)?(?<check>[+#]?)$");
                if (!parsed.Success)
                {
                    throw this.Error(PgnDiagnosticCode.IllegalMove);
                }
                String pieceLetter = parsed.Groups["piece"].Value;
                String destination = parsed.Groups["to"].Value;
                String disambiguation = parsed.Groups["from"].Value;
                Boolean capture = parsed.Groups["capture"].Value == "x";
                MoveSimulation[] candidates = position.GetMoves().Where(candidate =>
                {
                    MoveStruct move = candidate.MoveStruct.Move;
                    Pieces piece = position.GetPiece((Byte)move.From);
                    String family = piece switch
                    {
                        Pieces.King or Pieces.KingMoved => "K", Pieces.Queen or Pieces.QueenMoved => "Q",
                        Pieces.Rook => "R", Pieces.Bishop => "B", Pieces.Knight => "N", _ => String.Empty,
                    };
                    String source = new Coordinates((Byte)move.From).HumanReadableCoordinates;
                    return family == pieceLetter && new Coordinates((Byte)move.To).HumanReadableCoordinates == destination
                        && capture == position.IsFieldTaken((Byte)move.To)
                        && (disambiguation.Length == 0 || (disambiguation.Length == 1 ? source.Contains(disambiguation) : source == disambiguation));
                }).ToArray();
                MoveSimulation[] matches = candidates.Where(candidate => position.Clone().MoveDescriptive(candidate.MoveStruct.Move).AlgebraicNotation == san).ToArray();
                if (matches.Length != 1)
                {
                    throw this.Error(matches.Length > 1 || candidates.Length > 1 ? PgnDiagnosticCode.AmbiguousMove : PgnDiagnosticCode.IllegalMove);
                }
                return matches[0].MoveStruct.Move;
            }

            private String NormalizeResult(String result, PgnToken location)
            {
                if (result != null && this.options.AllowLegacyTriglav)
                {
                    String normalized = result.Replace('–', '-');
                    if (normalized != result || normalized.Length == 0)
                    {
                        this.Warning(PgnDiagnosticCode.LegacyRepair, location);
                    }
                    result = normalized.Length == 0 ? null : normalized;
                }
                if (result != null && !PgnProfile.IsScore(result))
                {
                    throw new PgnException(PgnDiagnosticCode.InvalidResult, location.Line, location.Column, this.ply, result);
                }
                return result;
            }

            internal PgnImportResult Read()
            {
                this.ReadHeaders();
                this.ValidateProfile();
                Position position = this.InitialPosition();
                PgnMetadata metadata = this.Metadata();
                GameDataSerializable game = new()
                {
                    StartingPosition = position.PositionDataSerializable, Metadata = metadata,
                    WhitePlayer = this.Player("White", metadata.White), BlackPlayer = this.Player("Black", metadata.Black),
                };
                using PositionStackWrapper history = PositionStackWrapper.New(position.OnMove);
                PositionEvaluator evaluator = PositionEvaluator.GetDefault();
                GameResult? terminal = PgnProfile.Terminal(position, history, evaluator);
                String marker = null;
                while (this.token.Kind != PgnTokenKind.End)
                {
                    if (this.token.Kind == PgnTokenKind.Number)
                    {
                        this.ReadNumber(position);
                    }
                    if (PgnProfile.IsScore(this.token.Value) || (this.options.AllowLegacyTriglav && this.token.Value is "1–0" or "0–1"))
                    {
                        marker = this.NormalizeResult(this.token.Value, this.token);
                        this.Advance();
                        if (this.token.Kind != PgnTokenKind.End)
                        {
                            throw this.Error(PgnDiagnosticCode.UnsupportedConstruct);
                        }
                        break;
                    }
                    if (this.token.Kind != PgnTokenKind.Word)
                    {
                        throw this.Error(PgnDiagnosticCode.Syntax);
                    }
                    if (this.ply >= this.options.MaxPlies || position.HalfmoveClock == Int32.MaxValue
                        || (position.OnMove == Colours.Black && position.FullMoveCounter == Int32.MaxValue))
                    {
                        throw this.Error(PgnDiagnosticCode.LimitExceeded);
                    }
                    if (terminal.HasValue)
                    {
                        throw this.Error(PgnDiagnosticCode.MovesAfterEnd);
                    }
                    MoveStruct move = this.ResolveMove(position);
                    history.Push(position.State);
                    position.Move(move);
                    game.AddMove(move, position.PositionDataSerializable);
                    this.ply++;
                    terminal = PgnProfile.Terminal(position, history, evaluator);
                    this.Advance();
                }
                String headerResult = this.NormalizeResult(this.Header("Result"), this.locations.GetValueOrDefault("Result", this.token));
                if (marker == null)
                {
                    if (!this.options.AllowLegacyTriglav)
                    {
                        throw this.Error(PgnDiagnosticCode.InvalidResult);
                    }
                    this.Warning(PgnDiagnosticCode.LegacyRepair, this.token);
                }
                if (headerResult != null && marker != null && headerResult != marker)
                {
                    throw this.Error(PgnDiagnosticCode.MismatchedResult);
                }
                String score = marker ?? headerResult ?? PgnProfile.Score(terminal ?? GameResult.Unknown);
                try
                {
                    game.Result = PgnProfile.ReadResult(score, this.Header("TriglavResult"), terminal);
                }
                catch (PgnException error)
                {
                    throw this.HeaderError(error.Diagnostic.Code, this.headers.ContainsKey("TriglavResult") ? "TriglavResult" : "Result");
                }
                String count = this.Header("PlyCount");
                if (count != null)
                {
                    if (!Int32.TryParse(count, NumberStyles.None, CultureInfo.InvariantCulture, out Int32 parsed))
                    {
                        throw this.HeaderError(PgnDiagnosticCode.InvalidMetadata, "PlyCount");
                    }
                    if (parsed != game.Moves.Count)
                    {
                        if (!this.options.AllowLegacyTriglav)
                        {
                            throw this.HeaderError(PgnDiagnosticCode.InvalidMetadata, "PlyCount");
                        }
                        this.Warning(PgnDiagnosticCode.LegacyRepair, this.locations["PlyCount"]);
                    }
                }
                return new PgnImportResult(game, position, this.warnings.AsReadOnly());
            }
        }
    }
}
