using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Assets;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.App.GUI
{
    public partial class FieldControl : UserControl
    {
        private Coordinates boardCoordinates;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public Coordinates BoardCoordinates
        {
            get => this.boardCoordinates;
            set
            {
                this.boardCoordinates = value;

                this.BackColor = (this.boardCoordinates.Row + this.boardCoordinates.Column) % 2 == 1 ? GUIColours.WhiteFieldColour : GUIColours.BlackFieldColour;
                this.Invalidate();
            }
        }

        private FieldData fieldData;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public FieldData FieldData
        {
            get => this.fieldData;
            set
            {
                this.fieldData = value;
                this.Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsKingInCheckHighlight { get; internal set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsLastMoveHighlight { get; internal set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsLegalMoveHighLight { get; internal set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool IsCurrentPieceMoveHighLight { get; internal set; }

        public FieldControl()
        {
            this.InitializeComponent();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;

            if (this.IsKingInCheckHighlight)
            {
                using Brush brush = new SolidBrush(GUIColours.KingCheckedColour);

                g.FillRectangle(brush, 0, 0, this.Size.Width, this.Size.Width);
            }
            else if (this.IsLegalMoveHighLight)
            {
                using Brush brush = new SolidBrush(GUIColours.LegalMoveColour);

                g.FillRectangle(brush, 0, 0, this.Size.Width, this.Size.Width);
            }
            else if (this.IsLastMoveHighlight)
            {
                using Brush brush = new SolidBrush(GUIColours.LastMoveColour);

                g.FillRectangle(brush, 0, 0, this.Size.Width, this.Size.Width);
            }
            else if (this.IsCurrentPieceMoveHighLight)
            {
                using Brush brush = new SolidBrush(GUIColours.LastMoveColour);

                g.FillRectangle(brush, 0, 0, this.Size.Width, this.Size.Width);
            }

            if (this.FieldData.IsTaken)
            {
                var pieceBitmap = AssetsManager.Instance.GetPieceBitmap(this.fieldData.Piece, this.fieldData.PieceColour, this.Size.Width);

                g.DrawImage(pieceBitmap, new Point(0, 0));
            }
        }
    }
}
