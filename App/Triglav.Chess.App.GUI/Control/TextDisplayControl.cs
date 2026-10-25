using System.Drawing;
using System.Windows.Forms;

namespace Triglav.Chess.App.GUI
{
    public partial class TextDisplayControl : UserControl
    {
        public TextDisplayControl()
        {
            this.InitializeComponent();
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            // Optional defaults
            this.Font = new Font("Segoe UI Symbol", 14f, FontStyle.Regular);
            this.ForeColor = Color.Black;
            this.BackColor = Color.White;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(this.BackColor);

            using var brush = new SolidBrush(this.ForeColor);

            var format = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center,
                
            };

            e.Graphics.DrawString(
                this.Text,
                this.Font,
                brush,
                this.ClientRectangle,
                format);
        }
    }
}
