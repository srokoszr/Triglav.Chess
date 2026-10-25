using System;
using System.Collections.Generic;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Persistence;

namespace Triglav.Chess.Engine.Notation
{
    public sealed class PgnImportOptions
    {
        public Boolean AssumeTriglavVariant { get; init; }
        public Boolean AllowLegacyTriglav { get; init; }
        public Int32 MaxBytes { get; init; } = 16 * 1024 * 1024;
        public Int32 MaxPlies { get; init; } = 20000;
    }

    public sealed record PgnImportResult(GameDataSerializable Game, Position FinalPosition, IReadOnlyList<PgnDiagnostic> Warnings)
    {
        public PositionStackWrapper CreateHistory() => GameReplay.CreateHistory(this.Game);
    }
}
