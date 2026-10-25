using Triglav.Chess.App.GUI.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Interface;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.App.GUI
{
    public partial class BoardControl : UserControl
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int FieldSize
        {
            get => this.boardFieldsControl.FieldSize;
            set
            {
                if (this.boardFieldsControl.FieldSize != value)
                {
                    this.boardFieldsControl.FieldSize = value;
                    this.ResizeBoard();
                }
            }
        }

        private int marginSize;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int MarginSize
        {
            get => this.marginSize;
            set
            {
                if (this.marginSize != value)
                {
                    this.marginSize = value;
                    this.ResizeBoard();
                    this.boardFieldsControl.Location = new Point(this.marginSize, this.marginSize);
                }
            }
        }

        private Label[] topLabels = new Label[8];
        private Label[] bottomLabels = new Label[8];
        private Label[] leftLabels = new Label[8];
        private Label[] rightLabels = new Label[8];

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IBoardClickHandler GameEngineInstance { get; set; }

        public BoardControl()
        {
            this.InitializeComponent();
            this.BackColor = GUIColours.BackgroundColour;
            this.SetLabels();
        }

        private void SetLabels()
        {
            for (int i = 0; i < 8; i++)
            {
                if (!this.FlipBoard)
                {
                    if(this.topLabels[i] is null)
                    {
                        this.topLabels[i] = new Label();
                        this.Controls.Add(this.topLabels[i]);
                    }
                    this.topLabels[i].Text = Resources.BoardFiles[i].ToString();
                    this.topLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.topLabels[i].ForeColor = Color.White;

                    if (this.bottomLabels[i] is null)
                    {
                        this.bottomLabels[i] = new Label();
                        this.Controls.Add(this.bottomLabels[i]);
                    }
                    this.bottomLabels[i].Text = Resources.BoardFiles[i].ToString();
                    this.bottomLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.bottomLabels[i].ForeColor = Color.White;

                    if (this.leftLabels[i] is null)
                    {
                        this.leftLabels[i] = new Label();
                        this.Controls.Add(this.leftLabels[i]);
                    }
                    this.leftLabels[i].Text = Resources.BoardRanks[i].ToString();
                    this.leftLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.leftLabels[i].ForeColor = Color.White;

                    if (this.rightLabels[i] is null)
                    {
                        this.rightLabels[i] = new Label();
                        this.Controls.Add(this.rightLabels[i]);
                    }
                    this.rightLabels[i].Text = Resources.BoardRanks[i].ToString();
                    this.rightLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.rightLabels[i].ForeColor = Color.White;
                }
                else
                {
                    int j = 7 - i;
                    if (this.topLabels[i] is null)
                    {
                        this.topLabels[i] = new Label();
                        this.Controls.Add(this.topLabels[i]);
                    }
                    this.topLabels[i].Text = Resources.BoardFiles[j].ToString();
                    this.topLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.topLabels[i].ForeColor = Color.White;

                    if (this.bottomLabels[i] is null)
                    {
                        this.bottomLabels[i] = new Label();
                        this.Controls.Add(this.bottomLabels[i]);
                    }
                    this.bottomLabels[i].Text = Resources.BoardFiles[j].ToString();
                    this.bottomLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.bottomLabels[i].ForeColor = Color.White;

                    if (this.leftLabels[i] is null)
                    {
                        this.leftLabels[i] = new Label();
                        this.Controls.Add(this.leftLabels[i]);
                    }
                    this.leftLabels[i].Text = Resources.BoardRanks[j].ToString();
                    this.leftLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.leftLabels[i].ForeColor = Color.White;

                    if (this.rightLabels[i] is null)
                    {
                        this.rightLabels[i] = new Label();
                        this.Controls.Add(this.rightLabels[i]);
                    }
                    this.rightLabels[i].Text = Resources.BoardRanks[j].ToString();
                    this.rightLabels[i].TextAlign = ContentAlignment.MiddleCenter;
                    this.rightLabels[i].ForeColor = Color.White;
                }
            }
            AlignLabels();
        }

        private void ResizeBoard()
        {
            this.SuspendLayout();
            this.Size = new Size((this.marginSize * 2) + (this.FieldSize * 8), (this.marginSize * 2) + (this.FieldSize * 8));
            this.AlignLabels();
            this.ResumeLayout(true);
        }


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Boolean FlipBoard
        {
            get => this.boardFieldsControl.FlipBoard;
            set
            {
                if (this.boardFieldsControl.FlipBoard != value)
                {
                    this.boardFieldsControl.FlipBoard = value;
                    SetLabels();
                }
            }
        }

        private void AlignLabels()
        {
            for (int i = 0; i < 8; i++)
            {
                this.topLabels[i].Size = new Size(this.marginSize, this.marginSize);
                this.topLabels[i].Location = new Point(this.marginSize + (this.FieldSize * i) + (this.FieldSize / 4), 0);

                this.bottomLabels[i].Size = new Size(this.marginSize, this.marginSize);
                this.bottomLabels[i].Location = new Point(this.marginSize + (this.FieldSize * i) + (this.FieldSize / 4), this.marginSize + (this.FieldSize * 8));

                this.leftLabels[i].Size = new Size(this.marginSize, this.marginSize);
                this.leftLabels[i].Location = new Point(0, this.marginSize + (this.FieldSize * i) + (this.FieldSize / 4));

                this.rightLabels[i].Size = new Size(this.marginSize, this.marginSize);
                this.rightLabels[i].Location = new Point(this.marginSize + (this.FieldSize * 8), this.marginSize + (this.FieldSize * i) + (this.FieldSize / 4));
            }
        }

        public void SetState(Position position)
        {
            this.SetStateInternal(position, null, null, null);
        }

        public void SetState(Position position, MoveStruct lastMove)
        {
            this.SetStateInternal(position, lastMove, null, null);
        }

        public void SetState(Position position, MoveStruct? lastMove, Coordinates currentField, MoveStruct[] legalMoves)
        {
            this.SetStateInternal(position, lastMove, currentField, legalMoves);
        }

        private void SetStateInternal(Position position, MoveStruct? lastMove, Coordinates? currentField, MoveStruct[] legalMoves)
        {
            Coordinates? checkedKingCoordinates = null;
            if(position.OnMove == Colours.White)
            {
                if(position.IsWhiteKingChecked)
                {
                    checkedKingCoordinates = new Coordinates((byte)position.State.WhiteKingLocation);
                }
            }
            else
            {
                if (position.IsBlackKingChecked)
                {
                    checkedKingCoordinates = new Coordinates((byte)position.State.BlackKingLocation);
                }
            }

            var movesToSet = new SortedSet<Coordinates>();
            if(legalMoves != null)
            {
                foreach(var move in legalMoves)
                {
                    movesToSet.Add(new Coordinates((byte)move.To));
                }
            }

            for (byte i = 0; i < 64; i++)
            {
                var fieldData = position.Get(i);

                var coordinates = new Coordinates(i);
                this.boardFieldsControl.SetField(i, fieldData, 
                    coordinates == checkedKingCoordinates, 
                    coordinates.SingleCoordinate == lastMove?.From || coordinates.SingleCoordinate == lastMove?.To, 
                    movesToSet.Contains(coordinates),
                    coordinates == currentField);
            }
        }

        private void BoardControl_Load(object sender, EventArgs e)
        {
            foreach(var fieldcontrol in this.boardFieldsControl.Fields)
            {
                fieldcontrol.Click += this.Fieldcontrol_Click;
            }
        }

        private void Fieldcontrol_Click(object sender, EventArgs e)
        {
            var field = sender as FieldControl;
            if (field != null)
            {
                var coordinates = field.BoardCoordinates;
                this.GameEngineInstance?.ClickMade(coordinates);
            }
        }
    }
}
