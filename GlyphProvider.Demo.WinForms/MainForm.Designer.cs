namespace IVSGlyphProvider.Demo.WinForms
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
            ActivatorTemplate<EnumIdButton> controlTemplate_11 = new ActivatorTemplate<EnumIdButton>();
            CounterBtn = new Button();
            centeringPanel = new CenteringPanel();
            SuspendLayout();
            // 
            // CounterBtn
            // 
            CounterBtn.BackColor = Color.FromArgb(81, 43, 212);
            CounterBtn.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            CounterBtn.ForeColor = Color.White;
            CounterBtn.Location = new Point(106, 405);
            CounterBtn.Name = "CounterBtn";
            CounterBtn.Size = new Size(298, 62);
            CounterBtn.TabIndex = 0;
            CounterBtn.Text = "Click me";
            CounterBtn.UseVisualStyleBackColor = false;
            // 
            // centeringPanel
            // 
            centeringPanel.BackColor = Color.LightBlue;
            centeringPanel.ActivatorTemplate = controlTemplate_11;
            centeringPanel.Dock = DockStyle.Bottom;
            centeringPanel.Location = new Point(0, 839);
            centeringPanel.Name = "centeringPanel";
            centeringPanel.RowHeightRequest = 31;
            centeringPanel.Size = new Size(518, 65);
            centeringPanel.TabIndex = 1;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(518, 904);
            Controls.Add(centeringPanel);
            Controls.Add(CounterBtn);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            ResumeLayout(false);
        }

        #endregion

        private Button CounterBtn;
        private CenteringPanel centeringPanel;
    }
}
