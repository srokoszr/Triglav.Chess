using System;
using System.Globalization;
using System.IO;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Notation;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI
{
    internal static class UiText
    {
        internal static String GameResultText(GameResult result) => result switch
        {
            GameResult.Draw => Resources.DrawResult,
            GameResult.DrawAgreed => Resources.AgreedDrawResult,
            GameResult.DrawThreefoldRepetition => Resources.RepetitionDrawResult,
            GameResult.WhiteWin => Resources.WhiteWinResult,
            GameResult.BlackWin => Resources.BlackWinResult,
            GameResult.WhiteWinStalemate => Resources.WhiteStalemateResult,
            GameResult.WhiteWinNakedMate => Resources.WhiteNakedMateResult,
            GameResult.WhiteWinMate => Resources.WhiteMateResult,
            GameResult.WhiteWinResign => Resources.WhiteResignResult,
            GameResult.BlackWinStalemate => Resources.BlackStalemateResult,
            GameResult.BlackWinNakedMate => Resources.BlackNakedMateResult,
            GameResult.BlackWinMate => Resources.BlackMateResult,
            GameResult.BlackWinResign => Resources.BlackResignResult,
            GameResult.Error => Resources.GameErrorResult,
            _ => Resources.UnfinishedResult,
        };

        private static String EvaluationResultText(EvaluationResult result) => result switch
        {
            EvaluationResult.WhiteWinStalemate => Resources.WhiteStalemateResult,
            EvaluationResult.WhiteWinNakedMate => Resources.WhiteNakedMateResult,
            EvaluationResult.WhiteWinMate => Resources.WhiteMateResult,
            EvaluationResult.BlackWinStalemate => Resources.BlackStalemateResult,
            EvaluationResult.BlackWinNakedMate => Resources.BlackNakedMateResult,
            EvaluationResult.BlackWinMate => Resources.BlackMateResult,
            _ => Resources.DrawResult,
        };

        internal static String EvaluationText(EvaluationWrapper evaluation, Boolean winningChances = false)
        {
            return evaluation.IsTerminal
                ? String.Format(CultureInfo.CurrentCulture, Resources.TerminalEvaluation, EvaluationResultText(evaluation.Result), evaluation.InNMoves)
                : winningChances
                ? String.Format(CultureInfo.CurrentCulture, Resources.PercentageEvaluation, evaluation.WhiteWinChance)
                : String.Format(CultureInfo.CurrentCulture, Resources.CentipawnEvaluation, evaluation.CentiPawnsEvaluation);
        }

        internal static String PgnDiagnosticText(PgnDiagnostic diagnostic)
        {
            String description = Resources.ResourceManager.GetString("PgnError" + diagnostic.Code, Resources.Culture) ?? Resources.FileInvalid;
            return String.Format(CultureInfo.CurrentCulture, Resources.PgnDiagnosticFormat, description, diagnostic.Line, diagnostic.Column, diagnostic.Ply, diagnostic.Token);
        }

        internal static String GameImportWarningText(GameImportWarning warning) => warning.Code switch
        {
            GameImportWarningCode.Pgn => PgnDiagnosticText(warning.PgnDiagnostic),
            GameImportWarningCode.ReconstructedCounters => Resources.ReconstructedGameCounters,
            _ => Resources.FileInvalid,
        };

        internal static String ReplayErrorText(GameReplayException error)
        {
            String description = Resources.ResourceManager.GetString("GameReplay" + error.Error, Resources.Culture) ?? Resources.FileInvalid;
            return String.Format(CultureInfo.CurrentCulture, Resources.GameReplayDiagnosticFormat, description, error.Ply);
        }

        internal static String FileErrorText(Exception error, String path = null)
        {
            String message = error switch
            {
                GameReplayException replay => ReplayErrorText(replay),
                PgnException pgn => PgnDiagnosticText(pgn.Diagnostic),
                InvalidDataException or ProtoBuf.ProtoException => Resources.FileInvalid,
                UnauthorizedAccessException => Resources.FileAccessDenied,
                NotSupportedException => Resources.FileUnsupported,
                ArgumentException => Resources.FileInvalid,
                _ => Resources.FileIoError,
            };
            return String.IsNullOrEmpty(path)
                ? message
                : String.Format(CultureInfo.CurrentCulture, Resources.FileErrorWithPath, message, path);
        }
    }
}
