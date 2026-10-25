using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Control;
using Triglav.Chess.App.GUI.Interface;
using Triglav.Chess.App.GUI.Model.Enumerations;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Model.Enumerations;
using Triglav.Chess.Model.Structs;
using Triglav.Chess.Model.Structs.Interoperable;

namespace Triglav.Chess.App.GUI.Forms
{
    public partial class PositionEditorForm : Form, IBoardClickHandler
    {
        private readonly RichTextBox fenText = NotationTextBox.Create("fenTextBox", Resources.AnalysisFen);

        public PositionEditorForm(Position initialPosition = null)
        {
            InitializeComponent();
            this.ConfigureActions();
            if (WinFormsHelper.IsInDesigner())
            {
                return;
            }
            this.boardControl.GameEngineInstance = this;
            this.State = initialPosition ?? Position.GetStartingPosition();
            this.RefreshPosition();
            this.radioButtonWhite.Checked = this.State.OnMove == Colours.White;
            this.radioButtonBlack.Checked = this.State.OnMove == Colours.Black;
            this.radioButtonEditModeMove.Checked = true;
            this.groupBoxPieces.Visible = false;
            this.groupBoxSpecialMove.Visible = false;
        }

        private Coordinates? LastClick = null;

        private void ConfigureActions()
        {
            // Keep reset actions as compact buttons and move other document actions to menus.
            this.groupBoxPositionReset.Controls.Remove(this.buttonEmptyPosition);
            this.groupBoxPositionReset.Controls.Remove(this.buttonStartingPosition);
            this.Controls.Remove(this.groupBoxPositionReset);
            this.Controls.Remove(this.groupBoxPositionSaveLoad);
            this.Controls.Remove(this.groupBoxPositionPlayAnalyse);
            this.groupBoxPositionReset.Dispose();
            this.groupBoxPositionSaveLoad.Dispose();
            this.groupBoxPositionPlayAnalyse.Dispose();
            MenuStrip menu = new();
            ToolStripMenuItem file = new(Resources.FileMenu);
            _ = file.DropDownItems.Add(Resources.LoadPosition, null, this.buttonLoadPosition_Click);
            _ = file.DropDownItems.Add(Resources.SavePosition, null, this.buttonSavePosition_Click);
            ToolStripMenuItem position = new(Resources.PositionMenu);
            _ = position.DropDownItems.Add(Resources.AnalysisStartingPosition, null, this.buttonStartingPosition_Click);
            _ = position.DropDownItems.Add(Resources.EmptyWithKings, null, this.buttonEmptyPosition_Click);
            _ = position.DropDownItems.Add(new ToolStripSeparator());
            _ = position.DropDownItems.Add(Resources.PlayPosition, null, this.buttonPlay_Click);
            _ = position.DropDownItems.Add(Resources.AnalysePosition, null, this.buttonAnalyse_Click);
            _ = menu.Items.Add(file);
            _ = menu.Items.Add(position);
            foreach (System.Windows.Forms.Control control in this.Controls)
            {
                control.Top += menu.Height;
            }
            this.buttonEmptyPosition.Location = new Point(this.groupBoxSpecialMove.Left, this.groupBoxSpecialMove.Bottom + 12);
            this.buttonEmptyPosition.Size = new Size(210, 30);
            this.buttonEmptyPosition.TabIndex = 14;
            this.buttonStartingPosition.Location = new Point(this.buttonEmptyPosition.Left, this.buttonEmptyPosition.Bottom + 6);
            this.buttonStartingPosition.Size = new Size(210, 30);
            this.buttonStartingPosition.TabIndex = 15;
            this.Controls.Add(this.buttonEmptyPosition);
            this.Controls.Add(this.buttonStartingPosition);
            PieceButton[] pieces = [.. this.groupBoxPieces.Controls.OfType<PieceButton>().OrderBy(static piece => piece.TabIndex)];
            for (Int32 index = 0; index < pieces.Length; index++)
            {
                pieces[index].Size = new Size(32, 32);
                pieces[index].Location = new Point(6 + index % 5 * 38, 22 + index / 5 * 38);
            }
            this.groupBoxPieces.Size = new Size(210, 104);
            this.Controls.Add(menu);
            this.MainMenuStrip = menu;
            GroupBox notation = new()
            {
                Text = Resources.AnalysisFen,
                Dock = DockStyle.Bottom,
                Height = 90,
                Padding = new Padding(8),
            };
            notation.Controls.Add(this.fenText);
            this.Controls.Add(notation);
            this.ClientSize = new Size(800, 680);
            this.StartPosition = FormStartPosition.CenterParent;
        }

        private void RefreshPosition()
        {
            this.boardControl.SetState(this.State);
            NotationTextBox.Update(this.fenText, this.State.FEN_Notation);
        }

        private (Colours Colour, Pieces Piece)? PieceToAdd = null;

        private PositionEditorMode EditorMode;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Position State
        {
            get; private set;
        }

        public void ClickMade(Coordinates coordinates)
        {
            if (this.EditorMode == PositionEditorMode.Move)
            {
                if (LastClick.HasValue)
                {
                    if (coordinates != this.LastClick.Value)
                    {
                        var targetPiece = this.State.GetPiece(coordinates);
                        var isTargetFieldTaken = this.State.IsFieldTaken(coordinates);
                        if (isTargetFieldTaken && (targetPiece == Pieces.King || targetPiece == Pieces.KingMoved))
                        {
                            //do not allow capturing King
                            return;
                        }

                        var piece = this.State.GetPiece(this.LastClick.Value);
                        var move = new MoveWithPieceStruct(new MoveStruct(this.LastClick.Value.SingleCoordinate, coordinates.SingleCoordinate), piece);

                        if (this.State.GetPieceColour(this.LastClick.Value) == Colours.White)
                        {
                            this.State.MoveWhite(move);
                        }
                        else
                        {
                            this.State.MoveBlack(move);
                        }
                    }
                    this.RefreshPosition();
                    this.LastClick = null;
                    this.radioButtonWhite.Checked = this.State.OnMove == Colours.White;
                    this.radioButtonBlack.Checked = this.State.OnMove == Colours.Black;
                }
                else
                {
                    if (this.State.IsFieldTaken(coordinates))
                    {
                        this.boardControl.SetState(this.State, null, coordinates, null);
                        this.LastClick = coordinates;
                    }
                }
            }

            if (this.EditorMode == PositionEditorMode.Delete)
            {
                var targetPiece = this.State.GetPiece(coordinates);
                var isTargetFieldTaken = this.State.IsFieldTaken(coordinates);
                if (isTargetFieldTaken && (targetPiece == Pieces.King || targetPiece == Pieces.KingMoved))
                {
                    //do not allow capturing King
                    return;
                }

                this.State.SetEmpty(coordinates);
                this.RefreshPosition();
            }

            if (this.EditorMode == PositionEditorMode.Add)
            {
                if (this.PieceToAdd.HasValue)
                {
                    var targetPiece = this.State.GetPiece(coordinates);
                    var isTargetFieldTaken = this.State.IsFieldTaken(coordinates);
                    if (isTargetFieldTaken && (targetPiece == Pieces.King || targetPiece == Pieces.KingMoved))
                    {
                        //do not allow capturing King
                        return;
                    }

                    this.State.SetEmpty(coordinates);
                    this.State.Add(this.PieceToAdd.Value.Colour, this.PieceToAdd.Value.Piece, coordinates);
                    this.RefreshPosition();
                }
            }

            if (this.EditorMode == PositionEditorMode.None)
            {
                var targetPiece = this.State.GetPiece(coordinates);
                var isTargetFieldTaken = this.State.IsFieldTaken(coordinates);
                if (isTargetFieldTaken && (targetPiece == Pieces.King || targetPiece == Pieces.KingMoved || targetPiece == Pieces.Queen || targetPiece == Pieces.QueenMoved))
                {
                    this.boardControl.SetState(this.State, null, coordinates, null);
                    this.LastClick = coordinates;

                    this.checkBoxSpecialMove.Checked = targetPiece == Pieces.King || targetPiece == Pieces.Queen;
                }
                else
                {
                    this.LastClick = null;
                    this.RefreshPosition();
                    this.checkBoxSpecialMove.Checked = false;
                }
            }
        }

        private void buttonEmptyPosition_Click(object sender, EventArgs e)
        {
            this.State = Position.GetEmptyWithKings();
            this.RefreshPosition();
            this.radioButtonWhite.Checked = this.State.OnMove == Colours.White;
            this.radioButtonBlack.Checked = this.State.OnMove == Colours.Black;
        }

        private void buttonStartingPosition_Click(object sender, EventArgs e)
        {
            this.State = Position.GetStartingPosition();
            this.RefreshPosition();
            this.radioButtonWhite.Checked = this.State.OnMove == Colours.White;
            this.radioButtonBlack.Checked = this.State.OnMove == Colours.Black;
        }

        private void buttonSavePosition_Click(object sender, EventArgs e) => PositionFileDialogs.Save(this, this.State.Clone());

        private void buttonLoadPosition_Click(object sender, EventArgs e)
        {
            if (!PositionFileDialogs.TryLoad(this, out Position position))
            {
                return;
            }
            this.LastClick = null;
            this.State = position;
            this.radioButtonEditModeMove.Checked = true;
            this.radioButtonWhite.Checked = this.State.OnMove == Colours.White;
            this.radioButtonBlack.Checked = this.State.OnMove == Colours.Black;
            this.RefreshPosition();
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            try
            {
                this.Visible = false;
                GameForm.Play(this, this.State.Clone());
            }
            finally
            {
                this.Visible = true;
            }
        }

        private void buttonAnalyse_Click(object sender, EventArgs e)
        {
            try
            {
                using AnalysisForm form = new(this.State.Clone());
                _ = form.ShowDialog(this);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(this, error);
            }
        }

        private void radioButtonEditModeNone_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Checked)
                {
                    this.EditorMode = PositionEditorMode.None;
                    this.LastClick = null;
                    this.RefreshPosition();
                }

                this.groupBoxSpecialMove.Visible = radioButton.Checked;
            }
        }

        private void radioButtonEditModeMove_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Checked)
                {
                    this.EditorMode = PositionEditorMode.Move;
                    this.LastClick = null;
                    this.RefreshPosition();
                }
            }
        }

        private void radioButtonEditModeAdd_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Checked)
                {
                    this.EditorMode = PositionEditorMode.Add;
                    this.LastClick = null;
                    this.RefreshPosition();
                }
                else
                {
                    this.PieceToAdd = null;
                    ResetPieceButtons();
                }

                this.groupBoxPieces.Visible = radioButton.Checked;
            }
        }

        private void radioButtonEditModeDelete_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton)
            {
                if (radioButton.Checked)
                {
                    this.EditorMode = PositionEditorMode.Delete;
                    this.LastClick = null;
                    this.RefreshPosition();
                }
            }
        }

        private void ResetPieceButtons()
        {
            this.pieceButtonWhitePawn.IsSelected = false;
            this.pieceButtonWhiteQueen.IsSelected = false;
            this.pieceButtonWhiteBishop.IsSelected = false;
            this.pieceButtonWhiteKnight.IsSelected = false;
            this.pieceButtonWhiteRook.IsSelected = false;
            this.pieceButtonBlackQueen.IsSelected = false;
            this.pieceButtonBlackBishop.IsSelected = false;
            this.pieceButtonBlackKnight.IsSelected = false;
            this.pieceButtonBlackRook.IsSelected = false;
            this.pieceButtonBlackPawn.IsSelected = false;
        }

        private void pieceButtonWhitePawn_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.White, Pieces.Pawn);
        }

        private void pieceButtonWhiteRook_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.White, Pieces.Rook);
        }

        private void pieceButtonWhiteKnight_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.White, Pieces.Knight);
        }

        private void pieceButtonWhiteBishop_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.White, Pieces.Bishop);
        }

        private void pieceButtonWhiteQueen_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.White, Pieces.QueenMoved);
        }

        private void pieceButtonBlackPawn_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.Black, Pieces.Pawn);
        }

        private void pieceButtonBlackRook_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.Black, Pieces.Rook);
        }

        private void pieceButtonBlackKnight_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.Black, Pieces.Knight);
        }

        private void pieceButtonBlackBishop_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.Black, Pieces.Bishop);
        }

        private void pieceButtonBlackQueen_Click(object sender, EventArgs e)
        {
            ResetPieceButtons();
            (sender as PieceButton)?.IsSelected = true;

            this.PieceToAdd = (Colours.Black, Pieces.QueenMoved);
        }

        private void radioButtonWhite_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.radioButtonWhite.Checked)
            {
                return;
            }
            this.State.OnMove = Colours.White;
            this.RefreshPosition();
        }

        private void radioButtonBlack_CheckedChanged(object sender, EventArgs e)
        {
            if (!this.radioButtonBlack.Checked)
            {
                return;
            }
            this.State.OnMove = Colours.Black;
            this.RefreshPosition();
        }

        private void checkBoxSpecialMove_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (this.LastClick.HasValue)
                {
                    var targetPiece = this.State.GetPiece(this.LastClick.Value);
                    var pieceColour = this.State.GetPieceColour(this.LastClick.Value);

                    if (checkBox.Checked)
                    {
                        if (targetPiece == Pieces.KingMoved)
                        {
                            this.State.SetEmpty(this.LastClick.Value);
                            this.State.Set(pieceColour, Pieces.King, this.LastClick.Value);
                            this.RefreshPosition();
                        }

                        if (targetPiece == Pieces.QueenMoved)
                        {
                            this.State.SetEmpty(this.LastClick.Value);
                            this.State.Set(pieceColour, Pieces.Queen, this.LastClick.Value);
                            this.RefreshPosition();
                        }
                    }
                    else
                    {
                        if (targetPiece == Pieces.King)
                        {
                            this.State.SetEmpty(this.LastClick.Value);
                            this.State.Set(pieceColour, Pieces.KingMoved, this.LastClick.Value);
                            this.RefreshPosition();
                        }

                        if (targetPiece == Pieces.Queen)
                        {
                            this.State.SetEmpty(this.LastClick.Value);
                            this.State.Set(pieceColour, Pieces.QueenMoved, this.LastClick.Value);
                            this.RefreshPosition();
                        }
                    }
                }
            }
        }
    }
}
