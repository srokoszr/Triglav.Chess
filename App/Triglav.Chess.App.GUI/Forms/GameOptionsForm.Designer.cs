using System;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Control;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Forms
{
    partial class GameOptionsForm
    {
        private System.ComponentModel.IContainer components = null;
        private ComboBox opponentComboBox;
        private GroupBox colourGroup;
        private ColourSelectionButton buttonWhite;
        private ColourSelectionButton buttonRandom;
        private ColourSelectionButton buttonBlack;
        private Button startFriendButton;
        private ComboBox algorithmLevelComboBox;
        private CheckBox checkBoxHumanMode;

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
            TableLayoutPanel layout = new() { Dock = DockStyle.Fill, Padding = new Padding(16), ColumnCount = 2, RowCount = 5 };
            _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            for (Int32 row = 0; row < 5; row++)
            {
                _ = layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            this.opponentComboBox = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            this.algorithmLevelComboBox = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            this.checkBoxHumanMode = new() { AutoSize = true, Text = Resources.HumanMode };
            layout.Controls.Add(new Label { AutoSize = true, Text = Resources.OpponentLabel, Padding = new Padding(0, 6, 12, 0) }, 0, 0);
            layout.Controls.Add(this.opponentComboBox, 1, 0);
            layout.Controls.Add(new Label { AutoSize = true, Text = Resources.EngineStrength, Padding = new Padding(0, 6, 12, 0) }, 0, 1);
            layout.Controls.Add(this.algorithmLevelComboBox, 1, 1);
            layout.Controls.Add(this.checkBoxHumanMode, 1, 2);
            this.colourGroup = new() { Text = Resources.Colour, Dock = DockStyle.Fill, Height = 190 };
            TableLayoutPanel colours = new() { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8) };
            for (Int32 column = 0; column < 3; column++)
            {
                _ = colours.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
            }
            this.buttonWhite = new() { IsWhiteButton = true, Size = new Size(150, 150), Anchor = AnchorStyles.None, AccessibleName = Resources.White, TabIndex = 0 };
            this.buttonRandom = new() { IsWhiteButton = true, IsBlackButton = true, Size = new Size(150, 150), Anchor = AnchorStyles.None, AccessibleName = Resources.RandomColour, TabIndex = 1 };
            this.buttonBlack = new() { IsBlackButton = true, Size = new Size(150, 150), Anchor = AnchorStyles.None, AccessibleName = Resources.Black, TabIndex = 2 };
            this.buttonWhite.Click += (_, _) => this.StartGame(Colours.White);
            this.buttonRandom.Click += (_, _) => this.StartGame((Colours)Random.Shared.Next(2));
            this.buttonBlack.Click += (_, _) => this.StartGame(Colours.Black);
            colours.Controls.Add(this.buttonWhite, 0, 0);
            colours.Controls.Add(this.buttonRandom, 1, 0);
            colours.Controls.Add(this.buttonBlack, 2, 0);
            this.colourGroup.Controls.Add(colours);
            layout.Controls.Add(this.colourGroup, 0, 3);
            layout.SetColumnSpan(this.colourGroup, 2);
            FlowLayoutPanel actions = new() { AutoSize = true, Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft, Padding = new Padding(0, 12, 0, 0) };
            Button cancel = new() { Text = Resources.CancelPreferences, AutoSize = true, DialogResult = DialogResult.Cancel };
            this.startFriendButton = new() { Text = Resources.NewGame, AutoSize = true };
            this.startFriendButton.Click += (_, _) => this.StartGame(Colours.White);
            actions.Controls.Add(cancel);
            actions.Controls.Add(this.startFriendButton);
            layout.Controls.Add(actions, 0, 4);
            layout.SetColumnSpan(actions, 2);
            this.Controls.Add(layout);
            this.CancelButton = cancel;
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(720, 380);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = Resources.NewGame;
            this.ResumeLayout(false);
        }
    }
}
