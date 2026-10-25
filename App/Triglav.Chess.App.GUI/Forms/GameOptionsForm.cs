using System;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Model;
using Triglav.Chess.App.GUI.Properties;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Forms
{
    public partial class GameOptionsForm : Form
    {
        public GameOptionsForm()
        {
            this.InitializeComponent();
            this.opponentComboBox.Items.AddRange([Resources.OpponentEngine, Resources.OpponentFriend]);
            this.algorithmLevelComboBox.DataSource = new[]
            {
                new AlgorithmComboBoxItem(AlgorithmDepthLevel.RandomMoves, Resources.RandomMoves),
                new AlgorithmComboBoxItem(AlgorithmDepthLevel.Tourist, Resources.Tourist),
                new AlgorithmComboBoxItem(AlgorithmDepthLevel.Reenactor, Resources.Reenactor),
                new AlgorithmComboBoxItem(AlgorithmDepthLevel.PrinceKaspian, Resources.PrinceKaspian),
                new AlgorithmComboBoxItem(AlgorithmDepthLevel.Master, Resources.Master),
                new AlgorithmComboBoxItem(AlgorithmDepthLevel.Triglav, Resources.Triglav),
            };
            this.algorithmLevelComboBox.DisplayMember = nameof(AlgorithmComboBoxItem.Text);
            this.algorithmLevelComboBox.ValueMember = nameof(AlgorithmComboBoxItem.Value);
            this.algorithmLevelComboBox.SelectedValue = AlgorithmDepthLevel.Master;
            this.opponentComboBox.SelectedIndexChanged += (_, _) => this.UpdateOpponent();
            this.opponentComboBox.SelectedIndex = 0;
        }

        public GameOptions Options { get; private set; } = new();

        private void UpdateOpponent()
        {
            Boolean engine = this.opponentComboBox.SelectedIndex == 0;
            this.colourGroup.Visible = engine;
            this.algorithmLevelComboBox.Enabled = engine;
            this.checkBoxHumanMode.Enabled = engine;
            this.startFriendButton.Visible = !engine;
            this.AcceptButton = engine ? this.buttonWhite : this.startFriendButton;
            this.ClientSize = new System.Drawing.Size(720, engine ? 380 : 200);
        }

        private void StartGame(Colours colour)
        {
            this.Options.PlayWithFriend = this.opponentComboBox.SelectedIndex == 1;
            this.Options.ColourToPlay = this.Options.PlayWithFriend ? Colours.White : colour;
            this.Options.Level = (AlgorithmDepthLevel)this.algorithmLevelComboBox.SelectedValue;
            this.Options.HumanMode = !this.Options.PlayWithFriend && this.checkBoxHumanMode.Checked;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
