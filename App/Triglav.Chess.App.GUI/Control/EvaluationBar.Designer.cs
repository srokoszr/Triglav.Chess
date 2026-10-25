namespace Triglav.Chess.App.GUI.Control
{
    partial class EvaluationBar
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.evaluationBarColourControlUpper = new EvaluationBarColourControl();
            this.evaluationBarColourControlLower = new EvaluationBarColourControl();
            this.SuspendLayout();
            // 
            // eValuationBarColourControlUpper
            // 
            this.evaluationBarColourControlUpper.BackColor = System.Drawing.Color.FromArgb(159, 89, 43);
            this.evaluationBarColourControlUpper.Location = new System.Drawing.Point(0, 0);
            this.evaluationBarColourControlUpper.Name = "eValuationBarColourControlUpper";
            this.evaluationBarColourControlUpper.Size = new System.Drawing.Size(40, 160);
            this.evaluationBarColourControlUpper.TabIndex = 0;
            // 
            // eValuationBarColourControlLower
            // 
            this.evaluationBarColourControlLower.BackColor = System.Drawing.Color.FromArgb(159, 89, 43);
            this.evaluationBarColourControlLower.Location = new System.Drawing.Point(0, 160);
            this.evaluationBarColourControlLower.Name = "eValuationBarColourControlLower";
            this.evaluationBarColourControlLower.Size = new System.Drawing.Size(40, 160);
            this.evaluationBarColourControlLower.TabIndex = 1;
            // 
            // EvaluationBar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.evaluationBarColourControlLower);
            this.Controls.Add(this.evaluationBarColourControlUpper);
            this.Name = "EvaluationBar";
            this.Size = new System.Drawing.Size(40, 320);
            this.ResumeLayout(false);
        }

        #endregion

        private EvaluationBarColourControl evaluationBarColourControlUpper;
        private EvaluationBarColourControl evaluationBarColourControlLower;
    }
}
