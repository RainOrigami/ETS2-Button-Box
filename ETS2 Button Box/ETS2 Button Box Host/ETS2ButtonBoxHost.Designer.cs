
namespace ETS2_Button_Box_Host
{
    partial class ETS2ButtonBoxHost
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
            this.components = new System.ComponentModel.Container();
            this.cbCOMPort = new System.Windows.Forms.ComboBox();
            this.lblCOMPort = new System.Windows.Forms.Label();
            this.btnReload = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // cbCOMPort
            // 
            this.cbCOMPort.FormattingEnabled = true;
            this.cbCOMPort.Location = new System.Drawing.Point(74, 12);
            this.cbCOMPort.Name = "cbCOMPort";
            this.cbCOMPort.Size = new System.Drawing.Size(180, 21);
            this.cbCOMPort.TabIndex = 0;
            this.toolTips.SetToolTip(this.cbCOMPort, "List of available COM ports.\r\nSelect the COM port that is the button box.\r\nIf you" +
        " are not sure which one to choose try the one with the highest number.");
            // 
            // lblCOMPort
            // 
            this.lblCOMPort.AutoSize = true;
            this.lblCOMPort.Location = new System.Drawing.Point(12, 15);
            this.lblCOMPort.Name = "lblCOMPort";
            this.lblCOMPort.Size = new System.Drawing.Size(56, 13);
            this.lblCOMPort.TabIndex = 1;
            this.lblCOMPort.Text = "COM Port:";
            // 
            // btnReload
            // 
            this.btnReload.Image = global::ETS2_Button_Box_Host.Properties.Resources.Refresh_16x;
            this.btnReload.Location = new System.Drawing.Point(260, 10);
            this.btnReload.Name = "btnReload";
            this.btnReload.Size = new System.Drawing.Size(23, 23);
            this.btnReload.TabIndex = 4;
            this.toolTips.SetToolTip(this.btnReload, "Refresh the list of COM ports");
            this.btnReload.UseVisualStyleBackColor = true;
            this.btnReload.Click += new System.EventHandler(this.btnReload_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Image = global::ETS2_Button_Box_Host.Properties.Resources.Connect_16x;
            this.btnConnect.Location = new System.Drawing.Point(289, 10);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(23, 23);
            this.btnConnect.TabIndex = 2;
            this.toolTips.SetToolTip(this.btnConnect, "Connect to the selected COM port");
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // ETS2ButtonBoxHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(324, 46);
            this.Controls.Add(this.btnReload);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.lblCOMPort);
            this.Controls.Add(this.cbCOMPort);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "ETS2ButtonBoxHost";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "ETS2 Button Box Host";
            this.Load += new System.EventHandler(this.ETS2ButtonBoxHost_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cbCOMPort;
        private System.Windows.Forms.Label lblCOMPort;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnReload;
        private System.Windows.Forms.ToolTip toolTips;
    }
}

