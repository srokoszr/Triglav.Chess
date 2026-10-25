using System;
using Triglav.Chess.Engine.Model.Serializable;
using Triglav.Chess.Engine.Notation;

namespace Triglav.Chess.Engine.Core
{
    public class GameData(GameDataSerializable gameDataSerializable)
    {
        public GameDataSerializable GameDataSerializable
        {
            get; set;
        } = gameDataSerializable;
        public Position StartingPosition => Position.GetPosition(this.GameDataSerializable.StartingPosition);
        private String Header(String name) => PgnProfile.Tag(name, PgnWriter.Headers(this.GameDataSerializable)[name]);
        public String Event_TAG => this.Header("Event");
        public String Site_TAG => this.Header("Site");
        public String Date_TAG => this.Header("Date");
        public String Round_TAG => this.Header("Round");
        public String White_TAG => this.Header("White");
        public String Black_TAG => this.Header("Black");
        public String Result => PgnProfile.Score(this.GameDataSerializable.Result);
        public String Result_TAG => this.Header("Result");
        public Boolean IsSetUp => PgnProfile.IsSetUp(this.StartingPosition);
        public String FEN_TAG => this.IsSetUp ? this.Header("FEN") : null;
        public String SetUp_TAG => this.IsSetUp ? this.Header("SetUp") : null;
        public String PlyCount_TAG => this.Header("PlyCount");
        public String PGN => PgnWriter.Write(this.GameDataSerializable);
    }
}
