using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Triglav.Chess.App.GUI.Model.Enumerations;
using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Control
{
    public partial class EvaluationBar : UserControl
    {
        public EvaluationBar()
        {
            InitializeComponent();
            Redraw();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Redraw();
        }

        private bool flipBoard = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool FlipBoard
        {
            get
            {
                return flipBoard;
            }
            set
            {
                flipBoard = value;
                Redraw();
            }
        }

        private bool hideEvaluation = true;


        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HideEvaluation
        {
            get
            {
                return hideEvaluation;
            }
            set
            {
                hideEvaluation = value;
                Redraw();
            }
        }

        private double evaluationPercent = 50f;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double EvaluationPercent
        {
            get
            {
                return evaluationPercent;
            }
            set
            {
                evaluationPercent = value;
                Redraw();
            }
        }

        private void Redraw()
        {
            var height = this.Height;
            var width = this.Width;
            this.evaluationBarColourControlUpper.Width = width;
            this.evaluationBarColourControlLower.Width = width;

            var percent = 100f - this.EvaluationPercent;
            if(this.flipBoard)
            {
                percent = 100f - percent;
            }
            if(this.hideEvaluation)
            {
                percent = 50f;
            }

            var midpoint = (height * percent) / 100f;

            this.evaluationBarColourControlUpper.Location = new Point(0, 0);
            this.evaluationBarColourControlLower.Location = new Point(0, (int)midpoint);
            this.evaluationBarColourControlUpper.Size = new Size(width, (int)midpoint);
            this.evaluationBarColourControlLower.Size = new Size(width, height - (int)midpoint);

            if (this.flipBoard)
            {
                this.evaluationBarColourControlUpper.EvaluationBarColour = Colours.White;
                this.evaluationBarColourControlLower.EvaluationBarColour = Colours.Black;
            }
            else
            {
                this.evaluationBarColourControlUpper.EvaluationBarColour = Colours.Black;
                this.evaluationBarColourControlLower.EvaluationBarColour = Colours.White;
            }

            this.Invalidate();
        }
    }
}
