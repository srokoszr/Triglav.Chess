using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.Engine.Notation
{
    public static class PgnProfile
    {
        public const String Variant = "Triglav-Medieval";
        public const String Version = "1";
        internal static readonly HashSet<String> ReservedTags = new(StringComparer.OrdinalIgnoreCase)
        {
            "Event", "Site", "Date", "Round", "White", "Black", "Result", "SetUp", "FEN", "PlyCount",
            "Variant", "TriglavVariantVersion", "TriglavResult", "WhiteType", "BlackType",
        };

        private static readonly Dictionary<GameResult, String> Reasons = new()
        {
            [GameResult.Unknown] = "unfinished", [GameResult.Error] = "error",
            [GameResult.Draw] = "draw", [GameResult.DrawAgreed] = "draw-agreed", [GameResult.DrawThreefoldRepetition] = "draw-repetition",
            [GameResult.WhiteWinStalemate] = "white-stalemate", [GameResult.WhiteWinNakedMate] = "white-bare-king",
            [GameResult.WhiteWinMate] = "white-checkmate", [GameResult.WhiteWinResign] = "white-resignation", [GameResult.WhiteWin] = "white-win",
            [GameResult.BlackWinStalemate] = "black-stalemate", [GameResult.BlackWinNakedMate] = "black-bare-king",
            [GameResult.BlackWinMate] = "black-checkmate", [GameResult.BlackWinResign] = "black-resignation", [GameResult.BlackWin] = "black-win",
        };

        public static String Score(GameResult result) => result switch
        {
            GameResult.Unknown or GameResult.Error => "*",
            GameResult.Draw or GameResult.DrawAgreed or GameResult.DrawThreefoldRepetition => "1/2-1/2",
            GameResult.WhiteWin or GameResult.WhiteWinMate or GameResult.WhiteWinNakedMate or GameResult.WhiteWinStalemate or GameResult.WhiteWinResign => "1-0",
            GameResult.BlackWin or GameResult.BlackWinMate or GameResult.BlackWinNakedMate or GameResult.BlackWinStalemate or GameResult.BlackWinResign => "0-1",
            _ => throw new PgnException(PgnDiagnosticCode.InvalidResult),
        };

        public static String Reason(GameResult result) => Reasons.TryGetValue(result, out String reason)
            ? reason : throw new PgnException(PgnDiagnosticCode.InvalidResult);

        internal static Boolean IsScore(String text) => text is "1-0" or "0-1" or "1/2-1/2" or "*";

        internal static GameResult ReadResult(String score, String reason, GameResult? terminal)
        {
            if (!IsScore(score))
            {
                throw new PgnException(PgnDiagnosticCode.InvalidResult, token: score);
            }
            GameResult result = score switch
            {
                "1-0" => GameResult.WhiteWin,
                "0-1" => GameResult.BlackWin,
                "1/2-1/2" => GameResult.Draw,
                _ => GameResult.Unknown,
            };
            if (reason != null)
            {
                KeyValuePair<GameResult, String>[] matches = Reasons.Where(pair => pair.Value == reason).ToArray();
                if (matches.Length != 1 || Score(matches[0].Key) != score)
                {
                    throw new PgnException(PgnDiagnosticCode.MismatchedResult, token: reason);
                }
                result = matches[0].Key;
                Boolean requiresTerminal = result is GameResult.WhiteWinMate or GameResult.BlackWinMate
                    or GameResult.WhiteWinStalemate or GameResult.BlackWinStalemate
                    or GameResult.WhiteWinNakedMate or GameResult.BlackWinNakedMate or GameResult.DrawThreefoldRepetition;
                if (requiresTerminal && terminal != result)
                {
                    throw new PgnException(PgnDiagnosticCode.MismatchedResult, token: reason);
                }
            }
            if (terminal.HasValue && score != "*")
            {
                if (Score(terminal.Value) != score)
                {
                    throw new PgnException(PgnDiagnosticCode.MismatchedResult, token: score);
                }
                if (reason == null)
                {
                    result = terminal.Value;
                }
            }
            return result;
        }

        internal static GameResult? Terminal(Position position, PositionStackWrapper history, PositionEvaluator evaluator)
        {
            EvaluationWrapper evaluation = evaluator.EvaluateTerminalPosition(position, history, 0);
            if (!evaluation.IsTerminal)
            {
                return null;
            }
            return evaluation.Result switch
            {
                EvaluationResult.WhiteWinMate => GameResult.WhiteWinMate,
                EvaluationResult.BlackWinMate => GameResult.BlackWinMate,
                EvaluationResult.WhiteWinStalemate => GameResult.WhiteWinStalemate,
                EvaluationResult.BlackWinStalemate => GameResult.BlackWinStalemate,
                EvaluationResult.WhiteWinNakedMate => GameResult.WhiteWinNakedMate,
                EvaluationResult.BlackWinNakedMate => GameResult.BlackWinNakedMate,
                _ => history.IsThreefoldRepetition(position) ? GameResult.DrawThreefoldRepetition : GameResult.Draw,
            };
        }

        internal static Boolean SamePosition(Position first, Position second, Boolean counters = true) => first.OnMove == second.OnMove
            && first.State.TakenFields == second.State.TakenFields && first.State.PieceColours == second.State.PieceColours
            && first.State.Pieces_0 == second.State.Pieces_0 && first.State.Pieces_1 == second.State.Pieces_1 && first.State.Pieces_2 == second.State.Pieces_2
            && (!counters || (first.HalfmoveClock == second.HalfmoveClock && first.FullMoveCounter == second.FullMoveCounter));

        public static Boolean IsSetUp(Position position) => !SamePosition(position, Position.GetStartingPosition());

        public static String Tag(String name, String value)
        {
            ValidateTag(name, value);
            return "[" + name + " \"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"]";
        }

        internal static void ValidateTag(String name, String value)
        {
            if (name == null || !Regex.IsMatch(name, @"^[A-Za-z][A-Za-z0-9_]*$") || value == null
                || value.Length > 255 || value.Any(Char.IsControl))
            {
                throw new PgnException(PgnDiagnosticCode.InvalidMetadata, token: name ?? String.Empty);
            }
            try
            {
                _ = new UTF8Encoding(false, true).GetByteCount(value);
            }
            catch (EncoderFallbackException)
            {
                throw new PgnException(PgnDiagnosticCode.InvalidMetadata, token: name);
            }
        }

        internal static Boolean ValidDate(String date)
        {
            if (date == null || !Regex.IsMatch(date, @"^(?:[0-9]{4}|\?{4})\.(?:[0-9]{2}|\?{2})\.(?:[0-9]{2}|\?{2})$"))
            {
                return false;
            }
            String[] parts = date.Split('.');
            if ((parts[0] != "????" && Int32.Parse(parts[0], CultureInfo.InvariantCulture) < 1)
                || (parts[1] != "??" && (Int32.Parse(parts[1], CultureInfo.InvariantCulture) < 1 || Int32.Parse(parts[1], CultureInfo.InvariantCulture) > 12))
                || (parts[2] != "??" && (Int32.Parse(parts[2], CultureInfo.InvariantCulture) < 1 || Int32.Parse(parts[2], CultureInfo.InvariantCulture) > 31)))
            {
                return false;
            }
            if (parts[1] != "??" && parts[2] != "??")
            {
                Int32 year = parts[0] == "????" ? 2000 : Int32.Parse(parts[0], CultureInfo.InvariantCulture);
                if (Int32.Parse(parts[2], CultureInfo.InvariantCulture) > DateTime.DaysInMonth(year, Int32.Parse(parts[1], CultureInfo.InvariantCulture)))
                {
                    return false;
                }
            }
            return date.Contains('?') || DateTime.TryParseExact(date, "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }
    }
}
