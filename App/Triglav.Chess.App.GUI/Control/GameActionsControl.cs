using System;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Properties;

namespace Triglav.Chess.App.GUI.Control
{
    public partial class GameActionsControl : UserControl
    {
        public GameActionsControl()
        {
            InitializeComponent();
            this.buttonDraw.Text = Resources.DrawOffer;
            this.buttonGiveUp.Text = Resources.GiveUp;
        }

        public event EventHandler OnResign;

        private void buttonGiveUp_Click(object sender, System.EventArgs e)
        {
            this.OnResign?.Invoke(this, e);
        }

        public event EventHandler OnDrawOffer;

        private void buttonDraw_Click(object sender, System.EventArgs e)
        {
            this.OnDrawOffer?.Invoke(this, e);
        }
    }
}
