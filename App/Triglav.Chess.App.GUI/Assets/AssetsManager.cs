using Svg;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Assets
{
    public class AssetsManager
    {
        private Dictionary<string, Bitmap> graphicsCache = new Dictionary<string, Bitmap>();

        private AssetsManager()
        {
        }

        public static AssetsManager Instance = new AssetsManager();

        private Stream GetImageStream(Pieces piece, Colours colour)
        {
            var colourChar = colour == Colours.White ? "l" : "d";
            var piecechar = (piece) switch
            {
                Pieces.King => "k",
                Pieces.KingMoved => "k",
                Pieces.Queen => "q",
                Pieces.QueenMoved => "q",
                Pieces.Rook => "r",
                Pieces.Bishop => "b",
                Pieces.Knight => "n",
                Pieces.Pawn => "p",

                _ => throw new InvalidEnumArgumentException()
            };
            var resourceName = $"Triglav.Chess.App.GUI.Assets.CBurnett.Chess_{piecechar}{colourChar}t45.svg";
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            return stream;
        }

        private string GetCacheKey(Pieces piece, Colours colour, int size) => $"{piece}-{colour}-{size}";

        public Bitmap GetPieceBitmap(Pieces piece, Colours colour, int size)
        {
            var key = this.GetCacheKey(piece, colour, size);
            if(!this.graphicsCache.TryGetValue(key, out var result))
            {
                using (var stream = this.GetImageStream(piece, colour))
                {
                    var svg = SvgDocument.Open<SvgDocument>(stream);

                    //svg.Height = size;
                    //svg.Width = size;          

                    result = svg.Draw(size, size);

                    this.graphicsCache[key] = result;
                }
            }

            return result;
        }

        public Bitmap GetWhitePiecesBitmap(int size) => this.GetPieceBitmap(Pieces.King, Colours.White, size);

        public Bitmap GetBlackPiecesBitmap(int size) => this.GetPieceBitmap(Pieces.King, Colours.Black, size);

        public Bitmap GetRandomSideBitmap(int size)
        {
            var key = "RANDOM_SIDE";

            if (!this.graphicsCache.TryGetValue(key, out var result))
            {
                Bitmap whiteKing = this.GetPieceBitmap(Pieces.King, Colours.White, size);
                Bitmap blackKing = this.GetPieceBitmap(Pieces.King, Colours.Black, size);

                result = new Bitmap(size, size);

                using Graphics g = Graphics.FromImage(result);

                g.Clear(Color.Transparent);

                //left side - white king
                Rectangle leftSrc = new Rectangle(0, 0, size / 2, size);

                Rectangle leftDst = new Rectangle(0, 0, size / 2, size);

                g.DrawImage(
                    whiteKing,
                    leftDst,
                    leftSrc,
                    GraphicsUnit.Pixel);

                //right side - black king
                Rectangle rightSrc = new Rectangle(size / 2, 0, size / 2, size);

                Rectangle rightDst = new Rectangle(size / 2, 0, size / 2, size);

                g.DrawImage(
                    blackKing,
                    rightDst,
                    rightSrc,
                    GraphicsUnit.Pixel);

                this.graphicsCache[key] = result;
            }
            return result;
        }
    }
}
