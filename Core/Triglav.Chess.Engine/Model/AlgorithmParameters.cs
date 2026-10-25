using System.IO;
using System.Text;

namespace Triglav.Chess.Engine.Model
{
    public class AlgorithmParameters
    {
        public int Depth { get; set; } = 3;

        public int StartingDepth => this.Depth * 2;

        public int MaxMovesTrackedHardLimitOverStartingDepth => 2;

        public int AdditionalDepthForCaptures { get; set; } = 2;

        public int AdditionalDepthForChecks { get; set; } = 2;


        public void Print(TextWriter stream)
        {
            var sb = new StringBuilder();
            sb.AppendLine("AlgorithmParameters:");
            sb.AppendLine($"\tStartingDepth: {this.StartingDepth}");
            sb.AppendLine($"\tMaxMovesTrackedHardLimitOverStartingDepth: {this.MaxMovesTrackedHardLimitOverStartingDepth}");
            sb.AppendLine($"\tAdditionalDepthForCaptures: {this.AdditionalDepthForCaptures}");
            sb.AppendLine($"\tAdditionalDepthForChecks: {this.AdditionalDepthForChecks}");
            stream.WriteLine(sb);
        }
    }
}