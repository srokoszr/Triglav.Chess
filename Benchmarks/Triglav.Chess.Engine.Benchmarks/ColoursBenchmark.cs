using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Extensions;
using System.Linq;

namespace Triglav.Chess.Engine.Benchmarks
{
    [MemoryDiagnoser(true)]
    [SimpleJob(RunStrategy.Throughput, launchCount: 2, warmupCount: 1, iterationCount: 10, runtimeMoniker: RuntimeMoniker.Net90)]
    public class ColoursBenchmark
    {
        [Params(10, 100, 1000, 10000)]
        public int N;

        public Colours[] Colours;

        [GlobalSetup]
        public void Setup()
        {
            this.Colours = Enumerable.Range(0, this.N).Select(x => x % 2).Select(x => (Colours)x).ToArray();
        }

        [Benchmark(Baseline = true)]
        public void GetOpposite()
        {
            for (int i = 0; i < this.N; i++)
            {
                var x = this.Colours[i].GetOpposite();
            }
        }

        [Benchmark]
        public void GetOppositeBitwise()
        {
            for (int i = 0; i < this.N; i++)
            {
                var x = this.Colours[i].GetOppositeBitwise();
            }
        }

        [Benchmark]
        public void GetOppositeBitwiseReturnInt()
        {
            for (int i = 0; i < this.N; i++)
            {
                var x = this.Colours[i].GetOppositeBitwiseReturnInt();
            }
        }

        [Benchmark]
        public void FlipWithOpposite()
        {
            Colours colour = default;

            for (int i = 0;i < this.N; i++)
            {
                colour = colour.GetOpposite();
            }
        }

        [Benchmark]
        public void FlipWithOppositeBitwise()
        {
            Colours colour = default;

            for (int i = 0; i < this.N; i++)
            {
                colour = colour.GetOppositeBitwise();
            }
        }

        [Benchmark]
        public void FlipWithOppositeBitwiseByte()
        {
            byte colour = (byte)Triglav.Chess.Model.Enumerations.Colours.White;

            for (int i = 0; i < this.N; i++)
            {
                colour ^= 1;
            }
        }
    }
}
