using System;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Forms;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.App.GUI.Properties;

namespace Triglav.Chess.App.GUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            this.InitializeComponent();
            MenuStrip menu = new();
            ToolStripMenuItem file = new(Resources.FileMenu);
            _ = file.DropDownItems.Add(Resources.ImportGameMenu, null, (_, _) =>
            {
                using GameImportForm form = new();
                _ = form.ShowDialog(this);
            });
            _ = file.DropDownItems.Add(Resources.PreferencesTitle, null, (_, _) => this.EditPreferences());
            _ = file.DropDownItems.Add(new ToolStripSeparator());
            _ = file.DropDownItems.Add(Resources.ExitApplication, null, (_, _) => this.Close());
            _ = menu.Items.Add(file);
            this.Controls.Add(menu);
            this.MainMenuStrip = menu;
        }

        private void EditPreferences()
        {
            try
            {
                using PreferencesForm form = new(UserPreferencesFile.Load());
                _ = form.ShowDialog(this);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(this, error, UserPreferencesFile.FilePath);
            }
        }

        private void NewGame_Click(Object sender, EventArgs e) => GameForm.Play(this);

        private void Analysis_Click(Object sender, EventArgs e)
        {
            using AnalysisForm form = new();
            _ = form.ShowDialog(this);
        }

        private void Editor_Click(Object sender, EventArgs e)
        {
            using PositionEditorForm form = new();
            _ = form.ShowDialog(this);
        }
    }
}
