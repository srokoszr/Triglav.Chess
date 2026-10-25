using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Triglav.Chess.Engine.KnownPieceLocations;
using Triglav.Chess.Engine.Model;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Util;
using Triglav.Chess.Interop;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Core
{
    //[StructLayout(LayoutKind.Explicit , Size = (8 * 5) + 8 + 8)]
    public sealed class Position : IEnumerable<PieceDataExtended>, ICloneable, IEquatable<Position>
    {
        #region const

        private const ulong pieceMask_0 = 0x0000_0004;

        private const ulong pieceMask_1 = 0x0000_0002;

        private const ulong pieceMask_2 = 0x0000_0001;

        private const ulong mask = 0x0000_0001;

        private const ulong zero = 0x0000_0000;

        private const byte Pawn = (byte)Pieces.Pawn;

        private const byte Rook = (byte)Pieces.Rook;

        private const byte Knight = (byte)Pieces.Knight;

        private const byte Bishop = (byte)Pieces.Bishop;

        private const byte King = (byte)Pieces.King;

        private const byte KingMoved = (byte)Pieces.KingMoved;

        private const byte Queen = (byte)Pieces.Queen;

        private const byte QueenMoved = (byte)Pieces.QueenMoved;

        private const ulong Black = 0x0000_0001;

        private const ulong White = 0x0000_0000;

        #endregion

        #region state fields

        public PositionData State { get; private set; }

        public Chess.Model.Structs.Interoperable.PositionKey PositionKey
            => new Chess.Model.Structs.Interoperable.PositionKey(this.State.PieceColours, this.State.Pieces_0, this.State.Pieces_1, this.State.Pieces_2);

        public int HalfmoveClock { get; private set; } = 0;

        public int FullMoveCounter { get; private set; } = 1;

        public int CountWhite => PopcountHelper.Popcount(this.State.WhitePieces);

        public int CountBlack => PopcountHelper.Popcount(this.State.PieceColours);

        public int Count => PopcountHelper.Popcount(this.State.TakenFields);

        #region King

        public bool HasWhiteKingMoved => this.GetPiece((byte)this.State.WhiteKingLocation) == Pieces.KingMoved;

        public bool HasBlackKingMoved => this.GetPiece((byte)this.State.BlackKingLocation) == Pieces.KingMoved;

        #endregion

        public Colours OnMove { get; set; }

        //public readonly ulong ULongHash;

        public Triglav.Chess.Model.Structs.PositionKey GetPositionKey()
        {
            return new Triglav.Chess.Model.Structs.PositionKey(this.State.Pieces_0, this.State.Pieces_1, this.State.Pieces_2, this.State.TakenFields, this.State.PieceColours);
        }

        public PositionDataSerializable PositionDataSerializable => new PositionDataSerializable(this.State, this.OnMove) { HalfmoveClock = this.HalfmoveClock, FullMoveCounter = this.FullMoveCounter };

        #endregion

        #region OptimisedPosition and Cache

        public bool CanBeOptimised => this.Count <= 6;

        public bool CanBeCached => false;//Count <= 10;
        /*
        public unsafe OptimisedPositionUpTo6 OptimisedPositionUpTo6
        {
            get
            {
                Span<PieceWithCoordinates> arr = stackalloc PieceWithCoordinates[Count];
                int pieceId = 0;

                PieceData* i_source = this._whitePiecesPtr;
                var end = i_source + this._countWhite;
                while (i_source < end)
                {
                    arr[pieceId++] = new PieceWithCoordinates(0, *i_source);
                }

                i_source = this._blackPiecesPtr;
                end = i_source + this._countBlack;
                while (i_source < end)
                {
                    arr[pieceId++] = new PieceWithCoordinates(1, *i_source);
                }

                return new OptimisedPositionUpTo6(arr, OnMove);
            }
        }
        */
        #endregion

        #region position manipulations

        public bool IsPieceColourWhite(byte field) => (this.State.PieceColours & (mask << field)) == zero;

        public bool IsPieceColourBlack(byte field) => (this.State.PieceColours & (mask << field)) != zero;

        public Colours GetPieceColour(byte field) => (Colours)((this.State.PieceColours & mask << field) >> field);

        public Colours GetPieceColour(Coordinates field) => this.GetPieceColour(field.SingleCoordinate);

        public Pieces GetPiece(byte field)
        {
            ulong mask = (ulong)1 << field;

            return (Pieces)((((this.State.Pieces_0 & mask) >> field) << 2) | (((this.State.Pieces_1 & mask) >> field) << 1) | ((this.State.Pieces_2 & mask) >> field));
        }

        public Pieces GetPiece(Coordinates field) => this.GetPiece(field.SingleCoordinate);

        public bool IsFieldTaken(byte field) => (this.State.TakenFields & (mask << field)) != zero;

        public bool IsFieldTaken(Coordinates field) => this.IsFieldTaken(field.SingleCoordinate);

        public FieldData Get(byte field) => !this.IsFieldTaken(field) ? default : new FieldData(true, this.GetPieceColour(field), this.GetPiece(field));

        public FieldData Get(Coordinates field) => this.Get(field.SingleCoordinate);

        private void Set(byte field, Colours colour, Pieces piece)
        {
            var maskToUse = mask << field;
            if((this.State.TakenFields & maskToUse) != 0)
            {
                return;//just silently ignore...
            }

            var pieceAsUInt64 = (ulong)piece;

            ulong pieceColours = this.State.PieceColours;
            int whiteKingLocation = this.State.WhiteKingLocation;
            int blackKingLocation = this.State.BlackKingLocation;
            var takenFields = this.State.TakenFields | maskToUse;

            if(colour == Colours.Black)
            {
                pieceColours |= maskToUse;
            }
            else
            {
                pieceColours &= ~maskToUse;
            }

            var pieces_0 = this.State.Pieces_0 & ~maskToUse;
            pieces_0 |= (pieceAsUInt64 & pieceMask_0) >> 2 << field;

            var pieces_1 = this.State.Pieces_1 & ~maskToUse;
            pieces_1 |= (pieceAsUInt64 & pieceMask_1) >> 1  << field;

            var pieces_2 = this.State.Pieces_2 & ~maskToUse;
            pieces_2 |= (pieceAsUInt64 & pieceMask_2) << field;

            if (piece == Pieces.King)
            {
                if (colour == Colours.White)
                {
                    whiteKingLocation = field;
                }
                else
                {
                    blackKingLocation = field;
                }
            }

            if (piece == Pieces.KingMoved)
            {
                if (colour == Colours.White)
                {
                    whiteKingLocation = field;
                }
                else
                {
                    blackKingLocation = field;
                }
            }

            var hash = this.State.Hash ^ ZobristTable.GetZobrist(piece, colour, field); 

            this.State = new PositionData(hash, takenFields, pieceColours & takenFields, pieces_0 & takenFields, pieces_1 & takenFields, pieces_2 & takenFields, whiteKingLocation, blackKingLocation);
        }

        public void Set(Colours colour, Pieces piece, Coordinates field) => this.Set(field.SingleCoordinate, colour, piece);

        public void SetEmpty(byte field)
        {
            var maskToUse = mask << field;
            if ((this.State.TakenFields & maskToUse) == 0)
            {
                return;//just silently ignore...
            }

            var piece = this.GetPiece(field);
            var colour = this.GetPieceColour(field);

            var hash = this.State.Hash ^ ZobristTable.GetZobrist(piece, colour, field);


            ulong pieceColours = this.State.PieceColours & ~maskToUse;
            var takenFields = this.State.TakenFields & ~maskToUse;

            var pieces_0 = this.State.Pieces_0 & ~maskToUse;
            var pieces_1 = this.State.Pieces_1 & ~maskToUse;
            var pieces_2 = this.State.Pieces_2 & ~maskToUse;

            this.State = new PositionData(hash, takenFields, pieceColours & takenFields, pieces_0 & takenFields, pieces_1 & takenFields, pieces_2 & takenFields, this.State.WhiteKingLocation, this.State.BlackKingLocation);
        }

        public void SetEmpty(Coordinates field) => this.SetEmpty(field.SingleCoordinate);

        public void Move(MoveStruct move)
        {
            var piece = this.GetPiece((byte)move.From);
            if(piece == Pieces.King)
            {
                piece = Pieces.KingMoved;
            }
            if (piece == Pieces.Queen)
            {
                piece = Pieces.QueenMoved;
            }

            if (piece == Pieces.Pawn)
            {
                if (this.OnMove == White)
                {
                    if (new Coordinates((byte)move.To).Row == 0)
                    {
                        piece = Pieces.Queen;
                    }
                }
                else
                {
                    if (new Coordinates((byte)move.To).Row == 7)
                    {
                        piece = Pieces.Queen;
                    }
                }
            }

            this.Move(new MoveWithPieceStruct(move, piece));
        }

        public MoveDescriptive MoveDescriptive(MoveStruct move)
        {
            bool isCapture = this.IsFieldTaken((byte)move.To);
            bool isCheck = false;
            bool isPromotion = false;
            bool isMate = false;

            var piece = this.GetPiece((byte)move.From);

            string disambiguitySuffix = String.Empty;
            var from = new Coordinates((byte)move.From);
            var column = from.HumanReadableCoordinates[0];
            var row = from.HumanReadableCoordinates[1];

            if (piece == Pieces.Pawn && isCapture)
            {
                disambiguitySuffix = column.ToString();
            }
            else
            {
                var competingMoves = this.GetMoves().Where(x => x.MoveStruct.Move.To == move.To).ToArray();
                if (competingMoves.Any())
                {
                    var filteredMoves = new List<MoveWithPieceStruct>();
                    foreach (var competingMove in competingMoves)
                    {
                        var competingMovePiece = this.GetPiece((byte)competingMove.MoveStruct.Move.From);
                        if (competingMovePiece == piece || ((piece == Pieces.Queen || piece == Pieces.QueenMoved)
                            && (competingMovePiece == Pieces.Queen || competingMovePiece == Pieces.QueenMoved)))
                        {
                            filteredMoves.Add(competingMove.MoveStruct);
                        }
                    }

                    if (filteredMoves.Count > 1)
                    {
                        var coordinatesList = filteredMoves.Select(
                            x => (From: x.Move.From, To: (Column: new Coordinates((byte)x.Move.From).HumanReadableCoordinates[0], Row: new Coordinates((byte)x.Move.From).HumanReadableCoordinates[1]))
                        ).ToList();

                        if (coordinatesList.All(x => x.To.Column != column || x.From == move.From))
                        {
                            disambiguitySuffix = column.ToString();
                        }
                        else if (coordinatesList.All(x => x.To.Row != row || x.From == move.From))
                        {
                            disambiguitySuffix = row.ToString();
                        }
                        else
                        {
                            disambiguitySuffix = from.HumanReadableCoordinates;
                        }
                    }
                }
            }

            if (piece == Pieces.King)
            {
                piece = Pieces.KingMoved;
            }
            if (piece == Pieces.Queen)
            {
                piece = Pieces.QueenMoved;
            }

            if (piece == Pieces.Pawn)
            {
                if (this.OnMove == White)
                {
                    if (new Coordinates((byte)move.To).Row == 0)
                    {
                        piece = Pieces.Queen;
                        isPromotion = true;
                    }
                }
                else
                {
                    if (new Coordinates((byte)move.To).Row == 7)
                    {
                        piece = Pieces.Queen;
                        isPromotion = true;
                    }
                }
            }

            this.Move(new MoveWithPieceStruct(move, piece));

            isCheck = this.OnMove == Colours.White ? this.IsWhiteKingChecked : this.IsBlackKingChecked;
            if(isCheck)
            {
                var terminal = PositionEvaluator.GetDefault().EvaluateTerminalPosition(this, 0);
                if (terminal.Result is EvaluationResult.WhiteWinMate or EvaluationResult.BlackWinMate)
                {
                    isMate = true;
                }
            }

            return new MoveDescriptive(move, this.OnMove, piece, isCapture, isCheck, isMate, isPromotion, disambiguitySuffix);
        }

        public void Move(MoveWithPieceStruct move)
        {
            if (this.OnMove == White)
            {
                this.MoveWhite(move);
            }
            else
            {
                this.MoveBlack(move);
            }
        }

        public void MoveWhite(MoveWithPieceStruct move)
        {
            this.HalfmoveClock++;
            var piece = this.GetPiece((byte)move.Move.From);
            if(piece == Pieces.Pawn)
            {
                this.HalfmoveClock = 0;
            }
            var isCapture = this.IsFieldTaken((byte)move.Move.To);
            if(isCapture)
            {
                this.HalfmoveClock = 0;
            }

            this.State = NativeInterface.MoveWhite(this.State, move).Position;
            this.OnMove = Colours.Black;
        }

        public void MoveBlack(MoveWithPieceStruct move)
        {
            this.HalfmoveClock++;
            this.FullMoveCounter++;
            var piece = this.GetPiece((byte)move.Move.From);
            if (piece == Pieces.Pawn)
            {
                this.HalfmoveClock = 0;
            }
            var isCapture = this.IsFieldTaken((byte)move.Move.To);
            if (isCapture)
            {
                this.HalfmoveClock = 0;
            }

            this.State = NativeInterface.MoveBlack(this.State, move).Position;
            this.OnMove = Colours.White;
        }


        /* for GUI
        public MoveDescriptive MoveDescriptive(MoveStruct move)
        {
            var data = Get(move.From);

            var isCapture = Move(move);

            bool isCheck = this.IsKingChecked();

            return new MoveDescriptive(move, (Colours)data.PieceColour, data.Piece, isCapture, isCheck);
        }
        */
        #endregion

        #region Extensions

        #region Is King in check

        public bool IsWhiteKingChecked => this.GetAttackedFieldsBlack()[this.State.WhiteKingLocation];

        public bool IsBlackKingChecked => this.GetAttackedFieldsWhite()[this.State.BlackKingLocation];

        #endregion

        public bool IsKingAlone => this.GetPiecesCount(this.OnMove) == 1;

        public bool IsWhiteKingAlone => this.CountWhite == 1;

        public bool IsBlackKingAlone => this.CountBlack == 1;

        #region MovesEnuemrator

        public unsafe int GetMovesNoCheckValidation(MoveWithPieceStruct* result) => this.OnMove == Colours.White ? this.GetMovesNoCheckValidationWhite(result) : this.GetMovesNoCheckValidationBlack(result);

        public unsafe int GetMovesNoCheckValidationWhite(MoveWithPieceStruct* result) => NativeInterface.GetMovesNoCheckValidationWhite(this.State, result);

        public unsafe int GetMovesNoCheckValidationBlack(MoveWithPieceStruct* result) => NativeInterface.GetMovesNoCheckValidationBlack(this.State, result);

        public OptimisedSetOfFields GetAttackedFields(Colours onMove) => this.OnMove == Colours.White ? this.GetAttackedFieldsWhite() : this.GetAttackedFieldsBlack();

        public OptimisedSetOfFields GetAttackedFieldsWhite() => new OptimisedSetOfFields(PositionNativeInterface.GetAttackedFieldsWhite(this.State));

        public OptimisedSetOfFields GetAttackedFieldsBlack() => new OptimisedSetOfFields(PositionNativeInterface.GetAttackedFieldsBlack(this.State));

        public OptimisedSetOfFields GetAttackedFields_BitwiseLoop(Colours onMove) => this.OnMove == Colours.White ? this.GetAttackedFieldsWhite_BitwiseLoop() : this.GetAttackedFieldsBlack_BitwiseLoop();

        public OptimisedSetOfFields GetAttackedFieldsWhite_BitwiseLoop() => new OptimisedSetOfFields(PositionNativeInterface.GetAttackedFieldsWhite_BitwiseLoop(this.State));

        public OptimisedSetOfFields GetAttackedFieldsBlack_BitwiseLoop() => new OptimisedSetOfFields(PositionNativeInterface.GetAttackedFieldsBlack_BitwiseLoop(this.State));


        public int AvailableMovesCount => this.GetMoves().Length;

        public unsafe MoveSimulation[] GetMoves()
        {
            MoveSimulation* moves = stackalloc MoveSimulation[MovesEnumerators.MaxMovesInPosition];

            var resultCount = this.GetMoves(moves);

            var result = new MoveSimulation[resultCount];

            for(int  i = 0; i < resultCount; i++)
            {
                result[i] = moves[i];
            }

            return result;
        }

        public unsafe int GetMoves(MoveSimulation* simulations) => this.OnMove == Colours.White? this.GetMovesWhite(simulations) : this.GetMovesBlack(simulations);

        public unsafe int GetMovesWhite(MoveSimulation* simulations) => PositionNativeInterface.GetMovesWhite(this.State, simulations);

        public unsafe int GetMovesBlack(MoveSimulation* simulations) => PositionNativeInterface.GetMovesBlack(this.State, simulations);

        #endregion

        #endregion

        #region constructors

        public Position()
        {
            this.State = new PositionData();
            this.OnMove = Colours.White;
        }

        public Position(PositionData state, Colours onMove)
        {
            this.State = state;
            this.OnMove = onMove;
        }

        public Position(OptimisedPositionUpTo6 optimised) : this()
        {
            this.OnMove = (Colours)optimised.OnMove;
            var (whitePieces, blackPieces) = optimised.PiecesArr;
            for (int i = 0; i < whitePieces.Length; i++)
            {
                this.Set(whitePieces[i].Field, Colours.White, whitePieces[i].Piece);
            }
            for (int i = 0; i < blackPieces.Length; i++)
            {
                this.Set(blackPieces[i].Field, Colours.Black, blackPieces[i].Piece);
            }
        }

        public static Position ReconstructPosition(PositionData positionData, Colours onMove)
        {
            var tmpPosition = new Position(positionData, onMove);

            var pieces = tmpPosition.ToArray().ToList();
            if (!pieces.Any(x => x.Piece == (Pieces)KingMoved && x.Colour == (Colours)White))
            {
                pieces.Add(new PieceDataExtended(White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition));
            }
            if (!pieces.Any(x => x.Piece == (Pieces)KingMoved && x.Colour == (Colours)Black))
            {
                pieces.Add(new PieceDataExtended((Colours)Black, Pieces.King, KnownKingPositions.BlackKingStartingPosition));
            }

            var result = new Position();
            result.OnMove = onMove;

            foreach (var piece in pieces)
            {
                result.Set(piece.Colour, piece.Piece, piece.Location);
            }

            return result;
        }

        public static PositionData GetPositionData(Triglav.Chess.Model.Structs.Interoperable.PositionKey positionKey, Colours onMove)
        {
            var _takenFields = PositionNativeInterface.GetTakenFields(positionKey);

            var tmpPositionData = new PositionData(0, _takenFields, positionKey.PieceColours, positionKey.Pieces_0, positionKey.Pieces_1, positionKey.Pieces_2, 0, 0);

            var tmpPosition = ReconstructPosition(tmpPositionData, onMove);

            return tmpPosition.State;
        }

        public static (MoveWithPieceStruct Move, Colours Colour) ReconstructMove(Triglav.Chess.Model.Structs.Interoperable.PositionKey positionKeyFrom, Triglav.Chess.Model.Structs.Interoperable.PositionKey positionKeyTo)
        {
            var positionFrom = GetPositionData(positionKeyFrom, Colours.White/*doesn't matter here*/);
            var move = PositionNativeInterface.GetMoveBasedOnTargetPositionKey(positionFrom, positionKeyTo);

            Colours colour = (Colours)((mask << move.Move.From) & positionFrom.PieceColours);

            return (move, colour);
        }

        public static Position GetPosition(PositionDataSerializable positionDataSerializable)
        {
            Position result = ReconstructPosition(positionDataSerializable.PositionData, (Colours)positionDataSerializable.OnMove);
            result.HalfmoveClock = positionDataSerializable.HalfmoveClock;
            result.FullMoveCounter = positionDataSerializable.FullMoveCounter;
            return result;
        }

        #endregion

        #region IEnumerable

        public IEnumerator<PieceDataExtended> GetEnumerator()
        {
            var arr = this.ToArray();
            foreach (var value in arr)
            {
                yield return value;
            }
        }

        public unsafe PieceDataExtended[] ToArray()
        {
            var result = new PieceDataExtended[this.Count];
            int resultIndex = 0;
            ulong mask = 1;
            for (byte i = 0; i < 64; i++, mask <<= 1)
            {
                var pieceLocation = new Coordinates(i);
                if (this.IsFieldTaken(i) && this.IsPieceColourWhite(i))
                {
                    var piece = this.GetPiece(i);

                    result[resultIndex++] = new PieceDataExtended(Colours.White, piece, pieceLocation);
                }
            }
            mask = 1;
            for (byte i = 0; i < 64; i++, mask <<= 1)
            {
                var pieceLocation = new Coordinates(i);
                if (this.IsFieldTaken(i) && this.IsPieceColourBlack(i))
                {
                    var piece = this.GetPiece(i);

                    result[resultIndex++] = new PieceDataExtended(Colours.Black, piece, pieceLocation);
                }
            }
            return result;
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public void Add(Colours colour, Pieces piece, Coordinates coordinates) => this.Set(colour, piece, coordinates);

        public void AddRange(IEnumerable<(Colours colour, Pieces piece, Coordinates coordinates)> values)
        {
            foreach (var value in values)
            {
                this.Set(value.colour, value.piece, value.coordinates);
            }
        }

        public int GetPiecesCount(byte colour) => colour == ColourConstants.White ? this.CountWhite : this.CountBlack;

        public int GetPiecesCount(Colours colour) => colour == Colours.White ? this.CountWhite : this.CountBlack;

        #endregion

        #region StartingPosition

        private static PositionData GetStartingPositionData() => new Position()
        {
            { Colours.White, Pieces.Rook, "a1"},
            { Colours.White, Pieces.Rook, "h1"},
            { Colours.White, Pieces.Knight, "b1"},
            { Colours.White, Pieces.Knight, "g1"},
            { Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            { Colours.White, Pieces.Queen, KnownQueenPositions.WhiteQueenStartingPosition},
            { Colours.White, Pieces.Bishop, "f1"},
            { Colours.White, Pieces.Bishop, "c1"},

            { Colours.White, Pieces.Pawn, "d2"},
            { Colours.White, Pieces.Pawn, "e2"},
            { Colours.White, Pieces.Pawn, "c2"},
            { Colours.White, Pieces.Pawn, "f2"},
            { Colours.White, Pieces.Pawn, "b2"},
            { Colours.White, Pieces.Pawn, "g2"},
            { Colours.White, Pieces.Pawn, "a2"},
            { Colours.White, Pieces.Pawn, "h2"},

            { Colours.Black, Pieces.Rook, "a8"},
            { Colours.Black, Pieces.Rook, "h8"},
            { Colours.Black, Pieces.Knight, "b8"},
            { Colours.Black, Pieces.Knight, "g8"},
            { Colours.Black, Pieces.King, KnownKingPositions.BlackKingStartingPosition},
            { Colours.Black, Pieces.Queen, KnownQueenPositions.BlackQueenStartingPosition},
            { Colours.Black, Pieces.Bishop, "f8"},
            { Colours.Black, Pieces.Bishop, "c8"},

            { Colours.Black, Pieces.Pawn, "d7"},
            { Colours.Black, Pieces.Pawn, "e7"},
            { Colours.Black, Pieces.Pawn, "c7"},
            { Colours.Black, Pieces.Pawn, "f7"},
            { Colours.Black, Pieces.Pawn, "b7"},
            { Colours.Black, Pieces.Pawn, "g7"},
            { Colours.Black, Pieces.Pawn, "a7"},
            { Colours.Black, Pieces.Pawn, "h7"},
        }.State;

        internal static readonly PositionData StartingPositionData = GetStartingPositionData();

        public static Position GetStartingPosition() => new Position(StartingPositionData, Colours.White);

        private static PositionData GetEmptyWithKingsNotMovedData() => new Position()
        {
            {Colours.White, Pieces.King, KnownKingPositions.WhiteKingStartingPosition},
            {Colours.Black, Pieces.King, KnownKingPositions.BlackKingStartingPosition},
        }.State;

        private static readonly PositionData EmptyWithKingsNotMovedData = GetEmptyWithKingsNotMovedData();

        private static PositionData GetEmptyWithKingsData() => new Position()
        {
            {Colours.White, Pieces.KingMoved, KnownKingPositions.WhiteKingStartingPosition},
            {Colours.Black, Pieces.KingMoved, KnownKingPositions.BlackKingStartingPosition},
        }.State;

        private static readonly PositionData EmptyWithKingsData = GetEmptyWithKingsData();

        public static Position GetEmptyWithKings() => new Position(EmptyWithKingsData, Colours.White);

        public static Position GetEmptyWithKingsNotMoved() => new Position(EmptyWithKingsNotMovedData, Colours.White);

        #endregion

        #region ToString

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine("   | a| b| c| d| e| f| g| h|   ".ToUpper());
            sb.AppendLine("-------------------------------");
            char row = '8';
            for (byte i = 0; i < 8; i++)
            {
                sb.Append(" " + row + " ");
                sb.Append("|");
                for (byte j = 0; j < 8; j++)
                {
                    var data = this.Get(new Coordinates(i, j));
                    string toAdd = "  ";
                    if (data.IsTaken)
                    {
                        toAdd = data.PieceColour == ColourConstants.White ? "w" : "b";
                        switch (data.Piece)
                        {
                            case Pieces.Pawn:
                                {
                                    toAdd += "P";
                                    break;
                                }
                            case Pieces.Rook:
                                {
                                    toAdd += "R";
                                    break;
                                }
                            case Pieces.Knight:
                                {
                                    toAdd += "N";
                                    break;
                                }
                            case Pieces.Bishop:
                                {
                                    toAdd += "B";
                                    break;
                                }
                            case Pieces.King:
                                {
                                    toAdd += "K";
                                    break;
                                }
                            case Pieces.KingMoved:
                                {
                                    toAdd += "k";
                                    break;
                                }
                            case Pieces.Queen:
                                {
                                    toAdd += "Q";
                                    break;
                                }
                            case Pieces.QueenMoved:
                                {
                                    toAdd += "q";
                                    break;
                                }
                        }

                    }

                    sb.Append(toAdd);

                    sb.Append("|");
                }
                sb.Append(" " + row + " ");
                row--;

                sb.AppendLine();
                sb.AppendLine("-------------------------------");
            }
            sb.AppendLine("   | a| b| c| d| e| f| g| h|   ".ToUpper());

            return sb.ToString();
        }

        public string TakenFieldsAsBinary
        {
            get
            {
                var data = this.State.TakenFields;
                string result = string.Empty;
                while (data > 0)
                {
                    byte @byte = (byte)(data & 0xFF);
                    result = Convert.ToString(@byte, 2).PadLeft(8, '0') + '_' + result;
                    data >>= 8;
                }

                result = result.TrimEnd('_');

                return "0b" + result;
            }
        }

        public string PieceColoursAsBinary
        {
            get
            {
                var data = this.State.PieceColours;
                string result = string.Empty;
                while (data > 0)
                {
                    byte @byte = (byte)(data & 0xFF);
                    result = Convert.ToString(@byte, 2).PadLeft(8, '0') + '_' + result;
                    data >>= 8;
                }

                result = result.TrimEnd('_');

                return "0b" + result;
            }
        }

        public string PiecesAsBinary
        {
            get
            {
                var sb = new StringBuilder();
                var data = this.State.Pieces_0;
                string result = string.Empty;
                while (data > 0)
                {
                    byte @byte = (byte)(data & 0xFF);
                    result = Convert.ToString(@byte, 2).PadLeft(8, '0') + '_' + result;
                    data >>= 8;
                }

                result = result.TrimEnd('_');

                sb.AppendLine("0b" + result);
                data = this.State.Pieces_1;
                result = string.Empty;
                while (data > 0)
                {
                    byte @byte = (byte)(data & 0xFF);
                    result = Convert.ToString(@byte, 2).PadLeft(8, '0') + '_' + result;
                    data >>= 8;
                }

                result = result.TrimEnd('_');

                sb.AppendLine("0b" + result);
                data = this.State.Pieces_2;
                result = string.Empty;
                while (data > 0)
                {
                    byte @byte = (byte)(data & 0xFF);
                    result = Convert.ToString(@byte, 2).PadLeft(8, '0') + '_' + result;
                    data >>= 8;
                }

                result = result.TrimEnd('_');

                sb.AppendLine("0b" + result);


                return sb.ToString();
            }
        }

        #endregion

        public string FEN_Notation
        {
            get
            {
                var result = new StringBuilder();
                for (int row = 0; row <= 7; row++)
                {
                    int countEmpty = 0;
                    for(int col = 0;  col <= 7; col++)
                    {
                        var coordinates = new Coordinates(row, col);
                        if (!this.IsFieldTaken(coordinates))
                        {
                            countEmpty++;
                            if(col == 7)
                            {
                                result.Append(countEmpty.ToString());
                            }
                        }
                        else
                        {
                            if(countEmpty > 0)
                            {
                                result.Append(countEmpty.ToString());
                            }
                            countEmpty = 0;

                            var piece = this.GetPiece(coordinates);
                            var pieceColour = this.GetPieceColour(coordinates);

                            var pieceLetter = piece switch
                            {
                                Pieces.King => "K" ,
                                Pieces.KingMoved => "K",
                                Pieces.Knight => "N",
                                Pieces.Bishop => "B",
                                Pieces.Queen => "Q",
                                Pieces.QueenMoved => "Q",
                                Pieces.Pawn => "P",
                                Pieces.Rook => "R",
                                _ => String.Empty,
                            };

                            if (pieceColour == Colours.Black)
                            {
                                pieceLetter = pieceLetter.ToLower();
                            }
                            else
                            {
                                pieceLetter = pieceLetter.ToUpper();
                            }

                            result.Append(pieceLetter);
                        }
                    }

                    if(row != 7)
                    {
                        result.Append("/");
                    }
                }

                string kingMoved = (this.HasWhiteKingMoved, this.HasBlackKingMoved) switch
                {
                    (true, true) => "-",
                    (true, false) => "k",
                    (false, true) => "K",
                    (false, false) => "Kk" 
                };

                var hasWhiteQueenNotMoved = this.GetPiece(KnownQueenPositions.WhiteQueenStartingPosition) == Pieces.Queen
                    && this.GetPieceColour(KnownQueenPositions.WhiteQueenStartingPosition) == Colours.White;
                var hasBlackQueenNotMoved = this.GetPiece(KnownQueenPositions.BlackQueenStartingPosition) == Pieces.Queen
                    && this.GetPieceColour(KnownQueenPositions.BlackQueenStartingPosition) == Colours.Black;

                if (hasWhiteQueenNotMoved)
                {
                    kingMoved = "Q" + kingMoved.Replace("-", String.Empty);
                }
                if (hasBlackQueenNotMoved)
                {
                    kingMoved = kingMoved.Replace("-", String.Empty) + "q";
                }

                // Medieval chess has no en passant: its field records moved queens away from home.
                String[] movedQueens = this.ToArray().Where(x => x.Piece == Pieces.QueenMoved
                    && x.Location != (x.Colour == Colours.White ? KnownQueenPositions.WhiteQueenStartingPosition : KnownQueenPositions.BlackQueenStartingPosition))
                    .Select(x => x.Location.HumanReadableCoordinates).ToArray();
                String queenState = movedQueens.Length == 0 ? "-" : String.Join(",", movedQueens);
                result.Append($" {(this.OnMove == White ? "w" : "b")} {kingMoved} {queenState} {this.HalfmoveClock} {this.FullMoveCounter}");

                return result.ToString();
            }
        }

        public void Save(Stream stream)
        {
            var data = this.PositionDataSerializable;
            ProtoBuf.Serializer.Serialize(stream, data);
        }

        public static Position Deserialize(Stream stream)
        {
            var data = ProtoBuf.Serializer.Deserialize<PositionDataSerializable>(stream);
            return GetPosition(data);
        }

        public static Position ReadFEN(string fen)
        {
            var splitFEN = fen.Trim().Split(' ', options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            var piecesSegment = splitFEN[0];
            var onMoveSegment = splitFEN[1];
            var specialMoveSegment = splitFEN[2];
            var halfMoveClockSegment = splitFEN[4];
            var fullMoveCounterSegment = splitFEN[5];

            var onMove = onMoveSegment.ToLower() == "w" ? Colours.White : Colours.Black;
            bool hasWhiteKingMoved = !specialMoveSegment.Contains("K");
            bool hasBlackKingMoved = !specialMoveSegment.Contains("k");
            bool hasWhiteQueenMoved = !specialMoveSegment.Contains("Q");
            bool hasBlackQueenMoved = !specialMoveSegment.Contains("q");

            var halfMoveClock = int.Parse(halfMoveClockSegment);
            var fullMoveCounter = int.Parse(fullMoveCounterSegment);

            var result = new Position
            {
                OnMove = onMove,
                HalfmoveClock = halfMoveClock,
                FullMoveCounter = fullMoveCounter,
            };

            var piecesRows = piecesSegment.Split('/', options: StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < 8; i++)
            {
                int row = 8 - i;

                var pieceRow = piecesRows[i];
                if(pieceRow == "8")
                {
                    continue;
                }

                int currentColumn = 0;
                for(int j = 0; j < pieceRow.Length; j++)
                {
                    string c = pieceRow[j].ToString();
                    if(int.TryParse(c, out int emptyFields))
                    {
                        currentColumn += emptyFields;
                    }
                    else
                    {
                        var coordinates = new Coordinates(i, currentColumn);
                        switch(c)
                        {
                            case "K":
                                {
                                    result.Add(Colours.White, hasWhiteKingMoved ? Pieces.KingMoved : Pieces.King, coordinates);
                                    break;
                                }
                            case "N":
                                {
                                    result.Add(Colours.White, Pieces.Knight, coordinates);
                                    break;
                                }
                            case "B":
                                {
                                    result.Add(Colours.White, Pieces.Bishop, coordinates);
                                    break;
                                }
                            case "Q":
                                {
                                    if(coordinates == KnownQueenPositions.WhiteQueenStartingPosition)
                                    {
                                        if(hasWhiteQueenMoved)
                                        {
                                            result.Add(Colours.White, Pieces.QueenMoved, coordinates);
                                        }
                                        else
                                        {
                                            result.Add(Colours.White, Pieces.Queen, coordinates);
                                        }
                                        break;
                                    }
                                    result.Add(Colours.White, Pieces.Queen, coordinates);
                                    break;
                                }
                            case "P":
                                {
                                    result.Add(Colours.White, Pieces.Pawn, coordinates);
                                    break;
                                }
                            case "R":
                                {
                                    result.Add(Colours.White, Pieces.Rook, coordinates);
                                    break;
                                }
                            case "k":
                                {
                                    result.Add(Colours.Black, hasBlackKingMoved ? Pieces.KingMoved : Pieces.King, coordinates);
                                    break;
                                }
                            case "n":
                                {
                                    result.Add(Colours.Black, Pieces.Knight, coordinates);
                                    break;
                                }
                            case "b":
                                {
                                    result.Add(Colours.Black, Pieces.Bishop, coordinates);
                                    break;
                                }
                            case "q":
                                {
                                    if (coordinates == KnownQueenPositions.BlackQueenStartingPosition)
                                    {
                                        if (hasBlackQueenMoved)
                                        {
                                            result.Add(Colours.Black, Pieces.QueenMoved, coordinates);
                                        }
                                        else
                                        {
                                            result.Add(Colours.Black, Pieces.Queen, coordinates);
                                        }
                                        break;
                                    }
                                    result.Add(Colours.Black, Pieces.Queen, coordinates);
                                    break;
                                }
                            case "p":
                                {
                                    result.Add(Colours.Black, Pieces.Pawn, coordinates);
                                    break;
                                }
                            case "r":
                                {
                                    result.Add(Colours.Black, Pieces.Rook, coordinates);
                                    break;
                                }
                        }

                        currentColumn++;
                    }
                }
            }

            if (splitFEN[3] != "-")
            {
                String[] squares = splitFEN[3].Split(',');
                if (squares.Distinct().Count() != squares.Length)
                {
                    throw new FormatException("Duplicate queen-state square.");
                }
                foreach (String square in squares)
                {
                    if (square.Length != 2 || square[0] < 'a' || square[0] > 'h' || square[1] < '1' || square[1] > '8'
                        || !result.IsFieldTaken(square) || (result.GetPiece(square) != Pieces.Queen && result.GetPiece(square) != Pieces.QueenMoved))
                    {
                        throw new FormatException("The fourth FEN field must identify occupied queen squares or be '-'.");
                    }
                    Colours colour = result.GetPieceColour(square);
                    if (square == (colour == Colours.White ? "d1" : "d8"))
                    {
                        throw new FormatException("Home-square queen rights belong in the third FEN field.");
                    }
                    result.SetEmpty(square);
                    result.Add(colour, Pieces.QueenMoved, square);
                }
            }
            return result;
        }

        Object ICloneable.Clone() => this.Clone();
        
        public Position Clone() => new Position(this.State, this.OnMove) { HalfmoveClock = this.HalfmoveClock, FullMoveCounter = this.FullMoveCounter };

        public Boolean Equals(Position other) => this.State.Equals(other.State) && this.OnMove == other.OnMove;

        public override Boolean Equals(Object obj) => this.Equals((Position)obj);
    }
}
