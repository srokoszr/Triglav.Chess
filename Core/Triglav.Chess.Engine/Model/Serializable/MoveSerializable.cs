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
    public class MoveSerializable
    {
        [ProtoMember(1)]
        public int From { get; set; }

        [ProtoMember(2)]
        public int To { get; set; }

        [ProtoMember(3)]
        public PositionDataSerializable ResultingPosition { get; set; }

        [ProtoIgnore]
        public MoveStruct Move => new MoveStruct((byte)this.From, (byte)this.To);

        /// <summary>
        /// For deserialization
        /// </summary>
        public MoveSerializable()
        {

        }

        public MoveSerializable(MoveStruct move)
        {
            this.From = move.From;
            this.To = move.To;
        }

        public MoveSerializable(MoveStruct move, PositionDataSerializable position) : this(move)
        {
            this.ResultingPosition = position;
        }
    }
}
