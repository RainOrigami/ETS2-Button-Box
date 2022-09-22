namespace ETS2_Button_Box_Host_Test
{
    partial class HostTest
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.cbCOMPort = new System.Windows.Forms.ComboBox();
            this.txtConnect = new System.Windows.Forms.Button();
            this.nudLedIndex = new System.Windows.Forms.NumericUpDown();
            this.btnSetLed = new System.Windows.Forms.Button();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.txtButtonStates = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.lblBrightness = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.nudLedIndex)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // cbCOMPort
            // 
            this.cbCOMPort.FormattingEnabled = true;
            this.cbCOMPort.Location = new System.Drawing.Point(12, 12);
            this.cbCOMPort.Name = "cbCOMPort";
            this.cbCOMPort.Size = new System.Drawing.Size(121, 23);
            this.cbCOMPort.TabIndex = 0;
            // 
            // txtConnect
            // 
            this.txtConnect.Location = new System.Drawing.Point(139, 12);
            this.txtConnect.Name = "txtConnect";
            this.txtConnect.Size = new System.Drawing.Size(75, 23);
            this.txtConnect.TabIndex = 1;
            this.txtConnect.Text = "Connect";
            this.txtConnect.UseVisualStyleBackColor = true;
            this.txtConnect.Click += new System.EventHandler(this.txtConnect_Click);
            // 
            // nudLedIndex
            // 
            this.nudLedIndex.Location = new System.Drawing.Point(13, 41);
            this.nudLedIndex.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudLedIndex.Name = "nudLedIndex";
            this.nudLedIndex.Size = new System.Drawing.Size(120, 23);
            this.nudLedIndex.TabIndex = 2;
            // 
            // btnSetLed
            // 
            this.btnSetLed.Location = new System.Drawing.Point(139, 41);
            this.btnSetLed.Name = "btnSetLed";
            this.btnSetLed.Size = new System.Drawing.Size(75, 23);
            this.btnSetLed.TabIndex = 4;
            this.btnSetLed.Text = "Set";
            this.btnSetLed.UseVisualStyleBackColor = true;
            this.btnSetLed.Click += new System.EventHandler(this.btnSetLed_Click);
            // 
            // txtButtonStates
            // 
            this.txtButtonStates.AutoSize = true;
            this.txtButtonStates.Location = new System.Drawing.Point(13, 67);
            this.txtButtonStates.Name = "txtButtonStates";
            this.txtButtonStates.Size = new System.Drawing.Size(0, 15);
            this.txtButtonStates.TabIndex = 5;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(12, 70);
            this.trackBar1.Maximum = 255;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(104, 45);
            this.trackBar1.TabIndex = 6;
            this.trackBar1.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // lblBrightness
            // 
            this.lblBrightness.AutoSize = true;
            this.lblBrightness.Location = new System.Drawing.Point(122, 70);
            this.lblBrightness.Name = "lblBrightness";
            this.lblBrightness.Size = new System.Drawing.Size(12, 15);
            this.lblBrightness.TabIndex = 7;
            this.lblBrightness.Text = "-";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 121);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(732, 317);
            this.textBox1.TabIndex = 8;
            // 
            // HostTest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.lblBrightness);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.txtButtonStates);
            this.Controls.Add(this.btnSetLed);
            this.Controls.Add(this.nudLedIndex);
            this.Controls.Add(this.txtConnect);
            this.Controls.Add(this.cbCOMPort);
            this.Name = "HostTest";
            this.Text = "HostTest";
            this.Load += new System.EventHandler(this.HostTest_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudLedIndex)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox cbCOMPort;
        private Button txtConnect;
        private NumericUpDown nudLedIndex;
        private Button btnSetLed;
        private ColorDialog colorDialog;
        private Label txtButtonStates;
        private TrackBar trackBar1;
        private Label lblBrightness;
        private TextBox textBox1;
    }
}