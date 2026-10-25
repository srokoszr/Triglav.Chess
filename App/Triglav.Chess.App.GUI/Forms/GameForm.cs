using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Control;
using Triglav.Chess.App.GUI.Interface;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;

namespace Triglav.Chess.App.GUI.Forms
{
    public partial class GameForm : Form, IGameForm, IDisposable
    {
        public IGameEngine GameEngine
        {
            get;
        }

        public GameForm(GameOptions options, Func<GameOptions, BoardControl, EvaluationBar, IGameForm, IGameEngine> gameEngineFactory)
        {
            this.InitializeComponent();
            this.boardControl.FieldSize = 60;
            this.boardControl.GameEngineInstance = this.GameEngine = gameEngineFactory(options, this.boardControl, this.evaluationBar, this);
            this.GameEngine.OnEvaluationChanged += this.EvaluationChanged;
            this.GameEngine.OnMinMaxEvaluationChanged += this.MinMaxEvaluationChanged;
            this.GameEngine.OnWinChanceEvaluationChanged += this.WinChanceEvaluationChanged;
            this.gameActionsControl.OnDrawOffer += this.DrawOffer;
            this.gameActionsControl.OnResign += this.Resign;
            this.gameInfoControl.EvaluationBar = this.evaluationBar;
            this.RefreshPgn();
            this.AddFileMenu();
            this.FormClosing += (_, e) => e.Cancel = !this.GameEngine.SaveGame();
            this.Shown += (_, _) =>
            {
                if (this.GameEngine.CurrentEaluation is { IsTerminal: true })
                {
                    this.EvaluationChanged();
                }
            };
        }

        private void Resign(Object sender, EventArgs e)
        {
            this.GameEngine.Resign();
            this.RefreshPgn();
        }

        private void DrawOffer(Object sender, EventArgs e) => this.GameEngine.DrawOffer();

        private void EvaluationChanged()
        {
            this.RefreshPgn();
            this.gameInfoControl.EvaluationChanged(this.GameEngine.CurrentEaluation);
            if (this.GameEngine.CurrentEaluation.IsTerminal)
            {
                _ = MessageBox.Show(this, UiText.GameResultText(this.GameEngine.GameData.Result), Resources.GameResultTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void RefreshPgn()
        {
            try
            {
                this.gameInfoControl.SetPgn(this.GameEngine.GameData.PGN);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                Trace.TraceError(error.ToString());
                this.gameInfoControl.SetPgn(Resources.PgnDisplayUnavailable);
            }
        }

        private void MinMaxEvaluationChanged() => this.gameInfoControl.MinMaxEvaluationChanged(this.GameEngine.MinMaxEaluation);

        private void WinChanceEvaluationChanged() => this.gameInfoControl.WinChanceEvaluationChanged(this.GameEngine.MinMaxEaluation);

        public void SetLabelText(String yourMove) => this.gameInfoControl.SetLabelText(yourMove);

        private void DisposeInternal()
        {
            this.GameEngine?.Dispose();
            this.boardControl?.Dispose();
        }

        public void ShowEngineError(Exception error)
        {
            this.SetLabelText(Resources.EngineStopped);
            Trace.TraceError(error.ToString());
            _ = MessageBox.Show(this, Resources.EngineFailure, Resources.EngineErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowPersistenceError(Exception error, String path = null) => PositionFileDialogs.ShowError(this, error, path);

        private void AddFileMenu()
        {
            MenuStrip menu = new();
            ToolStripMenuItem file = new(Resources.FileMenu);
            _ = file.DropDownItems.Add(Resources.SavePositionMenu, null, (_, _) => PositionFileDialogs.Save(this, this.GameEngine.GetPositionSnapshot()));
            _ = file.DropDownItems.Add(Resources.SaveGameMenu, null, (_, _) => this.GameEngine.SaveGame());
            _ = file.DropDownItems.Add(Resources.SaveGameAsMenu, null, (_, _) => this.SaveGameAs());
            _ = file.DropDownItems.Add(Resources.ExportPgnMenu, null, (_, _) => this.ExportPgn());
            _ = menu.Items.Add(file);
            foreach (System.Windows.Forms.Control control in this.Controls)
            {
                if (control.Dock == DockStyle.None)
                {
                    control.Top += menu.Height;
                }
            }
            this.ClientSize = new Size(this.ClientSize.Width, this.ClientSize.Height + menu.Height);
            this.Controls.Add(menu);
            this.MainMenuStrip = menu;
        }

        private void SaveGameAs()
        {
            using SaveFileDialog dialog = new()
            {
                DefaultExt = "TgGame",
                Title = Resources.ChooseGameDestination,
                Filter = WinFormsHelper.TriglavGameFileExtensionFilter,
                FileName = Path.GetFileName(this.GameEngine.GameFilePath),
                InitialDirectory = Path.GetDirectoryName(this.GameEngine.GameFilePath),
                OverwritePrompt = true,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _ = this.GameEngine.SaveGame(dialog.FileName);
            }
        }

        private void ExportPgn()
        {
            using SaveFileDialog dialog = new()
            {
                DefaultExt = "PGN",
                Title = Resources.ExportPgnTitle,
                Filter = WinFormsHelper.PGNFileExtensionFilter,
                FileName = Path.GetFileNameWithoutExtension(this.GameEngine.GameFilePath),
                InitialDirectory = Path.GetDirectoryName(this.GameEngine.GameFilePath),
                OverwritePrompt = true,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _ = this.GameEngine.ExportPgn(dialog.FileName);
            }
        }

        public static void Play(Form parentForm, Position positionToPlay = null)
        {
            try
            {
                using GameOptionsForm gameOptionsForm = new();
                if (gameOptionsForm.ShowDialog(parentForm) != DialogResult.OK)
                {
                    return;
                }
                UserPreferences preferences = UserPreferencesFile.Load();
                if (!File.Exists(UserPreferencesFile.FilePath))
                {
                    UserPreferencesFile.Save(preferences);
                }
                _ = Directory.CreateDirectory(preferences.GameSaveDirectory);
                String gamePath = preferences.CreateGameFilePath();
                // Verify the destination before starting a worker or opening the game window.
                Position initial = positionToPlay ?? Position.GetStartingPosition();
                Engine.Model.Serializable.GameDataSerializable initialGame = Engine.Model.Serializable.GameDataSerializable.Create(initial,
                    gameOptionsForm.Options.IsHuman(Chess.Model.Enumerations.Colours.White) ? Chess.Model.Enumerations.PlayerType.Player : Chess.Model.Enumerations.PlayerType.Triglav,
                    gameOptionsForm.Options.IsHuman(Chess.Model.Enumerations.Colours.Black) ? Chess.Model.Enumerations.PlayerType.Player : Chess.Model.Enumerations.PlayerType.Triglav);
                Engine.Persistence.GameFile.Save(gamePath, initialGame);
                using GameForm gameForm = new(gameOptionsForm.Options, (options, boardControl, evaluationBar, form) => new StandardGameEngine(options, boardControl, evaluationBar, form, gamePath, initial, initialGame));
                parentForm.Visible = false;
                _ = gameForm.ShowDialog(parentForm);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(parentForm, error);
            }
            finally
            {
                parentForm.Visible = true;
            }
        }
    }
}
