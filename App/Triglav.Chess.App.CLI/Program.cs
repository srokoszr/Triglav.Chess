using CliFx;
using Triglav.Chess.Engine.Benchmarks;
using System;
using System.Threading.Tasks;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.App.CLI
{
    public static class Program
    {
        internal static string BasePath =>
            $"{PathHelper.CurrentLocationDirectoryInfo?.Parent?.Parent?.Parent?.Parent}\\Output";
        /*
        static void Main(string[] args)
        {
            var position = Position.GetStartingPosition();

            string fileName = DateTime.Now.ToString("O").Replace(":", String.Empty);
            fileName = $"{BasePath}\\{fileName}.out";
            Console.WriteLine($"Output will be saved to: {fileName}");

            var algorithm = new Algorithm();
            using (var file = File.OpenWrite(fileName))
            {
                using (var textWriter = new StreamWriter(file))
                {
                    algorithm.Print(textWriter);
                    var tree = algorithm.GetTopNMoves(position, 8);
                    tree.Print(textWriter);
                }
            }
        }*/

        public static async Task<int> Main(string[] args)
        {
#if DEBUG
            Console.WriteLine("Running in DEBUG mode!");
#endif


            var app = new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .Build();

            if (args.Length > 0)
            {
                await app.RunAsync(args);
            }

            while(true)
            {
                Console.WriteLine("Enter command:");
                var line = Console.ReadLine();
                var @params = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                await app.RunAsync(@params);
            }
        }
    }
}
