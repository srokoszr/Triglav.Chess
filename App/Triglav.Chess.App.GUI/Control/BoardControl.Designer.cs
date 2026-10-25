namespace Triglav.Chess.App.GUI
{
    partial class BoardControl
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
            this.boardFieldsControl = new BoardFieldsControl();
            this.SuspendLayout();
            // 
            // boardFieldsControl
            // 
            this.boardFieldsControl.FieldSize = 40;
            this.boardFieldsControl.Location = new System.Drawing.Point(0, 0);
            this.boardFieldsControl.Name = "boardFieldsControl";
            this.boardFieldsControl.Size = new System.Drawing.Size(320, 320);
            this.boardFieldsControl.TabIndex = 0;
            // 
            // BoardControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.boardFieldsControl);
            this.Name = "BoardControl";
            this.Size = new System.Drawing.Size(360, 320);
            this.Load += this.BoardControl_Load;
            this.ResumeLayout(false);
        }

        #endregion

        private BoardFieldsControl boardFieldsControl;
    }
}
