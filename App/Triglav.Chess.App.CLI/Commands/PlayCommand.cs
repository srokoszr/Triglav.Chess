using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Core.Algorithms;
using Triglav.Chess.Model.Enumerations;
using System;
using System.IO;
using System.Threading.Tasks;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.App.CLI.Commands
{
    [Command("play")]
    public class PlayCommand : ICommand
    {
        [CommandOption("as-black", 'b', IsRequired = false)]
        public bool AsBlack { get; set; }

        [CommandParameter(0, Name = "first-move", IsRequired = false)]
        public string FirstMove { get; set; }

        [CommandOption("show-evaluation", 'e', IsRequired = false)]
        public bool ShowEvaluation { get; set; } = true;

        public Colours ColourToPlayByPlayer => this.AsBlack ? Colours.Black : Colours.White;

        public MoveStruct? FirstMoveToPlay => string.IsNullOrWhiteSpace(this.FirstMove) ? (MoveStruct?)null : MoveStruct.Parse(this.FirstMove);

        public async ValueTask ExecuteAsync(IConsole console)
        {
            Position position = Position.GetStartingPosition();
            /*position = new Position()
            {
                { Colours.Black, Pieces.Rook, "g8" },
                { Colours.Black, Pieces.KingMoved, "a7" },
                { Colours.Black, Pieces.Pawn, "b6" },
                { Colours.White, Pieces.Rook, "d5" },
                { Colours.White, Pieces.Pawn, "h5" },
                { Colours.Black, Pieces.Bishop, "b4" },
                { Colours.White, Pieces.QueenMoved, "c4" },
                { Colours.Black, Pieces.Knight, "f3" },
                { Colours.White, Pieces.KingMoved, "h2" },
                { Colours.White, Pieces.Bishop, "c1" },
                { Colours.White, Pieces.Bishop, "f1" },
            };
            position = new Position()
            {
                { Colours.Black, Pieces.KingMoved, "e8" },
                { Colours.White, Pieces.Rook, "b7" },
                { Colours.White, Pieces.Pawn, "h7" },
                { Colours.Black, Pieces.Pawn, "b6" },
                { Colours.White, Pieces.QueenMoved, "b5" },
                { Colours.White, Pieces.KingMoved, "e5" },
                { Colours.Black, Pieces.Rook, "h5" },
                { Colours.Black, Pieces.Knight, "e1" },
            };*/

            bool alreadyMadeFirstMove = !this.FirstMoveToPlay.HasValue;

            var algorithm = new BreadthFirstTreeSearch();/* new MinMaxWithAlphaBeta()*/;

            string fileName = DateTime.Now.ToString("O").Replace(":", String.Empty);
            fileName = $"{Program.BasePath}\\{fileName}.out";
            console.WriteLine($"Output will be saved to: {fileName}");

            using var stack = PositionStackWrapper.New();

            using (var file = File.OpenWrite(fileName))
            {
                using (var textWriter = new StreamWriter(file) { AutoFlush = true })
                {
                    algorithm.Print(textWriter);

                    int moveCount = 2;

                    while (true)
                    {
                        console.WriteLine();
                        console.WriteLine(position);
                        await textWriter.WriteLineAsync(position.ToString());

                        var terminal = algorithm.PositionEvaluator.EvaluateTerminalPosition(position, 0);
                        if (terminal.IsTerminal)
                        {
                            console.WriteLine(terminal.Result);
                            break;
                        }
                        if (this.ShowEvaluation)
                        {
                            var evaluation = algorithm.PositionEvaluator.EvaluateNonTerminalPosition(position, 0);
                            console.WriteLine($"Current position evaluation: {evaluation}");
                        }

                        MoveWithPieceStruct move = new MoveWithPieceStruct();

                        if (position.OnMove != this.ColourToPlayByPlayer)
                        {
                            console.WriteLine("Engine is thinking of a move...");
                            (_, move, _, _) = algorithm.GetTopMove(position, stack, textWriter);
                        }
                        else
                        {
                            bool isValidMove = false;
                            while (true)
                            {
                                console.WriteLine("your move:");
                                var moveInput = console.ReadLine();
                                isValidMove = MoveStruct.TryParse(moveInput, out var moveNoPiece);

                                if (isValidMove)
                                {
                                    isValidMove = false;

                                    var moves = position.GetMoves();
                                    for (int i = 0; i < moves.Length; i++)
                                    {
                                        if (moves[i].MoveStruct.Move == moveNoPiece)
                                        {
                                            isValidMove = true;
                                            move = moves[i].MoveStruct;
                                            break;
                                        }
                                    }
                                }

                                if (isValidMove)
                                {
                                    break;
                                }
                            }
                        }

                        console.WriteLine($"making move: {move}");
                        await textWriter.WriteLineAsync($"{(moveCount) / 2}: {(moveCount % 2 == 0 ? move.ToString() : "..." + move.ToString())}");
                        moveCount++;

                        position.Move(move);
                        stack.Push(position.State);
                    }

                    return;
                }
            }
        }
    }
}
