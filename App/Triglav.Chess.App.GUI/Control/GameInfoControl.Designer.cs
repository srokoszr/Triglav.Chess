namespace Triglav.Chess.App.GUI.Control
{
    partial class GameInfoControl
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
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelCurrentEvaluation = new System.Windows.Forms.Label();
            this.labelCurrentEvaluationValue = new System.Windows.Forms.Label();
            this.checkBoxShowCurrentEvaluation = new System.Windows.Forms.CheckBox();
            this.checkBoxShowMinMaxEvaluation = new System.Windows.Forms.CheckBox();
            this.labelMinMaxEvaluationValue = new System.Windows.Forms.Label();
            this.labelMinMaxEvaluation = new System.Windows.Forms.Label();
            this.checkBoxShowWinChance = new System.Windows.Forms.CheckBox();
            this.labelWinChanceEvaluationValue = new System.Windows.Forms.Label();
            this.labelWinChanceEvaluation = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.labelStatus.Location = new System.Drawing.Point(9, 8);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(52, 21);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = global::Triglav.Chess.App.GUI.Properties.Resources.Status;
            // 
            // labelCurrentEvaluation
            // 
            this.labelCurrentEvaluation.AutoSize = true;
            this.labelCurrentEvaluation.Location = new System.Drawing.Point(9, 77);
            this.labelCurrentEvaluation.Name = "labelCurrentEvaluation";
            this.labelCurrentEvaluation.Size = new System.Drawing.Size(108, 15);
            this.labelCurrentEvaluation.TabIndex = 1;
            this.labelCurrentEvaluation.Text = global::Triglav.Chess.App.GUI.Properties.Resources.CurrentEvaluation;
            // 
            // labelCurrentEvaluationValue
            // 
            this.labelCurrentEvaluationValue.AutoSize = true;
            this.labelCurrentEvaluationValue.Location = new System.Drawing.Point(123, 77);
            this.labelCurrentEvaluationValue.Name = "labelCurrentEvaluationValue";
            this.labelCurrentEvaluationValue.Size = new System.Drawing.Size(93, 15);
            this.labelCurrentEvaluationValue.TabIndex = 2;
            this.labelCurrentEvaluationValue.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EvaluationValue;
            // 
            // checkBoxShowCurrentEvaluation
            // 
            this.checkBoxShowCurrentEvaluation.AutoSize = true;
            this.checkBoxShowCurrentEvaluation.Location = new System.Drawing.Point(9, 55);
            this.checkBoxShowCurrentEvaluation.Name = "checkBoxShowCurrentEvaluation";
            this.checkBoxShowCurrentEvaluation.Size = new System.Drawing.Size(156, 19);
            this.checkBoxShowCurrentEvaluation.TabIndex = 3;
            this.checkBoxShowCurrentEvaluation.Text = global::Triglav.Chess.App.GUI.Properties.Resources.ShowCurrentEvaluation;
            this.checkBoxShowCurrentEvaluation.UseVisualStyleBackColor = true;
            this.checkBoxShowCurrentEvaluation.CheckedChanged += this.checkBoxshowCurrentEvaluation_CheckedChanged;
            // 
            // checkBoxShowMinMaxEvaluation
            // 
            this.checkBoxShowMinMaxEvaluation.AutoSize = true;
            this.checkBoxShowMinMaxEvaluation.Location = new System.Drawing.Point(9, 95);
            this.checkBoxShowMinMaxEvaluation.Name = "checkBoxShowMinMaxEvaluation";
            this.checkBoxShowMinMaxEvaluation.Size = new System.Drawing.Size(160, 19);
            this.checkBoxShowMinMaxEvaluation.TabIndex = 6;
            this.checkBoxShowMinMaxEvaluation.Text = global::Triglav.Chess.App.GUI.Properties.Resources.ShowMinMaxEvaluation;
            this.checkBoxShowMinMaxEvaluation.UseVisualStyleBackColor = true;
            this.checkBoxShowMinMaxEvaluation.CheckedChanged += this.checkBoxShowMinMaxEvaluation_CheckedChanged;
            // 
            // labelMinMaxEvaluationValue
            // 
            this.labelMinMaxEvaluationValue.AutoSize = true;
            this.labelMinMaxEvaluationValue.Location = new System.Drawing.Point(123, 117);
            this.labelMinMaxEvaluationValue.Name = "labelMinMaxEvaluationValue";
            this.labelMinMaxEvaluationValue.Size = new System.Drawing.Size(93, 15);
            this.labelMinMaxEvaluationValue.TabIndex = 5;
            this.labelMinMaxEvaluationValue.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EvaluationValue;
            // 
            // labelMinMaxEvaluation
            // 
            this.labelMinMaxEvaluation.AutoSize = true;
            this.labelMinMaxEvaluation.Location = new System.Drawing.Point(9, 117);
            this.labelMinMaxEvaluation.Name = "labelMinMaxEvaluation";
            this.labelMinMaxEvaluation.Size = new System.Drawing.Size(112, 15);
            this.labelMinMaxEvaluation.TabIndex = 4;
            this.labelMinMaxEvaluation.Text = global::Triglav.Chess.App.GUI.Properties.Resources.MinMaxEvaluation;
            // 
            // checkBoxShowWinChance
            // 
            this.checkBoxShowWinChance.AutoSize = true;
            this.checkBoxShowWinChance.Location = new System.Drawing.Point(9, 135);
            this.checkBoxShowWinChance.Name = "checkBoxShowWinChance";
            this.checkBoxShowWinChance.Size = new System.Drawing.Size(122, 19);
            this.checkBoxShowWinChance.TabIndex = 9;
            this.checkBoxShowWinChance.Text = global::Triglav.Chess.App.GUI.Properties.Resources.ShowWinChance;
            this.checkBoxShowWinChance.UseVisualStyleBackColor = true;
            this.checkBoxShowWinChance.CheckedChanged += this.checkBoxShowWinChanceEvaluation_CheckedChanged;
            // 
            // labelWinChanceEvaluationValue
            // 
            this.labelWinChanceEvaluationValue.AutoSize = true;
            this.labelWinChanceEvaluationValue.Location = new System.Drawing.Point(123, 157);
            this.labelWinChanceEvaluationValue.Name = "labelWinChanceEvaluationValue";
            this.labelWinChanceEvaluationValue.Size = new System.Drawing.Size(93, 15);
            this.labelWinChanceEvaluationValue.TabIndex = 8;
            this.labelWinChanceEvaluationValue.Text = global::Triglav.Chess.App.GUI.Properties.Resources.EvaluationValue;
            // 
            // labelWinChanceEvaluation
            // 
            this.labelWinChanceEvaluation.AutoSize = true;
            this.labelWinChanceEvaluation.Location = new System.Drawing.Point(9, 157);
            this.labelWinChanceEvaluation.Name = "labelWinChanceEvaluation";
            this.labelWinChanceEvaluation.Size = new System.Drawing.Size(74, 15);
            this.labelWinChanceEvaluation.TabIndex = 7;
            this.labelWinChanceEvaluation.Text = global::Triglav.Chess.App.GUI.Properties.Resources.WinChance;
            // 
            // GameInfoControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.checkBoxShowWinChance);
            this.Controls.Add(this.labelWinChanceEvaluationValue);
            this.Controls.Add(this.labelWinChanceEvaluation);
            this.Controls.Add(this.checkBoxShowMinMaxEvaluation);
            this.Controls.Add(this.labelMinMaxEvaluationValue);
            this.Controls.Add(this.labelMinMaxEvaluation);
            this.Controls.Add(this.checkBoxShowCurrentEvaluation);
            this.Controls.Add(this.labelCurrentEvaluationValue);
            this.Controls.Add(this.labelCurrentEvaluation);
            this.Controls.Add(this.labelStatus);
            this.Name = "GameInfoControl";
            this.Size = new System.Drawing.Size(346, 476);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelCurrentEvaluation;
        private System.Windows.Forms.Label labelCurrentEvaluationValue;
        private System.Windows.Forms.CheckBox checkBoxShowCurrentEvaluation;
        private System.Windows.Forms.CheckBox checkBoxShowMinMaxEvaluation;
        private System.Windows.Forms.Label labelMinMaxEvaluationValue;
        private System.Windows.Forms.Label labelMinMaxEvaluation;
        private System.Windows.Forms.CheckBox checkBoxShowWinChance;
        private System.Windows.Forms.Label labelWinChanceEvaluationValue;
        private System.Windows.Forms.Label labelWinChanceEvaluation;
    }
}
