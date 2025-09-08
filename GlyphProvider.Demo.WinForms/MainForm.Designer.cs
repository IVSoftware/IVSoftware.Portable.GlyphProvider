namespace GlyphProvider.Demo.WinForms
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            CounterBtn = new Button();
            SuspendLayout();
            // 
            // CounterBtn
            // 
            CounterBtn.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CounterBtn.Location = new Point(140, 405);
            CounterBtn.Name = "CounterBtn";
            CounterBtn.Size = new Size(221, 51);
            CounterBtn.TabIndex = 0;
            CounterBtn.Text = "Click me";
            CounterBtn.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(518, 904);
            Controls.Add(CounterBtn);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            ResumeLayout(false);
        }

        #endregion

        private Button CounterBtn;
    }
}
