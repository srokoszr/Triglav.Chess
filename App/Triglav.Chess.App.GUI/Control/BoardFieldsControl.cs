using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Triglav.Chess.Model.Structs;

namespace Triglav.Chess.App.GUI
{
    public partial class BoardFieldsControl : UserControl
    {
        private FieldControl[] fields;

        public FieldControl[] Fields => this.fields;

        private int fieldSize = 0;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int FieldSize
        {
            get => this.fieldSize;
            set
            {
                if (this.fieldSize != value)
                {
                    this.fieldSize = value;
                    this.Size = new Size(this.fieldSize * 8, this.fieldSize * 8);
                    this.AlignFields();
                }
            }
        }

        private Boolean flipBoard = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Boolean FlipBoard
        {
            get => this.flipBoard;
            set
            {
                if (this.flipBoard != value)
                {
                    this.flipBoard = value;
                    this.AssignCoordinates();
                }
            }
        }

        private void AssignCoordinates()
        {
            for (byte i = 0; i < this.fields.Length; i++)
            {
                if (this.FlipBoard)
                {
                    byte j = (byte)(63 - i);
                    this.fields[i].BoardCoordinates = new Coordinates(j);
                }
                else
                {
                    this.fields[i].BoardCoordinates = new Coordinates(i);
                }
            }
        }

        public BoardFieldsControl()
        {
            this.InitializeComponent();
            this.fields = new FieldControl[64];
            for (byte i = 0; i < this.fields.Length; i++)
            {
                this.fields[i] = new FieldControl();
                this.Controls.Add(this.fields[i]);
            }
            this.AssignCoordinates();
        }

        private FieldControl GetFieldByCoordinates(int index) => this.fields.Single(x => x.BoardCoordinates.SingleCoordinate == index);

        private void AlignFields()
        {
            this.SuspendLayout();
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int index = Coordinates.GetSingleCoordinate(row, col);
                    var control = this.fields[index];

                    control.Size = new Size(this.fieldSize, this.fieldSize);
                    control.Location = new Point(col * this.fieldSize, row * this.fieldSize);
                }
            }
            this.ResumeLayout(true);
        }

        public void SetField(byte index, FieldData fieldData, bool isKingInCheckHighlight, bool isLastMoveHighlight, bool isLegalMoveHighLight, bool isCurrentPieceMoveHighLight)
        {
            var control = this.GetFieldByCoordinates(index);

            if (control.IsKingInCheckHighlight != isKingInCheckHighlight ||
                control.IsLastMoveHighlight != isLastMoveHighlight ||
                control.IsLegalMoveHighLight != isLegalMoveHighLight ||
                control.IsCurrentPieceMoveHighLight != isCurrentPieceMoveHighLight ||
                !control.FieldData.Equals(fieldData))
            {
                control.IsKingInCheckHighlight = isKingInCheckHighlight;
                control.IsLastMoveHighlight = isLastMoveHighlight;
                control.IsLegalMoveHighLight = isLegalMoveHighLight;
                control.IsCurrentPieceMoveHighLight = isCurrentPieceMoveHighLight;
                control.FieldData = fieldData;
            }
        }
    }
}
