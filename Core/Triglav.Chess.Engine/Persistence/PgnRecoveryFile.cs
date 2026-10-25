using System;
using System.IO;
using Triglav.Chess.Engine.Model.Serializable;

namespace Triglav.Chess.Engine.Persistence
{
    public sealed class PgnRecoveryFile
    {
        public PgnRecoveryFile(String directory)
        {
            this.FilePath = Path.Combine(directory, $"game-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}-{Guid.NewGuid():N}.PGN");
        }

        public String FilePath
        {
            get;
        }

        public void Save(GameDataSerializable game) => GameFile.ExportPgn(this.FilePath, game);
    }
}
