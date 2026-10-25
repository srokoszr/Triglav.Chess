using System;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.App.GUI.Model
{
    public sealed class AnalysisTimeline
    {
        private readonly Position[] positions;
        private readonly MoveStruct[] moves;
        private readonly String[] notation;

        public AnalysisTimeline(GameImportResult imported)
        {
            ArgumentNullException.ThrowIfNull(imported);
            Int32 count = imported.Game.Moves.Count;
            this.positions = new Position[count + 1];
            this.moves = new MoveStruct[count];
            this.notation = new String[count];
            this.positions[0] = Position.GetPosition(imported.Game.StartingPosition);
            for (Int32 index = 0; index < count; index++)
            {
                this.moves[index] = imported.Game.Moves[index].Move;
                Position before = this.positions[index];
                String separator = before.OnMove == Colours.White ? ". " : "... ";
                this.notation[index] = $"{before.FullMoveCounter}{separator}{before.Clone().MoveDescriptive(this.moves[index]).AlgebraicNotation}";
                this.positions[index + 1] = Position.GetPosition(imported.Game.Moves[index].ResultingPosition);
            }
        }

        public Int32 MoveCount => this.moves.Length;
        public Int32 SelectedPly
        {
            get; private set;
        }
        public Boolean CanPrevious => this.SelectedPly > 0;
        public Boolean CanNext => this.SelectedPly < this.MoveCount;
        public Position SelectedPosition => this.positions[this.SelectedPly].Clone();
        public MoveStruct? SelectedMove => this.SelectedPly == 0 ? (MoveStruct?)null : this.moves[this.SelectedPly - 1];

        public String GetMoveText(Int32 index) => this.notation[index];

        public void Select(Int32 ply)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(ply);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(ply, this.MoveCount);
            this.SelectedPly = ply;
        }

        public void Previous() => this.Select(Math.Max(0, this.SelectedPly - 1));
        public void Next() => this.Select(Math.Min(this.MoveCount, this.SelectedPly + 1));
    }
}
