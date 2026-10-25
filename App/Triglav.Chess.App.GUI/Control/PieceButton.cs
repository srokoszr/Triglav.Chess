using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Assets;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Control
{
    public partial class PieceButton : Button
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Colours Colour { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Pieces Piece { get; set; }

        private bool isSelected = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }
            set
            {
                this.isSelected = value;
                this.Invalidate();
            }
        }

        public PieceButton()
        {
            InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if(WinFormsHelper.IsInDesigner())
            {
                return;
            }

            Graphics g = e.Graphics;

            if (this.IsSelected)
            {
                using Brush brush = new SolidBrush(GUIColours.LastMoveColour);

                g.FillRectangle(brush, 0, 0, this.Size.Width, this.Size.Width);
            }

            var pieceBitmap = AssetsManager.Instance.GetPieceBitmap(this.Piece, this.Colour, this.Size.Width);

            g.DrawImage(pieceBitmap, new Point(0, 0));
        }
    }
}
