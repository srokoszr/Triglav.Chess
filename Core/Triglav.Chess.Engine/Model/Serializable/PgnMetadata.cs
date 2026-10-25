using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Triglav.Chess.Engine.Model.Serializable
{
    [ProtoContract]
    public sealed class PgnMetadata
    {
        [ProtoMember(1)]
        public String Event { get; set; } = "?";
        [ProtoMember(2)]
        public String Site { get; set; } = "?";
        [ProtoMember(3)]
        public String Date { get; set; } = "????.??.??";
        [ProtoMember(4)]
        public String Round { get; set; } = "?";
        [ProtoMember(5)]
        public String White { get; set; } = "?";
        [ProtoMember(6)]
        public String Black { get; set; } = "?";
        [ProtoMember(7)]
        public Dictionary<String, String> ExtraTags { get; set; } = new(StringComparer.Ordinal);
    }
}
