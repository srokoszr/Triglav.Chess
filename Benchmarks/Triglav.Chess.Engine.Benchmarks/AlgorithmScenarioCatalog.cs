using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Triglav.Chess.Engine.Comparison;
using Triglav.Chess.Engine.Core;

namespace Triglav.Chess.Engine.Benchmarks
{
    internal static class AlgorithmScenarioCatalog
    {
        internal static void Print()
        {
            List<Object> rows = new();
            TextWriter output = Console.Out;
            try
            {
                Console.SetOut(TextWriter.Null);
                Int32 index = 0;
                foreach (AlgorithmBenchmarkCase scenario in AlgorithmBenchmarkCase.All)
                {
                    Position position = scenario.CreatePosition();
                    rows.Add(new
                    {
                        Index = index++,
                        Scenario = scenario.ToString(),
                        scenario.Kind,
                        Side = position.OnMove.ToString(),
                        FullMoveDepth = scenario.Depth,
                        scenario.Plies,
                        SourceGamePly = scenario.GamePly,
                        HistoryPositions = scenario.CreatePriorPositions().Length,
                        PieceCount = position.Count,
                        LegalMoveCount = position.GetMoves().Count(),
                        Fen = position.FEN_Notation,
                    });
                }
            }
            finally
            {
                Console.SetOut(output);
            }
            Console.WriteLine(JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
