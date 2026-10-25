using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Persistence;

namespace Triglav.Chess.App.GUI.Forms
{
    public sealed class GameImportForm : Form
    {
        private readonly TextBox filePath = new() { ReadOnly = true, Width = 470 };
        private readonly CheckBox assumeVariant = new() { AutoSize = true, Enabled = false, Text = Resources.AssumeMedievalPgn };
        private readonly CheckBox legacy = new() { AutoSize = true, Enabled = false, Text = Resources.LegacyPgn };
        private readonly TextBox summary = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical };
        private readonly Button openPosition = new() { AutoSize = true, Text = Resources.OpenFinalPosition, Enabled = false };
        private GameImportResult imported;

        public GameImportForm()
        {
            this.Text = Resources.ImportGameTitle;
            this.ClientSize = new Size(760, 530);
            this.MinimumSize = new Size(700, 450);
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(12)
            };
            for (Int32 row = 0; row < 6; row++)
            {
                _ = layout.RowStyles.Add(new RowStyle(row == 4 ? SizeType.Percent : SizeType.AutoSize, row == 4 ? 100 : 0));
            }
            FlowLayoutPanel source = new()
            {
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            Button choose = new()
            {
                AutoSize = true,
                Text = Resources.ChooseGameFile
            };
            choose.Click += (_, _) => this.ChooseFile();
            source.Controls.Add(this.filePath);
            source.Controls.Add(choose);
            Button read = new()
            {
                AutoSize = true,
                Text = Resources.ReadGame
            };
            read.Click += (_, _) => this.ReadGame();
            FlowLayoutPanel actions = new()
            {
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            Button close = new()
            {
                AutoSize = true,
                Text = Resources.CloseWindow,
                DialogResult = DialogResult.Cancel
            };
            this.openPosition.Click += (_, _) => this.OpenPosition();
            actions.Controls.Add(this.openPosition);
            Button analyse = new()
            {
                AutoSize = true,
                Text = Resources.GameAnalysis,
                Enabled = false
            };
            analyse.Click += (_, _) =>
            {
                if (this.imported != null)
                {
                    using AnalysisForm form = new(imported: this.imported, source: this.filePath.Text);
                    _ = form.ShowDialog(this);
                }
            };
            this.openPosition.EnabledChanged += (_, _) => analyse.Enabled = this.openPosition.Enabled;
            actions.Controls.Add(analyse);
            actions.Controls.Add(close);
            Label notice = new()
            {
                AutoSize = true,
                MaximumSize = new Size(720, 0),
                Text = Resources.ImportNewGameNotice
            };
            FlowLayoutPanel options = new()
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown
            };
            options.Controls.Add(this.assumeVariant);
            options.Controls.Add(this.legacy);
            layout.Controls.Add(source, 0, 0);
            layout.Controls.Add(options, 0, 1);
            layout.Controls.Add(read, 0, 2);
            layout.Controls.Add(notice, 0, 3);
            layout.Controls.Add(this.summary, 0, 4);
            layout.Controls.Add(actions, 0, 5);
            this.Controls.Add(layout);
            this.CancelButton = close;
            this.AcceptButton = read;
        }

        private void ChooseFile()
        {
            using OpenFileDialog dialog = new()
            {
                Title = Resources.ChooseGameFile,
                Filter = WinFormsHelper.GameFileExtensionFilter,
                CheckFileExists = true,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.filePath.Text = dialog.FileName;
                Boolean isPgn = Path.GetExtension(dialog.FileName).Equals(WinFormsHelper.PGNFileExtension, StringComparison.OrdinalIgnoreCase);
                this.assumeVariant.Enabled = isPgn;
                this.legacy.Enabled = isPgn;
                this.imported = null;
                this.summary.Clear();
                this.openPosition.Enabled = false;
            }
        }

        private void ReadGame()
        {
            if (String.IsNullOrEmpty(this.filePath.Text))
            {
                this.ChooseFile();
                if (String.IsNullOrEmpty(this.filePath.Text))
                {
                    return;
                }
            }
            try
            {
                GameImportResult result = GameFile.Import(this.filePath.Text, new GameImportOptions
                {
                    AssumeTriglavVariant = this.assumeVariant.Checked,
                    AllowLegacyPgn = this.legacy.Checked,
                });
                String warnings = result.Warnings.Count == 0 ? Resources.NoImportWarnings
                    : String.Join(Environment.NewLine + Environment.NewLine, result.Warnings.Select(UiText.GameImportWarningText));
                String text = String.Format(CultureInfo.CurrentCulture, Resources.GameSummary, this.filePath.Text,
                    result.Game.Metadata?.White ?? Resources.UnknownPlayer, result.Game.Metadata?.Black ?? Resources.UnknownPlayer, UiText.GameResultText(result.Game.Result), result.Game.Moves.Count, warnings);
                this.imported = result;
                this.summary.Text = text;
                this.openPosition.Enabled = true;
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(this, error, this.filePath.Text);
            }
        }

        private void OpenPosition()
        {
            if (this.imported == null)
            {
                return;
            }
            using PositionEditorForm editor = new(this.imported.FinalPosition.Clone());
            _ = editor.ShowDialog(this);
        }
    }
}
