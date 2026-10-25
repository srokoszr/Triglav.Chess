using System;

namespace Triglav.Chess.App.GUI.Forms
{
    partial class GameForm
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
            if(disposing)
            {
                DisposeInternal();
            }
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
            this.gameInfoControl = new Triglav.Chess.App.GUI.Control.GameInfoControl();
            this.gameActionsControl = new Triglav.Chess.App.GUI.Control.GameActionsControl();
            this.evaluationBar = new Triglav.Chess.App.GUI.Control.EvaluationBar();
            this.SuspendLayout();
            // 
            // boardControl
            // 
            this.boardControl.BackColor = System.Drawing.Color.FromArgb(159, 89, 43);
            this.boardControl.FieldSize = 60;
            this.boardControl.Location = new System.Drawing.Point(40, 0);
            this.boardControl.MarginSize = 25;
            this.boardControl.Name = "boardControl";
            this.boardControl.Size = new System.Drawing.Size(530, 530);
            this.boardControl.TabIndex = 0;
            // 
            // gameInfoControl
            // 
            this.gameInfoControl.Dock = System.Windows.Forms.DockStyle.Right;
            this.gameInfoControl.Location = new System.Drawing.Point(588, 0);
            this.gameInfoControl.Name = "gameInfoControl";
            this.gameInfoControl.Size = new System.Drawing.Size(346, 711);
            this.gameInfoControl.TabIndex = 1;
            // 
            // gameActionsControl
            // 
            this.gameActionsControl.Location = new System.Drawing.Point(12, 556);
            this.gameActionsControl.Name = "gameActionsControl";
            this.gameActionsControl.Size = new System.Drawing.Size(360, 44);
            this.gameActionsControl.TabIndex = 2;
            // 
            // evaluationBar1
            // 
            this.evaluationBar.Location = new System.Drawing.Point(0, 0);
            this.evaluationBar.Name = "evaluationBar1";
            this.evaluationBar.Size = new System.Drawing.Size(40, 530);
            this.evaluationBar.TabIndex = 3;
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(934, 711);
            this.Controls.Add(this.evaluationBar);
            this.Controls.Add(this.gameActionsControl);
            this.Controls.Add(this.gameInfoControl);
            this.Controls.Add(this.boardControl);
            this.Name = "GameForm";
            this.Text = global::Triglav.Chess.App.GUI.Properties.Resources.GameTitle;
            this.ResumeLayout(false);
        }

        #endregion

        private BoardControl boardControl;
        private Control.GameInfoControl gameInfoControl;
        private Control.GameActionsControl gameActionsControl;
        private Control.EvaluationBar evaluationBar;
    }
}
