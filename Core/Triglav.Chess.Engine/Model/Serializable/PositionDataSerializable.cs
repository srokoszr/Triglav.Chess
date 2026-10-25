using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Model.Serializable
{
    [ProtoContract]
    public class PositionDataSerializable
    {
        [ProtoMember(1)]
        public ulong TakenFields { get; set; }

        [ProtoMember(2)]
        public ulong PieceColours { get; set; }

        [ProtoMember(3)]
        public ulong Pieces_0 { get; set; }

        [ProtoMember(4)]
        public ulong Pieces_1 { get; set; }

        [ProtoMember(5)]
        public ulong Pieces_2 { get; set; }

        [ProtoMember(6)]
        public Int32 OnMove { get; set; }

        [ProtoMember(7)]
        public Int32 HalfmoveClock { get; set; }

        [ProtoMember(8)]
        [System.ComponentModel.DefaultValue(1)]
        public Int32 FullMoveCounter { get; set; } = 1;

        [ProtoIgnore]
        public PositionData PositionData => new PositionData(0, TakenFields, PieceColours, Pieces_0, Pieces_1, Pieces_2, 0, 0);

        /// <summary>
        /// For deserialization
        /// </summary>
        public PositionDataSerializable()
        {

        }

        public PositionDataSerializable(UInt64 takenFields, UInt64 pieceColours, UInt64 pieces_0, UInt64 pieces_1, UInt64 pieces_2, Colours onMove)
        {
            this.TakenFields = takenFields;
            this.PieceColours = pieceColours;
            this.Pieces_0 = pieces_0;
            this.Pieces_1 = pieces_1;
            this.Pieces_2 = pieces_2;
            this.OnMove = (int)onMove;
        }

        public PositionDataSerializable(PositionData positionData, Colours onMove)
        {
            this.TakenFields = positionData.TakenFields;
            this.PieceColours= positionData.PieceColours;
            this.Pieces_0 = positionData.Pieces_0;
            this.Pieces_1 = positionData.Pieces_1;
            this.Pieces_2 = positionData.Pieces_2;
            this.OnMove = (int)onMove;
        }
    }
}
