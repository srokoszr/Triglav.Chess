using Triglav.Chess.Model.Enumerations;

namespace Triglav.Chess.App.GUI.Model
{
    public class AlgorithmComboBoxItem
    {
        public AlgorithmDepthLevel Value { get; set; }

        public string Text { get; set; }

        public AlgorithmComboBoxItem(AlgorithmDepthLevel value, string text)
        {
            this.Value = value;
            this.Text = text;
        }
    }
}