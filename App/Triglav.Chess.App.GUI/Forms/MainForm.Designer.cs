using System;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Properties;

namespace Triglav.Chess.App.GUI
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                this.components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(24, 55, 24, 40),
                ColumnCount = 3,
                RowCount = 1,
            };
            for (Int32 column = 0; column < 3; column++)
            {
                _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
            }
            Button newGame = new() { Text = Resources.NewGame, Dock = DockStyle.Fill, Margin = new Padding(8), Name = "newGameButton", TabIndex = 0 };
            Button analysis = new() { Text = Resources.AnalysisTitle, Dock = DockStyle.Fill, Margin = new Padding(8), Name = "analysisButton", TabIndex = 1 };
            Button editor = new() { Text = Resources.Editor, Dock = DockStyle.Fill, Margin = new Padding(8), Name = "editorButton", TabIndex = 2 };
            newGame.Click += this.NewGame_Click;
            analysis.Click += this.Analysis_Click;
            editor.Click += this.Editor_Click;
            layout.Controls.Add(newGame, 0, 0);
            layout.Controls.Add(analysis, 1, 0);
            layout.Controls.Add(editor, 2, 0);
            this.Controls.Add(layout);
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(660, 250);
            this.MinimumSize = new Size(600, 270);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Name = "MainForm";
            this.Text = Resources.ApplicationTitle;
            this.ResumeLayout(false);
        }
    }
}
