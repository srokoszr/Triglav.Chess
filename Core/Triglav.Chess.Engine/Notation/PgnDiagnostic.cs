using System;

namespace Triglav.Chess.Engine.Notation
{
    public enum PgnDiagnosticCode
    {
        Syntax, UnsupportedConstruct, UnknownVariant, MissingVariant, UnsupportedVersion,
        InvalidFen, InvalidMetadata, DuplicateHeader, InvalidNumber, IllegalMove, AmbiguousMove,
        InvalidResult, MismatchedResult, SnapshotMismatch, MovesAfterEnd, LimitExceeded,
        InvalidEncoding, LegacyRepair, CommentsDiscarded, UnmarkedVariant,
    }

    public sealed record PgnDiagnostic(PgnDiagnosticCode Code, Int32 Line = 0, Int32 Column = 0, Int32 Ply = 0, String Token = "");

    public sealed class PgnException : FormatException
    {
        public PgnException(PgnDiagnosticCode code, Int32 line = 0, Int32 column = 0, Int32 ply = 0, String token = "")
            : base($"{code} at {line}:{column}, ply {ply}: {token}")
        {
            this.Diagnostic = new PgnDiagnostic(code, line, column, ply, token);
        }

        public PgnDiagnostic Diagnostic { get; }
    }
}
