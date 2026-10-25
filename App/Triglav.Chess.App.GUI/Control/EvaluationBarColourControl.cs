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
    public partial class EvaluationBarColourControl : UserControl
    {
        public EvaluationBarColourControl()
        {
            InitializeComponent();
            Redraw();
        }

        private Colours evaluationBarColour = default;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Colours EvaluationBarColour
        {
            get
            {
                return this.evaluationBarColour;
            }
            set
            {
                this.evaluationBarColour = value;
                Redraw();
            }
        }

        private void Redraw()
        {
            this.BackColor = this.evaluationBarColour switch
            {
                Colours.White => GUIColours.White,
                Colours.Black => GUIColours.Black,
                _ => GUIColours.BackgroundColour,
            };

            this.Invalidate();
        }
    }
}
