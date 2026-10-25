using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Extensions;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core
{
    public unsafe class PositionStackWrapper : IDisposable, IEnumerable<Position>
    {
        private bool disposedValue;

        private readonly Colours startingColour;

        public PositionStack* NativeStack
        {
            get; private set;
        }

        public int Count => this.NativeStack->Count;

        public int Capacity => this.NativeStack->Capacity;

        public unsafe Position[] Array
        {
            get
            {
                var arr = new Position[this.NativeStack->Count];

                for (int i = 0; i < this.NativeStack->Count; i++)
                {
                    var nativePosition = this.NativeStack->Array[i];
                    arr[i] = new Position(nativePosition, i % 2 == 0 ? this.startingColour : this.startingColour.GetOpposite());
                }

                return arr;
            }
        }

        private PositionStackWrapper(PositionStack* nativeStack, Colours startingColour)
        {
            this.NativeStack = nativeStack;
            this.startingColour = startingColour;
        }

        public static PositionStackWrapper New(Colours startingColour = Colours.White)
        {
            var nativeStack = PositionStackNativeInterface.New();
            return new PositionStackWrapper(nativeStack, startingColour);
        }

        public void Push(PositionData position)
        {
            PositionStackNativeInterface.Push(this.NativeStack, position);
        }

        // The stack contains prior positions in ply order, excluding the current position.
        public Boolean IsThreefoldRepetition(Position position)
        {
            Int32 matches = 1;
            PositionData current = position.State;
            // Two plies preserve the side to move, including games started with Black.
            for (Int32 index = this.Count - 2; index >= 0; index -= 2)
            {
                PositionData previous = this.NativeStack->Array[index];
                if (previous.TakenFields == current.TakenFields && previous.PieceColours == current.PieceColours
                    && previous.Pieces_0 == current.Pieces_0 && previous.Pieces_1 == current.Pieces_1 && previous.Pieces_2 == current.Pieces_2)
                {
                    matches++;
                    if (matches == 3)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public PositionData Pop()
        {
            return PositionStackNativeInterface.Pop(this.NativeStack);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                PositionStackNativeInterface.Free(this.NativeStack);
                // TODO: set large fields to null
                this.disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~PositionStackWrapper()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Array.GetEnumerator();
        }

        public IEnumerator<Position> GetEnumerator()
        {
            return ((IEnumerable<Position>)this.Array).GetEnumerator();
        }
    }
}
