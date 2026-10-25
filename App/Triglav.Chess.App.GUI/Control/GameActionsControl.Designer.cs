namespace Triglav.Chess.App.GUI.Control
{
    partial class GameActionsControl
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
            this.buttonDraw = new System.Windows.Forms.Button();
            this.buttonGiveUp = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonDraw
            // 
            this.buttonDraw.Location = new System.Drawing.Point(11, 8);
            this.buttonDraw.Name = "buttonDraw";
            this.buttonDraw.Size = new System.Drawing.Size(124, 28);
            this.buttonDraw.TabIndex = 0;
            this.buttonDraw.Text = global::Triglav.Chess.App.GUI.Properties.Resources.DrawOffer;
            this.buttonDraw.UseVisualStyleBackColor = true;
            this.buttonDraw.Click += this.buttonDraw_Click;
            // 
            // buttonGiveUp
            // 
            this.buttonGiveUp.Location = new System.Drawing.Point(233, 8);
            this.buttonGiveUp.Name = "buttonGiveUp";
            this.buttonGiveUp.Size = new System.Drawing.Size(124, 28);
            this.buttonGiveUp.TabIndex = 1;
            this.buttonGiveUp.Text = global::Triglav.Chess.App.GUI.Properties.Resources.GiveUp;
            this.buttonGiveUp.UseVisualStyleBackColor = true;
            this.buttonGiveUp.Click += this.buttonGiveUp_Click;
            // 
            // GameActionsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.buttonGiveUp);
            this.Controls.Add(this.buttonDraw);
            this.Name = "GameActionsControl";
            this.Size = new System.Drawing.Size(360, 44);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button buttonDraw;
        private System.Windows.Forms.Button buttonGiveUp;
    }
}
