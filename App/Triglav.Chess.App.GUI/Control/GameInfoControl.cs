using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Engine.Core;

namespace Triglav.Chess.App.GUI.Control
{
    public partial class GameInfoControl : UserControl
    {
        private readonly RichTextBox pgnText = NotationTextBox.Create("pgnTextBox", Resources.GamePgn);

        public GameInfoControl()
        {
            this.InitializeComponent();
            this.labelCurrentEvaluationValue.Visible = false;
            this.labelMinMaxEvaluationValue.Visible = false;
            this.labelWinChanceEvaluationValue.Visible = false;
            GroupBox notation = new()
            {
                Text = Resources.GamePgn,
                Location = new Point(9, 190),
                Size = new Size(this.ClientSize.Width - 18, this.ClientSize.Height - 202),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Padding = new Padding(8),
            };
            notation.Controls.Add(this.pgnText);
            this.Controls.Add(notation);
        }

        public void SetPgn(String pgn) => NotationTextBox.Update(this.pgnText, pgn);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public EvaluationBar EvaluationBar
        {
            get; set;
        }

        public void EvaluationChanged(EvaluationWrapper currentEaluation) => this.labelCurrentEvaluationValue.Text = UiText.EvaluationText(currentEaluation);

        public void MinMaxEvaluationChanged(EvaluationWrapper currentEaluation) => this.labelMinMaxEvaluationValue.Text = UiText.EvaluationText(currentEaluation);

        public void WinChanceEvaluationChanged(EvaluationWrapper currentEaluation)
        {
            this.labelWinChanceEvaluationValue.Text = UiText.EvaluationText(currentEaluation, winningChances: true);
            this.EvaluationBar.EvaluationPercent = currentEaluation.WhiteWinChance;
        }

        public void SetLabelText(String yourMove) => this.labelStatus.Text = yourMove;

        private void checkBoxshowCurrentEvaluation_CheckedChanged(Object sender, EventArgs e) => this.labelCurrentEvaluationValue.Visible = this.checkBoxShowCurrentEvaluation.Checked;

        private void checkBoxShowMinMaxEvaluation_CheckedChanged(Object sender, EventArgs e) => this.labelMinMaxEvaluationValue.Visible = this.checkBoxShowMinMaxEvaluation.Checked;

        private void checkBoxShowWinChanceEvaluation_CheckedChanged(Object sender, EventArgs e)
        {
            this.labelWinChanceEvaluationValue.Visible = this.checkBoxShowWinChance.Checked;
            this.EvaluationBar.HideEvaluation = !this.checkBoxShowWinChance.Checked;
        }
    }
}
