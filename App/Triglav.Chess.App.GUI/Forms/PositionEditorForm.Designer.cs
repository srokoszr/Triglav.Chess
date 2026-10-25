namespace Triglav.Chess.App.GUI.Forms
{
    partial class PositionEditorForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.boardControl = new BoardControl();
            this.buttonEmptyPosition = new System.Windows.Forms.Button();
            this.buttonStartingPosition = new System.Windows.Forms.Button();
            this.buttonSavePosition = new System.Windows.Forms.Button();
            this.buttonLoadPosition = new System.Windows.Forms.Button();
            this.buttonPlay = new System.Windows.Forms.Button();
            this.buttonAnalyse = new System.Windows.Forms.Button();
            this.groupBoxPositionReset = new System.Windows.Forms.GroupBox();
            this.groupBoxPositionSaveLoad = new System.Windows.Forms.GroupBox();
            this.groupBoxPositionPlayAnalyse = new System.Windows.Forms.GroupBox();
            this.groupBoxEditModes = new System.Windows.Forms.GroupBox();
            this.radioButtonEditModeDelete = new System.Windows.Forms.RadioButton();
            this.radioButtonEditModeAdd = new System.Windows.Forms.RadioButton();
            this.radioButtonEditModeMove = new System.Windows.Forms.RadioButton();
            this.radioButtonEditModeNone = new System.Windows.Forms.RadioButton();
            this.groupBoxPieces = new System.Windows.Forms.GroupBox();
            this.pieceButtonBlackQueen = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonBlackBishop = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonBlackKnight = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonBlackRook = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonBlackPawn = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonWhiteQueen = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonWhiteBishop = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonWhiteKnight = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonWhiteRook = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.pieceButtonWhitePawn = new Triglav.Chess.App.GUI.Control.PieceButton();
            this.groupBoxColour = new System.Windows.Forms.GroupBox();
            this.radioButtonBlack = new System.Windows.Forms.RadioButton();
            this.radioButtonWhite = new System.Windows.Forms.RadioButton();
            this.groupBoxSpecialMove = new System.Windows.Forms.GroupBox();
            this.checkBoxSpecialMove = new System.Windows.Forms.CheckBox();
            this.groupBoxPositionReset.SuspendLayout();
            this.groupBoxPositionSaveLoad.SuspendLayout();
            this.groupBoxPositionPlayAnalyse.SuspendLayout();
            this.groupBoxEditModes.SuspendLayout();
            this.groupBoxPieces.SuspendLayout();
            this.groupBoxColour.SuspendLayout();
            this.groupBoxSpecialMove.SuspendLayout();
            this.SuspendLayout();
            // 
            // boardControl
            // 
            this.boardControl.BackColor = System.Drawing.Color.FromArgb(159, 89, 43);
            this.boardControl.FieldSize = 60;
            this.boardControl.Location = new System.Drawing.Point(12, 12);
            this.boardControl.MarginSize = 25;
            this.boardControl.Name = "boardControl";
            this.boardControl.Size = new System.Drawing.Size(530, 530);
            this.boardControl.TabIndex = 0;
            // 
            // buttonEmptyPosition
            // 
            this.buttonEmptyPosition.Location = new System.Drawing.Point(15, 15);
            this.buttonEmptyPosition.Name = "buttonEmptyPosition";
            this.buttonEmptyPosition.Size = new System.Drawing.Size(162, 84);
            this.buttonEmptyPosition.TabIndex = 1;
            this.buttonEmptyPosition.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EmptyWithKings;
            this.buttonEmptyPosition.UseVisualStyleBackColor = true;
            this.buttonEmptyPosition.Click += this.buttonEmptyPosition_Click;
            // 
            // buttonStartingPosition
            // 
            this.buttonStartingPosition.Location = new System.Drawing.Point(205, 15);
            this.buttonStartingPosition.Name = "buttonStartingPosition";
            this.buttonStartingPosition.Size = new System.Drawing.Size(162, 84);
            this.buttonStartingPosition.TabIndex = 2;
            this.buttonStartingPosition.Text = global::Triglav.Chess.App.GUI.Properties.Resources.StartingPosition;
            this.buttonStartingPosition.UseVisualStyleBackColor = true;
            this.buttonStartingPosition.Click += this.buttonStartingPosition_Click;
            // 
            // buttonSavePosition
            // 
            this.buttonSavePosition.Location = new System.Drawing.Point(15, 15);
            this.buttonSavePosition.Name = "buttonSavePosition";
            this.buttonSavePosition.Size = new System.Drawing.Size(162, 84);
            this.buttonSavePosition.TabIndex = 3;
            this.buttonSavePosition.Text = global::Triglav.Chess.App.GUI.Properties.Resources.SavePosition;
            this.buttonSavePosition.UseVisualStyleBackColor = true;
            this.buttonSavePosition.Click += this.buttonSavePosition_Click;
            // 
            // buttonLoadPosition
            // 
            this.buttonLoadPosition.Location = new System.Drawing.Point(205, 15);
            this.buttonLoadPosition.Name = "buttonLoadPosition";
            this.buttonLoadPosition.Size = new System.Drawing.Size(162, 84);
            this.buttonLoadPosition.TabIndex = 4;
            this.buttonLoadPosition.Text = global::Triglav.Chess.App.GUI.Properties.Resources.LoadPosition;
            this.buttonLoadPosition.UseVisualStyleBackColor = true;
            this.buttonLoadPosition.Click += this.buttonLoadPosition_Click;
            // 
            // buttonPlay
            // 
            this.buttonPlay.Location = new System.Drawing.Point(15, 15);
            this.buttonPlay.Name = "buttonPlay";
            this.buttonPlay.Size = new System.Drawing.Size(162, 84);
            this.buttonPlay.TabIndex = 5;
            this.buttonPlay.Text = global::Triglav.Chess.App.GUI.Properties.Resources.PlayPosition;
            this.buttonPlay.UseVisualStyleBackColor = true;
            this.buttonPlay.Click += this.buttonPlay_Click;
            // 
            // buttonAnalyse
            // 
            this.buttonAnalyse.Location = new System.Drawing.Point(15, 113);
            this.buttonAnalyse.Name = "buttonAnalyse";
            this.buttonAnalyse.Size = new System.Drawing.Size(162, 84);
            this.buttonAnalyse.TabIndex = 6;
            this.buttonAnalyse.Text = global::Triglav.Chess.App.GUI.Properties.Resources.AnalysePosition;
            this.buttonAnalyse.UseVisualStyleBackColor = true;
            this.buttonAnalyse.Click += this.buttonAnalyse_Click;
            // 
            // groupBoxPositionReset
            // 
            this.groupBoxPositionReset.Controls.Add(this.buttonEmptyPosition);
            this.groupBoxPositionReset.Controls.Add(this.buttonStartingPosition);
            this.groupBoxPositionReset.Location = new System.Drawing.Point(7, 549);
            this.groupBoxPositionReset.Name = "groupBoxPositionReset";
            this.groupBoxPositionReset.Size = new System.Drawing.Size(380, 104);
            this.groupBoxPositionReset.TabIndex = 7;
            this.groupBoxPositionReset.TabStop = false;
            // 
            // groupBoxPositionSaveLoad
            // 
            this.groupBoxPositionSaveLoad.Controls.Add(this.buttonSavePosition);
            this.groupBoxPositionSaveLoad.Controls.Add(this.buttonLoadPosition);
            this.groupBoxPositionSaveLoad.Location = new System.Drawing.Point(405, 549);
            this.groupBoxPositionSaveLoad.Name = "groupBoxPositionSaveLoad";
            this.groupBoxPositionSaveLoad.Size = new System.Drawing.Size(380, 104);
            this.groupBoxPositionSaveLoad.TabIndex = 8;
            this.groupBoxPositionSaveLoad.TabStop = false;
            // 
            // groupBoxPositionPlayAnalyse
            // 
            this.groupBoxPositionPlayAnalyse.Controls.Add(this.buttonPlay);
            this.groupBoxPositionPlayAnalyse.Controls.Add(this.buttonAnalyse);
            this.groupBoxPositionPlayAnalyse.Location = new System.Drawing.Point(798, 451);
            this.groupBoxPositionPlayAnalyse.Name = "groupBoxPositionPlayAnalyse";
            this.groupBoxPositionPlayAnalyse.Size = new System.Drawing.Size(188, 202);
            this.groupBoxPositionPlayAnalyse.TabIndex = 9;
            this.groupBoxPositionPlayAnalyse.TabStop = false;
            // 
            // groupBoxEditModes
            // 
            this.groupBoxEditModes.Controls.Add(this.radioButtonEditModeDelete);
            this.groupBoxEditModes.Controls.Add(this.radioButtonEditModeAdd);
            this.groupBoxEditModes.Controls.Add(this.radioButtonEditModeMove);
            this.groupBoxEditModes.Controls.Add(this.radioButtonEditModeNone);
            this.groupBoxEditModes.Location = new System.Drawing.Point(557, 12);
            this.groupBoxEditModes.Name = "groupBoxEditModes";
            this.groupBoxEditModes.Size = new System.Drawing.Size(200, 126);
            this.groupBoxEditModes.TabIndex = 10;
            this.groupBoxEditModes.TabStop = false;
            this.groupBoxEditModes.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EditMode;
            // 
            // radioButtonEditModeDelete
            // 
            this.radioButtonEditModeDelete.AutoSize = true;
            this.radioButtonEditModeDelete.Location = new System.Drawing.Point(6, 97);
            this.radioButtonEditModeDelete.Name = "radioButtonEditModeDelete";
            this.radioButtonEditModeDelete.Size = new System.Drawing.Size(58, 19);
            this.radioButtonEditModeDelete.TabIndex = 3;
            this.radioButtonEditModeDelete.TabStop = true;
            this.radioButtonEditModeDelete.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EditDelete;
            this.radioButtonEditModeDelete.UseVisualStyleBackColor = true;
            this.radioButtonEditModeDelete.CheckedChanged += this.radioButtonEditModeDelete_CheckedChanged;
            // 
            // radioButtonEditModeAdd
            // 
            this.radioButtonEditModeAdd.AutoSize = true;
            this.radioButtonEditModeAdd.Location = new System.Drawing.Point(6, 72);
            this.radioButtonEditModeAdd.Name = "radioButtonEditModeAdd";
            this.radioButtonEditModeAdd.Size = new System.Drawing.Size(47, 19);
            this.radioButtonEditModeAdd.TabIndex = 2;
            this.radioButtonEditModeAdd.TabStop = true;
            this.radioButtonEditModeAdd.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EditAdd;
            this.radioButtonEditModeAdd.UseVisualStyleBackColor = true;
            this.radioButtonEditModeAdd.CheckedChanged += this.radioButtonEditModeAdd_CheckedChanged;
            // 
            // radioButtonEditModeMove
            // 
            this.radioButtonEditModeMove.AutoSize = true;
            this.radioButtonEditModeMove.Location = new System.Drawing.Point(6, 47);
            this.radioButtonEditModeMove.Name = "radioButtonEditModeMove";
            this.radioButtonEditModeMove.Size = new System.Drawing.Size(55, 19);
            this.radioButtonEditModeMove.TabIndex = 1;
            this.radioButtonEditModeMove.TabStop = true;
            this.radioButtonEditModeMove.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EditMove;
            this.radioButtonEditModeMove.UseVisualStyleBackColor = true;
            this.radioButtonEditModeMove.CheckedChanged += this.radioButtonEditModeMove_CheckedChanged;
            // 
            // radioButtonEditModeNone
            // 
            this.radioButtonEditModeNone.AutoSize = true;
            this.radioButtonEditModeNone.Location = new System.Drawing.Point(6, 22);
            this.radioButtonEditModeNone.Name = "radioButtonEditModeNone";
            this.radioButtonEditModeNone.Size = new System.Drawing.Size(54, 19);
            this.radioButtonEditModeNone.TabIndex = 0;
            this.radioButtonEditModeNone.TabStop = true;
            this.radioButtonEditModeNone.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EditSpecialMoves;
            this.radioButtonEditModeNone.UseVisualStyleBackColor = true;
            this.radioButtonEditModeNone.CheckedChanged += this.radioButtonEditModeNone_CheckedChanged;
            // 
            // groupBoxPieces
            // 
            this.groupBoxPieces.Controls.Add(this.pieceButtonBlackQueen);
            this.groupBoxPieces.Controls.Add(this.pieceButtonBlackBishop);
            this.groupBoxPieces.Controls.Add(this.pieceButtonBlackKnight);
            this.groupBoxPieces.Controls.Add(this.pieceButtonBlackRook);
            this.groupBoxPieces.Controls.Add(this.pieceButtonBlackPawn);
            this.groupBoxPieces.Controls.Add(this.pieceButtonWhiteQueen);
            this.groupBoxPieces.Controls.Add(this.pieceButtonWhiteBishop);
            this.groupBoxPieces.Controls.Add(this.pieceButtonWhiteKnight);
            this.groupBoxPieces.Controls.Add(this.pieceButtonWhiteRook);
            this.groupBoxPieces.Controls.Add(this.pieceButtonWhitePawn);
            this.groupBoxPieces.Location = new System.Drawing.Point(557, 154);
            this.groupBoxPieces.Name = "groupBoxPieces";
            this.groupBoxPieces.Size = new System.Drawing.Size(287, 137);
            this.groupBoxPieces.TabIndex = 11;
            this.groupBoxPieces.TabStop = false;
            this.groupBoxPieces.Text = global::Triglav.Chess.App.GUI.Properties.Resources.Pieces;
            // 
            // pieceButtonBlackQueen
            // 
            this.pieceButtonBlackQueen.Colour = Chess.Model.Enumerations.Colours.Black;
            this.pieceButtonBlackQueen.IsSelected = false;
            this.pieceButtonBlackQueen.Location = new System.Drawing.Point(229, 78);
            this.pieceButtonBlackQueen.Name = "pieceButtonBlackQueen";
            this.pieceButtonBlackQueen.Piece = Chess.Model.Enumerations.Pieces.Queen;
            this.pieceButtonBlackQueen.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonBlackQueen.TabIndex = 9;
            this.pieceButtonBlackQueen.UseVisualStyleBackColor = true;
            this.pieceButtonBlackQueen.Click += this.pieceButtonBlackQueen_Click;
            // 
            // pieceButtonBlackBishop
            // 
            this.pieceButtonBlackBishop.Colour = Chess.Model.Enumerations.Colours.Black;
            this.pieceButtonBlackBishop.IsSelected = false;
            this.pieceButtonBlackBishop.Location = new System.Drawing.Point(173, 78);
            this.pieceButtonBlackBishop.Name = "pieceButtonBlackBishop";
            this.pieceButtonBlackBishop.Piece = Chess.Model.Enumerations.Pieces.Bishop;
            this.pieceButtonBlackBishop.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonBlackBishop.TabIndex = 8;
            this.pieceButtonBlackBishop.UseVisualStyleBackColor = true;
            this.pieceButtonBlackBishop.Click += this.pieceButtonBlackBishop_Click;
            // 
            // pieceButtonBlackKnight
            // 
            this.pieceButtonBlackKnight.Colour = Chess.Model.Enumerations.Colours.Black;
            this.pieceButtonBlackKnight.IsSelected = false;
            this.pieceButtonBlackKnight.Location = new System.Drawing.Point(117, 78);
            this.pieceButtonBlackKnight.Name = "pieceButtonBlackKnight";
            this.pieceButtonBlackKnight.Piece = Chess.Model.Enumerations.Pieces.Knight;
            this.pieceButtonBlackKnight.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonBlackKnight.TabIndex = 7;
            this.pieceButtonBlackKnight.UseVisualStyleBackColor = true;
            this.pieceButtonBlackKnight.Click += this.pieceButtonBlackKnight_Click;
            // 
            // pieceButtonBlackRook
            // 
            this.pieceButtonBlackRook.Colour = Chess.Model.Enumerations.Colours.Black;
            this.pieceButtonBlackRook.IsSelected = false;
            this.pieceButtonBlackRook.Location = new System.Drawing.Point(62, 78);
            this.pieceButtonBlackRook.Name = "pieceButtonBlackRook";
            this.pieceButtonBlackRook.Piece = Chess.Model.Enumerations.Pieces.Rook;
            this.pieceButtonBlackRook.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonBlackRook.TabIndex = 6;
            this.pieceButtonBlackRook.UseVisualStyleBackColor = true;
            this.pieceButtonBlackRook.Click += this.pieceButtonBlackRook_Click;
            // 
            // pieceButtonBlackPawn
            // 
            this.pieceButtonBlackPawn.Colour = Chess.Model.Enumerations.Colours.Black;
            this.pieceButtonBlackPawn.IsSelected = false;
            this.pieceButtonBlackPawn.Location = new System.Drawing.Point(6, 78);
            this.pieceButtonBlackPawn.Name = "pieceButtonBlackPawn";
            this.pieceButtonBlackPawn.Piece = Chess.Model.Enumerations.Pieces.Pawn;
            this.pieceButtonBlackPawn.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonBlackPawn.TabIndex = 5;
            this.pieceButtonBlackPawn.UseVisualStyleBackColor = true;
            this.pieceButtonBlackPawn.Click += this.pieceButtonBlackPawn_Click;
            // 
            // pieceButtonWhiteQueen
            // 
            this.pieceButtonWhiteQueen.Colour = Chess.Model.Enumerations.Colours.White;
            this.pieceButtonWhiteQueen.IsSelected = false;
            this.pieceButtonWhiteQueen.Location = new System.Drawing.Point(229, 22);
            this.pieceButtonWhiteQueen.Name = "pieceButtonWhiteQueen";
            this.pieceButtonWhiteQueen.Piece = Chess.Model.Enumerations.Pieces.Queen;
            this.pieceButtonWhiteQueen.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonWhiteQueen.TabIndex = 4;
            this.pieceButtonWhiteQueen.UseVisualStyleBackColor = true;
            this.pieceButtonWhiteQueen.Click += this.pieceButtonWhiteQueen_Click;
            // 
            // pieceButtonWhiteBishop
            // 
            this.pieceButtonWhiteBishop.Colour = Chess.Model.Enumerations.Colours.White;
            this.pieceButtonWhiteBishop.IsSelected = false;
            this.pieceButtonWhiteBishop.Location = new System.Drawing.Point(173, 22);
            this.pieceButtonWhiteBishop.Name = "pieceButtonWhiteBishop";
            this.pieceButtonWhiteBishop.Piece = Chess.Model.Enumerations.Pieces.Bishop;
            this.pieceButtonWhiteBishop.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonWhiteBishop.TabIndex = 3;
            this.pieceButtonWhiteBishop.UseVisualStyleBackColor = true;
            this.pieceButtonWhiteBishop.Click += this.pieceButtonWhiteBishop_Click;
            // 
            // pieceButtonWhiteKnight
            // 
            this.pieceButtonWhiteKnight.Colour = Chess.Model.Enumerations.Colours.White;
            this.pieceButtonWhiteKnight.IsSelected = false;
            this.pieceButtonWhiteKnight.Location = new System.Drawing.Point(117, 22);
            this.pieceButtonWhiteKnight.Name = "pieceButtonWhiteKnight";
            this.pieceButtonWhiteKnight.Piece = Chess.Model.Enumerations.Pieces.Knight;
            this.pieceButtonWhiteKnight.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonWhiteKnight.TabIndex = 2;
            this.pieceButtonWhiteKnight.UseVisualStyleBackColor = true;
            this.pieceButtonWhiteKnight.Click += this.pieceButtonWhiteKnight_Click;
            // 
            // pieceButtonWhiteRook
            // 
            this.pieceButtonWhiteRook.Colour = Chess.Model.Enumerations.Colours.White;
            this.pieceButtonWhiteRook.IsSelected = false;
            this.pieceButtonWhiteRook.Location = new System.Drawing.Point(62, 22);
            this.pieceButtonWhiteRook.Name = "pieceButtonWhiteRook";
            this.pieceButtonWhiteRook.Piece = Chess.Model.Enumerations.Pieces.Rook;
            this.pieceButtonWhiteRook.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonWhiteRook.TabIndex = 1;
            this.pieceButtonWhiteRook.UseVisualStyleBackColor = true;
            this.pieceButtonWhiteRook.Click += this.pieceButtonWhiteRook_Click;
            // 
            // pieceButtonWhitePawn
            // 
            this.pieceButtonWhitePawn.Colour = Chess.Model.Enumerations.Colours.White;
            this.pieceButtonWhitePawn.IsSelected = false;
            this.pieceButtonWhitePawn.Location = new System.Drawing.Point(6, 22);
            this.pieceButtonWhitePawn.Name = "pieceButtonWhitePawn";
            this.pieceButtonWhitePawn.Piece = Chess.Model.Enumerations.Pieces.Pawn;
            this.pieceButtonWhitePawn.Size = new System.Drawing.Size(50, 50);
            this.pieceButtonWhitePawn.TabIndex = 0;
            this.pieceButtonWhitePawn.UseVisualStyleBackColor = true;
            this.pieceButtonWhitePawn.Click += this.pieceButtonWhitePawn_Click;
            // 
            // groupBoxColour
            // 
            this.groupBoxColour.Controls.Add(this.radioButtonBlack);
            this.groupBoxColour.Controls.Add(this.radioButtonWhite);
            this.groupBoxColour.Location = new System.Drawing.Point(557, 316);
            this.groupBoxColour.Name = "groupBoxColour";
            this.groupBoxColour.Size = new System.Drawing.Size(200, 81);
            this.groupBoxColour.TabIndex = 12;
            this.groupBoxColour.TabStop = false;
            this.groupBoxColour.Text = global::Triglav.Chess.App.GUI.Properties.Resources.SideToMove;
            // 
            // radioButtonBlack
            // 
            this.radioButtonBlack.AutoSize = true;
            this.radioButtonBlack.Location = new System.Drawing.Point(6, 47);
            this.radioButtonBlack.Name = "radioButtonBlack";
            this.radioButtonBlack.Size = new System.Drawing.Size(53, 19);
            this.radioButtonBlack.TabIndex = 1;
            this.radioButtonBlack.TabStop = true;
            this.radioButtonBlack.Text = global::Triglav.Chess.App.GUI.Properties.Resources.Black;
            this.radioButtonBlack.UseVisualStyleBackColor = true;
            this.radioButtonBlack.CheckedChanged += this.radioButtonBlack_CheckedChanged;
            // 
            // radioButtonWhite
            // 
            this.radioButtonWhite.AutoSize = true;
            this.radioButtonWhite.Location = new System.Drawing.Point(6, 22);
            this.radioButtonWhite.Name = "radioButtonWhite";
            this.radioButtonWhite.Size = new System.Drawing.Size(56, 19);
            this.radioButtonWhite.TabIndex = 0;
            this.radioButtonWhite.TabStop = true;
            this.radioButtonWhite.Text = global::Triglav.Chess.App.GUI.Properties.Resources.White;
            this.radioButtonWhite.UseVisualStyleBackColor = true;
            this.radioButtonWhite.CheckedChanged += this.radioButtonWhite_CheckedChanged;
            // 
            // groupBoxSpecialMove
            // 
            this.groupBoxSpecialMove.Controls.Add(this.checkBoxSpecialMove);
            this.groupBoxSpecialMove.Location = new System.Drawing.Point(557, 419);
            this.groupBoxSpecialMove.Name = "groupBoxSpecialMove";
            this.groupBoxSpecialMove.Size = new System.Drawing.Size(200, 58);
            this.groupBoxSpecialMove.TabIndex = 13;
            this.groupBoxSpecialMove.TabStop = false;
            // 
            // checkBoxSpecialMove
            // 
            this.checkBoxSpecialMove.AutoSize = true;
            this.checkBoxSpecialMove.Location = new System.Drawing.Point(6, 22);
            this.checkBoxSpecialMove.Name = "checkBoxSpecialMove";
            this.checkBoxSpecialMove.Size = new System.Drawing.Size(101, 19);
            this.checkBoxSpecialMove.TabIndex = 0;
            this.checkBoxSpecialMove.Text = global::Triglav.Chess.App.GUI.Properties.Resources.SpecialMoveAvailable;
            this.checkBoxSpecialMove.UseVisualStyleBackColor = true;
            this.checkBoxSpecialMove.CheckedChanged += this.checkBoxSpecialMove_CheckedChanged;
            // 
            // PositionEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(995, 656);
            this.Controls.Add(this.groupBoxSpecialMove);
            this.Controls.Add(this.groupBoxColour);
            this.Controls.Add(this.groupBoxPieces);
            this.Controls.Add(this.groupBoxEditModes);
            this.Controls.Add(this.groupBoxPositionPlayAnalyse);
            this.Controls.Add(this.groupBoxPositionSaveLoad);
            this.Controls.Add(this.groupBoxPositionReset);
            this.Controls.Add(this.boardControl);
            this.Name = "PositionEditorForm";
            this.Text = global::Triglav.Chess.App.GUI.Properties.Resources.PositionEditorTitle;
            this.groupBoxPositionReset.ResumeLayout(false);
            this.groupBoxPositionSaveLoad.ResumeLayout(false);
            this.groupBoxPositionPlayAnalyse.ResumeLayout(false);
            this.groupBoxEditModes.ResumeLayout(false);
            this.groupBoxEditModes.PerformLayout();
            this.groupBoxPieces.ResumeLayout(false);
            this.groupBoxColour.ResumeLayout(false);
            this.groupBoxColour.PerformLayout();
            this.groupBoxSpecialMove.ResumeLayout(false);
            this.groupBoxSpecialMove.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private BoardControl boardControl;
        private System.Windows.Forms.Button buttonEmptyPosition;
        private System.Windows.Forms.Button buttonStartingPosition;
        private System.Windows.Forms.Button buttonSavePosition;
        private System.Windows.Forms.Button buttonLoadPosition;
        private System.Windows.Forms.Button buttonPlay;
        private System.Windows.Forms.Button buttonAnalyse;
        private System.Windows.Forms.GroupBox groupBoxPositionReset;
        private System.Windows.Forms.GroupBox groupBoxPositionSaveLoad;
        private System.Windows.Forms.GroupBox groupBoxPositionPlayAnalyse;
        private System.Windows.Forms.GroupBox groupBoxEditModes;
        private System.Windows.Forms.RadioButton radioButtonEditModeDelete;
        private System.Windows.Forms.RadioButton radioButtonEditModeAdd;
        private System.Windows.Forms.RadioButton radioButtonEditModeMove;
        private System.Windows.Forms.RadioButton radioButtonEditModeNone;
        private System.Windows.Forms.GroupBox groupBoxPieces;
        private Control.PieceButton pieceButtonWhitePawn;
        private Control.PieceButton pieceButtonWhiteQueen;
        private Control.PieceButton pieceButtonWhiteBishop;
        private Control.PieceButton pieceButtonWhiteKnight;
        private Control.PieceButton pieceButtonWhiteRook;
        private Control.PieceButton pieceButtonBlackQueen;
        private Control.PieceButton pieceButtonBlackBishop;
        private Control.PieceButton pieceButtonBlackKnight;
        private Control.PieceButton pieceButtonBlackRook;
        private Control.PieceButton pieceButtonBlackPawn;
        private System.Windows.Forms.GroupBox groupBoxColour;
        private System.Windows.Forms.RadioButton radioButtonBlack;
        private System.Windows.Forms.RadioButton radioButtonWhite;
        private System.Windows.Forms.GroupBox groupBoxSpecialMove;
        private System.Windows.Forms.CheckBox checkBoxSpecialMove;
    }
}