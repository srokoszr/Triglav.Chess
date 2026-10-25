using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;
using Triglav.Chess.Engine.Persistence;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Forms
{
    public sealed class AnalysisForm : Form
    {
        private readonly TabControl modes = new() { Dock = DockStyle.Fill };
        private readonly BoardControl positionBoard = CreateBoard();
        private readonly BoardControl gameBoard = CreateBoard();
        private readonly Label positionStatus = new() { AutoSize = true };
        private readonly Label gameStatus = new() { AutoSize = true };
        private readonly Label gameSummary = new() { AutoSize = true, MaximumSize = new Size(400, 0) };
        private readonly Label gameWarnings = new() { AutoSize = true, MaximumSize = new Size(400, 0) };
        private readonly TextBox positionFen = new() { ReadOnly = true, Dock = DockStyle.Fill };
        private readonly TextBox gameFen = new() { ReadOnly = true };
        private readonly TextBox gameSource = new() { ReadOnly = true, Width = 650 };
        private readonly ListBox moveList = new() { Dock = DockStyle.Fill, IntegralHeight = false };
        private readonly Button first = new() { AutoSize = true, Text = Resources.AnalysisFirst };
        private readonly Button previous = new() { AutoSize = true, Text = Resources.AnalysisPrevious };
        private readonly Button next = new() { AutoSize = true, Text = Resources.AnalysisNext };
        private readonly Button last = new() { AutoSize = true, Text = Resources.AnalysisLast };
        private Position position;
        private AnalysisTimeline timeline;

        public AnalysisForm(Position initialPosition = null, GameImportResult imported = null, String source = null)
        {
            this.Text = Resources.AnalysisTitle;
            this.ClientSize = new Size(1100, 880);
            this.MinimumSize = new Size(1050, 900);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Controls.Add(this.modes);
            this.BuildPositionTab();
            this.BuildGameTab();
            this.SetPosition(initialPosition ?? Position.GetStartingPosition());
            if (imported != null)
            {
                this.SetGame(imported, source);
            }
            else
            {
                this.LoadExample();
            }
            this.modes.SelectedIndex = initialPosition == null ? 1 : 0;
        }

        public static String ExamplePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Triglav Chess", "Examples", "AnalysisExample.TgGame");

        private void LoadExample()
        {
            try
            {
                _ = Directory.CreateDirectory(Path.GetDirectoryName(ExamplePath));
                using Stream example = typeof(AnalysisForm).Assembly.GetManifestResourceStream("AnalysisExample.TgGame")
                    ?? throw new InvalidDataException("The bundled example game is missing.");
                AtomicFile.Write(ExamplePath, example.CopyTo);
                this.LoadGame(ExamplePath);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(this, error, ExamplePath);
            }
        }

        private static BoardControl CreateBoard() => new() { FieldSize = 52, MarginSize = 24, Anchor = AnchorStyles.Top | AnchorStyles.Left };

        private static Button ActionButton(String text, Action action)
        {
            Button button = new()
            {
                AutoSize = true,
                Text = text
            };
            button.Click += (_, _) => action();
            return button;
        }

        private static FlowLayoutPanel Actions() => new() { AutoSize = true, Dock = DockStyle.Fill, WrapContents = true };

        private static GroupBox ResultsPlaceholder()
        {
            GroupBox results = new()
            {
                Text = Resources.AnalysisResults,
                Dock = DockStyle.Fill,
                Height = 150
            };
            FlowLayoutPanel content = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                Padding = new Padding(8),
                WrapContents = false,
            };
            content.Controls.Add(new Label { AutoSize = true, MaximumSize = new Size(380, 0), Text = Resources.AnalysisNotAvailable });
            content.Controls.Add(new Label { AutoSize = true, Text = Resources.AnalysisEmptyResults });
            content.Controls.Add(new Button { AutoSize = true, Enabled = false, Text = Resources.AnalysisStart });
            results.Controls.Add(content);
            return results;
        }

        private static TableLayoutPanel TabLayout(TabPage tab)
        {
            TableLayoutPanel layout = new()
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                ColumnCount = 2,
                RowCount = 5,
            };
            _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 490));
            _ = layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ = layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 165));
            tab.Controls.Add(layout);
            return layout;
        }

        private void BuildPositionTab()
        {
            TabPage tab = new(Resources.PositionAnalysis);
            this.modes.TabPages.Add(tab);
            TableLayoutPanel layout = TabLayout(tab);
            FlowLayoutPanel actions = Actions();
            actions.Controls.Add(ActionButton(Resources.LoadPosition, this.LoadPosition));
            actions.Controls.Add(ActionButton(Resources.AnalysisStartingPosition, () => this.SetPosition(Position.GetStartingPosition())));
            actions.Controls.Add(ActionButton(Resources.AnalysisFlipBoard, () => this.positionBoard.FlipBoard = !this.positionBoard.FlipBoard));
            actions.Controls.Add(ActionButton(Resources.SavePosition, () => PositionFileDialogs.Save(this, this.position.Clone())));
            layout.Controls.Add(actions, 0, 0);
            layout.SetColumnSpan(actions, 2);
            layout.Controls.Add(this.positionStatus, 0, 1);
            layout.SetColumnSpan(this.positionStatus, 2);
            layout.Controls.Add(this.positionBoard, 0, 2);
            layout.SetRowSpan(this.positionBoard, 3);
            Label notice = new()
            {
                AutoSize = true,
                MaximumSize = new Size(400, 0),
                Text = Resources.PositionAnalysisNotice
            };
            layout.Controls.Add(notice, 1, 2);
            layout.Controls.Add(this.positionFen, 1, 3);
            layout.Controls.Add(ResultsPlaceholder(), 1, 4);
            this.positionFen.AccessibleName = Resources.AnalysisFen;
        }

        private void BuildGameTab()
        {
            TabPage tab = new(Resources.GameAnalysis);
            this.modes.TabPages.Add(tab);
            TableLayoutPanel layout = TabLayout(tab);
            FlowLayoutPanel actions = Actions();
            actions.Controls.Add(ActionButton(Resources.ChooseGameFile, this.ChooseGame));
            actions.Controls.Add(ActionButton(Resources.AnalysisLoadExample, this.LoadExample));
            actions.Controls.Add(ActionButton(Resources.AnalysisFlipBoard, () => this.gameBoard.FlipBoard = !this.gameBoard.FlipBoard));
            actions.Controls.Add(this.gameSource);
            layout.Controls.Add(actions, 0, 0);
            layout.SetColumnSpan(actions, 2);
            FlowLayoutPanel details = Actions();
            details.Controls.Add(this.gameSummary);
            details.Controls.Add(this.gameWarnings);
            layout.Controls.Add(details, 0, 1);
            layout.SetColumnSpan(details, 2);
            layout.Controls.Add(this.gameBoard, 0, 2);
            TableLayoutPanel moves = new()
            {
                Dock = DockStyle.Fill,
                RowCount = 2,
                ColumnCount = 1
            };
            _ = moves.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _ = moves.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            moves.Controls.Add(new Label { AutoSize = true, Text = Resources.AnalysisMoves }, 0, 0);
            moves.Controls.Add(this.moveList, 0, 1);
            layout.Controls.Add(moves, 1, 2);
            FlowLayoutPanel navigation = Actions();
            navigation.Controls.Add(this.first);
            navigation.Controls.Add(this.previous);
            navigation.Controls.Add(this.next);
            navigation.Controls.Add(this.last);
            navigation.Controls.Add(this.gameStatus);
            layout.Controls.Add(navigation, 0, 3);
            layout.SetColumnSpan(navigation, 2);
            this.first.Click += (_, _) => this.SelectPly(0);
            this.previous.Click += (_, _) => this.SelectPly((this.timeline?.SelectedPly ?? 0) - 1);
            this.next.Click += (_, _) => this.SelectPly((this.timeline?.SelectedPly ?? 0) + 1);
            this.last.Click += (_, _) => this.SelectPly(this.timeline?.MoveCount ?? 0);
            this.moveList.SelectedIndexChanged += (_, _) => this.SelectPly(this.moveList.SelectedIndex);
            FlowLayoutPanel selected = new()
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
            };
            this.gameFen.Width = 470;
            selected.Controls.Add(new Label { AutoSize = true, Text = Resources.AnalysisFen });
            selected.Controls.Add(this.gameFen);
            selected.Controls.Add(ActionButton(Resources.AnalysisOpenPosition, this.OpenSelectedPosition));
            layout.Controls.Add(selected, 0, 4);
            layout.Controls.Add(ResultsPlaceholder(), 1, 4);
            this.gameSource.AccessibleName = Resources.ChooseGameFile;
            this.gameFen.AccessibleName = Resources.AnalysisFen;
            this.RefreshNavigation();
        }

        private void SetPosition(Position value)
        {
            _ = PositionFile.ParseFen(value.FEN_Notation);
            this.position = value.Clone();
            this.positionBoard.SetState(this.position);
            this.positionFen.Text = this.position.FEN_Notation;
            this.positionStatus.Text = this.position.OnMove == Colours.White ? Resources.AnalysisWhiteToMove : Resources.AnalysisBlackToMove;
        }

        private void LoadPosition()
        {
            if (PositionFileDialogs.TryLoad(this, out Position loaded))
            {
                this.SetPosition(loaded);
            }
        }

        private void ChooseGame()
        {
            using OpenFileDialog dialog = new()
            {
                Title = Resources.ChooseGameFile,
                Filter = WinFormsHelper.GameFileExtensionFilter,
                CheckFileExists = true,
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                this.LoadGame(dialog.FileName);
            }
        }

        private void LoadGame(String path)
        {
            try
            {
                this.SetGame(GameFile.Import(path), path);
            }
            catch (Exception error) when (PositionFileDialogs.IsFileError(error))
            {
                PositionFileDialogs.ShowError(this, error, path);
            }
        }

        private void SetGame(GameImportResult imported, String source)
        {
            AnalysisTimeline loaded = new(imported);
            this.timeline = loaded;
            this.gameSource.Text = source ?? String.Empty;
            this.gameSummary.Text = String.Format(CultureInfo.CurrentCulture, Resources.AnalysisGameSummary,
                imported.Game.Metadata?.White ?? Resources.UnknownPlayer, imported.Game.Metadata?.Black ?? Resources.UnknownPlayer,
                UiText.GameResultText(imported.Game.Result));
            this.gameWarnings.Text = String.Join(Environment.NewLine, imported.Warnings.Select(UiText.GameImportWarningText));
            this.moveList.BeginUpdate();
            this.moveList.Items.Clear();
            _ = this.moveList.Items.Add(Resources.AnalysisInitialPosition);
            for (Int32 index = 0; index < loaded.MoveCount; index++)
            {
                _ = this.moveList.Items.Add(loaded.GetMoveText(index));
            }
            this.moveList.EndUpdate();
            this.SelectPly(0);
        }

        private void SelectPly(Int32 ply)
        {
            if (this.timeline == null || ply < 0 || ply > this.timeline.MoveCount)
            {
                return;
            }
            this.timeline.Select(ply);
            Position selected = this.timeline.SelectedPosition;
            if (this.timeline.SelectedMove.HasValue)
            {
                this.gameBoard.SetState(selected, this.timeline.SelectedMove.Value);
            }
            else
            {
                this.gameBoard.SetState(selected);
            }
            this.gameFen.Text = selected.FEN_Notation;
            this.gameStatus.Text = String.Format(CultureInfo.CurrentCulture, Resources.AnalysisPlyStatus, ply, this.timeline.MoveCount,
                selected.OnMove == Colours.White ? Resources.AnalysisWhiteToMove : Resources.AnalysisBlackToMove);
            if (this.moveList.SelectedIndex != ply)
            {
                this.moveList.SelectedIndex = ply;
            }
            this.RefreshNavigation();
        }

        private void RefreshNavigation()
        {
            this.first.Enabled = this.previous.Enabled = this.timeline?.CanPrevious ?? false;
            this.next.Enabled = this.last.Enabled = this.timeline?.CanNext ?? false;
        }

        private void OpenSelectedPosition()
        {
            if (this.timeline != null)
            {
                this.SetPosition(this.timeline.SelectedPosition);
                this.modes.SelectedIndex = 0;
            }
        }

        protected override Boolean ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (this.modes.SelectedIndex == 1 && this.timeline != null)
            {
                switch (keyData)
                {
                    case Keys.Left:
                        this.SelectPly(Math.Max(0, this.timeline.SelectedPly - 1));
                        return true;
                    case Keys.Right:
                        this.SelectPly(Math.Min(this.timeline.MoveCount, this.timeline.SelectedPly + 1));
                        return true;
                    case Keys.Home:
                        this.SelectPly(0);
                        return true;
                    case Keys.End:
                        this.SelectPly(this.timeline.MoveCount);
                        return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
