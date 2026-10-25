using System;
using System.Collections.Generic;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;

namespace Triglav.Chess.Engine.Persistence
{
    public sealed class GameImportOptions
    {
        public Int32 MaxBytes { get; init; } = 16 * 1024 * 1024;
        public Int32 MaxPlies { get; init; } = 20000;
        public Boolean AssumeTriglavVariant { get; init; }
        public Boolean AllowLegacyPgn { get; init; }
    }

    public enum GameImportWarningCode
    {
        Pgn,
        ReconstructedCounters,
    }

    public sealed record GameImportWarning(GameImportWarningCode Code, PgnDiagnostic PgnDiagnostic = null);

    public sealed record GameImportResult(GameDataSerializable Game, Position FinalPosition, IReadOnlyList<GameImportWarning> Warnings)
    {
        public PositionStackWrapper CreateHistory() => GameReplay.CreateHistory(this.Game);
    }
}
