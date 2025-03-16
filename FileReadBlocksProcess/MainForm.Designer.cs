namespace ImportValidateAnalyzeReporter
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
            btnAction = new Button();
            labelElapsed = new Label();
            checkBoxBreakMe = new CheckBox();
            SuspendLayout();
            // 
            // btnAction
            // 
            btnAction.Location = new Point(145, 98);
            btnAction.Name = "btnAction";
            btnAction.Size = new Size(234, 63);
            btnAction.TabIndex = 0;
            btnAction.Text = "Start";
            btnAction.UseVisualStyleBackColor = true;
            // 
            // labelElapsed
            // 
            labelElapsed.Location = new Point(145, 27);
            labelElapsed.Name = "labelElapsed";
            labelElapsed.Size = new Size(234, 56);
            labelElapsed.TabIndex = 1;
            labelElapsed.Text = "00:00:00";
            labelElapsed.TextAlign = ContentAlignment.MiddleCenter;
            labelElapsed.Visible = false;
            // 
            // checkBoxBreakMe
            // 
            checkBoxBreakMe.ForeColor = Color.Maroon;
            checkBoxBreakMe.Location = new Point(182, 179);
            checkBoxBreakMe.Name = "checkBoxBreakMe";
            checkBoxBreakMe.Size = new Size(153, 63);
            checkBoxBreakMe.TabIndex = 2;
            checkBoxBreakMe.Text = "Break Me";
            checkBoxBreakMe.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(528, 244);
            Controls.Add(checkBoxBreakMe);
            Controls.Add(labelElapsed);
            Controls.Add(btnAction);
            Font = new Font("Segoe UI", 12F);
            Margin = new Padding(4);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main Form";
            ResumeLayout(false);
        }

        #endregion

        private Button btnAction;
        private Label labelElapsed;
        private CheckBox checkBoxBreakMe;
    }
}
