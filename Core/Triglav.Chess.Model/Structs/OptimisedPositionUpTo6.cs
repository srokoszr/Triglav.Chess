using Triglav.Chess.Model.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Triglav.Chess.Model.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 8, CharSet = CharSet.Unicode, Pack = 0)]
    public readonly struct OptimisedPositionUpTo6
    {
        public const ulong PiecesMask            = 0b11100000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        public const ulong ColourMask            = 0b00010000_00000000_00000000_00000000_00000000_00000000_00000000_00000000;
        public const ulong EvaluationMask        = 0b00000000_00000000_00000000_00000000_00000000_00000000_00000000_00000011;
        public const ulong WhiteKingPositionMask = 0b00001111_11000000_00000000_00000000_00000000_00000000_00000000_00000000;
        public const ulong WhiteKingMovedMask    = 0b00000000_00000000_10000000_00000000_00000000_00000000_00000000_00000000;
        public const ulong BlackKingPositionMask = 0b00000000_00111111_00000000_00000000_00000000_00000000_00000000_00000000;
        public const ulong BlackKingMovedMask    = 0b00000000_00000000_01000000_00000000_00000000_00000000_00000000_00000000;

        public static readonly ulong[] PiecePositionMasks = new ulong[4]
        {
                                                    0b00000000_00000000_00111111_11110000_00000000_00000000_00000000_00000000,
                                                    0b00000000_00000000_00000000_00001111_11111100_00000000_00000000_00000000,
                                                    0b00000000_00000000_00000000_00000000_00000011_11111111_00000000_00000000,
                                                    0b00000000_00000000_00000000_00000000_00000000_00000000_11111111_11000000,
        };

        public const int ColourShift            = 4 + 8 + 8 + 8 + 8 + 8 + 8 + 8;
        public const int PiecesShift            = 5 + 8 + 8 + 8 + 8 + 8 + 8 + 8;
        public const int WhiteKingPositionShift =     6 + 8 + 8 + 8 + 8 + 8 + 8;
        public const int BlackKingPositionShift =         8 + 8 + 8 + 8 + 8 + 8;

        public static readonly int[] PiecesShifts = new int[4]
        {
            4 + 8 + 8 + 8 + 8, 
                2 + 8 + 8 + 8,
                        8 + 8,
                            6,
        };

        [FieldOffset(0)]
        private readonly ulong _data;

        public ulong Data => this._data;

        [FieldOffset(7)]
        private readonly byte _firstByte;

        public byte FirstByte => this._firstByte;

        [FieldOffset(6)]
        private readonly ushort _firstTwoBytes;

        public ushort FirstTwoBytes => this._firstTwoBytes;

        public byte OnMove => (byte)((this._data & ColourMask) >> ColourShift);

        public ushort PiecesCount => (ushort)((this._data & PiecesMask) >> PiecesShift);

        public EvaluationResult Evaluation => (EvaluationResult)(this._data & EvaluationMask);

        public (PieceWithCoordinates[] White, PieceWithCoordinates[] Black) PiecesArr
        {
            get
            {
                List<PieceWithCoordinates> whitePieces = new List<PieceWithCoordinates>(5);
                List<PieceWithCoordinates> blackPieces = new List<PieceWithCoordinates>(5);

                whitePieces.Add(new PieceWithCoordinates(ColourConstants.White,
                    (this._data & WhiteKingMovedMask) == WhiteKingMovedMask
                        ? (byte)Pieces.KingMoved
                        : (byte)Pieces.King, (byte)((this._data & WhiteKingPositionMask) >> WhiteKingPositionShift)));
                blackPieces.Add(new PieceWithCoordinates(ColourConstants.Black,
                    (this._data & BlackKingMovedMask) == BlackKingMovedMask
                        ? (byte)Pieces.KingMoved
                        : (byte)Pieces.King, (byte)((this._data & BlackKingPositionMask) >> BlackKingPositionShift)));

                switch (this.PiecesCount)
                {
                    case 2:
                    {
                        break;
                    }
                    case 3:
                    {
                        var piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[0]) >> PiecesShifts[0]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        break;
                    }
                    case 4:
                    {
                        var piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[0]) >> PiecesShifts[0]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[1]) >> PiecesShifts[1]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        break;
                    }
                    case 5:
                    {
                        var piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[0]) >> PiecesShifts[0]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[1]) >> PiecesShifts[1]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[2]) >> PiecesShifts[2]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        break;
                    }
                    case 6:
                    {
                        var piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[0]) >> PiecesShifts[0]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[1]) >> PiecesShifts[1]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[2]) >> PiecesShifts[2]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        piece = new PieceWithCoordinates((ushort)((this._data & PiecePositionMasks[3]) >> PiecesShifts[3]));
                        if (piece.Colour == Colours.White)
                        {
                            whitePieces.Add(piece);
                        }
                        else
                        {
                            blackPieces.Add(piece);
                        }

                        break;
                    }
                    default: throw new ArgumentOutOfRangeException();
                }

                return (whitePieces.ToArray(), blackPieces.ToArray());
            }
        }

        public OptimisedPositionUpTo6(Span<PieceWithCoordinates> pieces, byte onMove)
        {
            if (pieces.Length < 2 || pieces.Length > 6)
            {
                throw new ArgumentOutOfRangeException();
            }

            this._data = onMove;

            int pieceId = 0;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].Piece == Pieces.King)
                {
                    if (pieces[i].Colour == Colours.White)
                    {
                        this._data |= (ulong)pieces[i].Field << WhiteKingPositionShift;
                    }
                    else
                    {
                        this._data |= (ulong)pieces[i].Field << BlackKingPositionShift;
                    }
                }
                else if (pieces[i].Piece == Pieces.KingMoved)
                {
                    if (pieces[i].Colour == Colours.White)
                    {
                        this._data |= (ulong)pieces[i].Field << WhiteKingPositionShift;
                        this._data |= WhiteKingMovedMask;
                    }
                    else
                    {
                        this._data |= (ulong)pieces[i].Field << BlackKingPositionShift;
                        this._data |= BlackKingMovedMask;
                    }
                }
                else
                {
                    this._data |= (ulong)pieces[i].Data << PiecesShifts[pieceId++];
                }
            }
        }

        public OptimisedPositionUpTo6(PieceWithCoordinates[] pieces, Colours onMove, EvaluationResult evaluation)
        {
            if (pieces.Length < 2 || pieces.Length > 6)
            {
                throw new ArgumentOutOfRangeException();
            }
            pieces = pieces.Order().ToArray();

            this._data = (ulong)onMove;

            this._data |= (ulong)evaluation;

            int pieceId = 0;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].Piece == Pieces.King)
                {
                    if (pieces[i].Colour == Colours.White)
                    {
                        this._data |= (ulong)pieces[i].Field << WhiteKingPositionShift;
                    }
                    else
                    {
                        this._data |= (ulong)pieces[i].Field << BlackKingPositionShift;
                    }
                }
                else if (pieces[i].Piece == Pieces.KingMoved)
                {
                    if (pieces[i].Colour == Colours.White)
                    {
                        this._data |= (ulong)pieces[i].Field << WhiteKingPositionShift;
                        this._data |= WhiteKingMovedMask;
                    }
                    else
                    {
                        this._data |= (ulong)pieces[i].Field << BlackKingPositionShift;
                        this._data |= BlackKingMovedMask;
                    }
                }
                else
                {
                    this._data |= (ulong)pieces[i].Data << PiecesShifts[pieceId++];
                }
            }
        }

        public OptimisedPositionUpTo6(ulong data)
        {
            this._data = data;
        }

        public OptimisedPositionUpTo6 StrippedOfEvaluation => new OptimisedPositionUpTo6(this._data & ~EvaluationMask);

        public string Binary
        {
            get
            {
                var data = this._data;
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

        public override string ToString() =>
            $"{this.Binary}{Environment.NewLine}WhitePieces:{string.Join("|", this.PiecesArr.White)}{Environment.NewLine}BlackPieces{string.Join("|", this.PiecesArr.Black)}";
    }
}
