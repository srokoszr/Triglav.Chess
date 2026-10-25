using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.App.GUI.Properties;

namespace Triglav.Chess.App.GUI.Forms
{
    public sealed class PreferencesForm : Form
    {
        private readonly TextBox gameSaveDirectory = new() { Dock = DockStyle.Fill };

        public PreferencesForm(UserPreferences preferences)
        {
            this.Text = Resources.PreferencesTitle;
            this.ClientSize = new Size(660, 170);
            this.MinimumSize = new Size(550, 210);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.gameSaveDirectory.Text = preferences.GameSaveDirectory;
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12),
                ColumnCount = 2,
                RowCount = 3,
            };
            _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Label label = new()
            {
                AutoSize = true,
                Text = Resources.GameSaveDirectoryLabel,
                Margin = new Padding(3, 3, 3, 8),
            };
            Button browse = new()
            {
                AutoSize = true,
                Text = Resources.BrowseFolder
            };
            browse.Click += (_, _) => this.Browse();
            Button save = new()
            {
                AutoSize = true,
                Text = Resources.SavePreferences
            };
            save.Click += (_, _) => this.Save();
            Button cancel = new()
            {
                AutoSize = true,
                Text = Resources.CancelPreferences,
                DialogResult = DialogResult.Cancel
            };
            FlowLayoutPanel actions = new()
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 12, 0, 0),
            };
            actions.Controls.Add(cancel);
            actions.Controls.Add(save);
            layout.Controls.Add(label, 0, 0);
            layout.SetColumnSpan(label, 2);
            layout.Controls.Add(this.gameSaveDirectory, 0, 1);
            layout.Controls.Add(browse, 1, 1);
            layout.Controls.Add(actions, 0, 2);
            layout.SetColumnSpan(actions, 2);
            this.Controls.Add(layout);
            this.AcceptButton = save;
            this.CancelButton = cancel;
        }

        private void Browse()
        {
            using FolderBrowserDialog dialog = new()
            {
                Description = Resources.ChooseGameSaveDirectory,
                UseDescriptionForTitle = true,
                SelectedPath = this.gameSaveDirectory.Text,
                ShowNewFolderButton = true,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.gameSaveDirectory.Text = dialog.SelectedPath;
            }
        }

        private void Save()
        {
            String directory = this.gameSaveDirectory.Text.Trim();
            try
            {
                if (String.IsNullOrWhiteSpace(directory) || !Path.IsPathFullyQualified(directory))
                {
                    _ = MessageBox.Show(this, Resources.InvalidGameSaveDirectory, Resources.PreferencesTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.gameSaveDirectory.Focus();
                    return;
                }
                UserPreferences preferences = new()
                {
                    GameSaveDirectory = Path.GetFullPath(directory)
                };
                _ = Directory.CreateDirectory(preferences.GameSaveDirectory);
                // Check write access before replacing the saved preference.
                using (FileStream probe = new(Path.Combine(preferences.GameSaveDirectory, $".{Guid.NewGuid():N}.tmp"), FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose))
                {
                    probe.WriteByte(0);
                }
                UserPreferencesFile.Save(preferences);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(this, error, directory);
            }
        }
    }
}
