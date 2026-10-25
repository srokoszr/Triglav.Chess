using ProtoBuf;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.Engine.Model.Serializable
{
    [ProtoContract]
    public class GameDataSerializable
    {
        [ProtoMember(1)]
        public PlayerType WhitePlayer { get; set; }

        [ProtoMember(2)]
        public PlayerType BlackPlayer { get; set; }

        [ProtoMember(3)]
        public PositionDataSerializable StartingPosition { get; set; }

        [ProtoMember(4)]
        public List<MoveSerializable> Moves { get; set; } = new List<MoveSerializable>();

        [ProtoMember(5)]
        public GameResult Result { get; set; }

        [ProtoMember(6)]
        public PgnMetadata Metadata { get; set; }

        public static GameDataSerializable Create(Position position, PlayerType whitePlayer, PlayerType blackPlayer, DateTimeOffset? startedAt = null)
        {
            return new GameDataSerializable
            {
                StartingPosition = position.PositionDataSerializable,
                WhitePlayer = whitePlayer,
                BlackPlayer = blackPlayer,
                Metadata = new PgnMetadata
                {
                    Date = (startedAt ?? DateTimeOffset.Now).ToString("yyyy.MM.dd", CultureInfo.InvariantCulture),
                    White = whitePlayer == PlayerType.Triglav ? "Triglav" : "?",
                    Black = blackPlayer == PlayerType.Triglav ? "Triglav" : "?",
                },
            };
        }

        [ProtoIgnore]
        public string PGN => new GameData(this).PGN;

        /// <summary>
        /// For deserialization
        /// </summary>
        public GameDataSerializable()
        {

        }

        public void AddMove(MoveStruct move, PositionDataSerializable positionDataSerializable)
        {
            this.Moves.Add(new MoveSerializable(move, positionDataSerializable));
        }
    }
}
