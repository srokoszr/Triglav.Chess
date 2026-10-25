using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Assets;

namespace Triglav.Chess.App.GUI.Control
{
    public partial class ColourSelectionButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsWhiteButton { get; set; } = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsBlackButton { get; set; } = false;

        public ColourSelectionButton()
        {
            this.InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            if (this.IsWhiteButton && this.IsBlackButton)
            {
                var pieceBitmap = AssetsManager.Instance.GetRandomSideBitmap(this.Size.Width);

                g.DrawImage(pieceBitmap, new Point(0, 0));
            }
            else if (this.IsWhiteButton)
            {
                var pieceBitmap = AssetsManager.Instance.GetWhitePiecesBitmap(this.Size.Width);

                g.DrawImage(pieceBitmap, new Point(0, 0));
            }
            else if (this.IsBlackButton)
            {
                var pieceBitmap = AssetsManager.Instance.GetBlackPiecesBitmap(this.Size.Width);

                g.DrawImage(pieceBitmap, new Point(0, 0));
            }
        }
    }
}
