using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Triglav.Chess.Engine.Comparison;

namespace Triglav.Chess.Engine.Benchmarks
{
    [Config(typeof(AlgorithmBenchmarkConfig))]
    [MemoryDiagnoser]
    public class FullDepthComparisonBenchmark
    {
        public IEnumerable<AlgorithmBenchmarkCase> Scenarios => AlgorithmBenchmarkCase.All;

        [ParamsSource(nameof(Scenarios))]
        public AlgorithmBenchmarkCase Scenario { get; set; }

        private SearchComparisonBenchmark search;
        private SearchComparisonResult latest;

        [GlobalSetup]
        public void Setup()
        {
            this.search = new SearchComparisonBenchmark { Scenario = this.Scenario };
            this.search.Initialize();
        }

        [Benchmark(Baseline = true)]
        public SearchComparisonResult MinMax() => this.latest = this.search.MinMax();

        [Benchmark]
        public SearchComparisonResult PositionTree() => this.latest = this.search.PositionTree();

        [Benchmark]
        public SearchComparisonResult PositionTreeWithCopying() => this.latest = this.search.PositionTreeWithCopying();

        [GlobalCleanup]
        public void Cleanup()
        {
            try
            {
                // Validate the actual last measured result without repeating other
                // backends' potentially very expensive searches during setup.
                this.search.ValidateResult(this.latest);
                Console.WriteLine("SEARCH_RESULT_JSON " + JsonSerializer.Serialize(new
                {
                    Scenario = this.Scenario.ToString(),
                    SearchPolicy = "FixedDepthNoExtensions",
                    Move = this.latest.Move.Move.ToString(),
                    Piece = this.latest.Move.Piece.ToString(),
                    ResultCode = this.latest.Evaluation.Result,
                    this.latest.Evaluation.InNMoves,
                    this.latest.Evaluation.EvaluationValue,
                }));
            }
            finally
            {
                this.search.Cleanup();
            }
        }
    }
}
