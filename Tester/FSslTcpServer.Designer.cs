namespace Tester
{
	partial class FSslTcpServer
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
			this.label1 = new System.Windows.Forms.Label();
			this.txtIP = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.btnStart = new System.Windows.Forms.Button();
			this.btnStop = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.txtMaxConnect = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtSendMsg = new System.Windows.Forms.TextBox();
			this.rtbConsole = new System.Windows.Forms.RichTextBox();
			this.chkHexString = new System.Windows.Forms.CheckBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtPfx = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.txtPfxPwd = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(24, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "IP位址";
			// 
			// txtIP
			// 
			this.txtIP.BackColor = System.Drawing.SystemColors.Window;
			this.txtIP.Location = new System.Drawing.Point(69, 10);
			this.txtIP.Name = "txtIP";
			this.txtIP.Size = new System.Drawing.Size(88, 22);
			this.txtIP.TabIndex = 1;
			this.txtIP.Text = "192.168.127.100";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(180, 15);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "通訊埠號";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(239, 10);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(60, 22);
			this.txtPort.TabIndex = 3;
			this.txtPort.Text = "8089";
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(460, 10);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 8;
			this.btnStart.Text = "啟動";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(460, 40);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 9;
			this.btnStop.Text = "停止";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Enabled = false;
			this.label3.Location = new System.Drawing.Point(321, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 12);
			this.label3.TabIndex = 4;
			this.label3.Text = "最大連線數";
			// 
			// txtMaxConnect
			// 
			this.txtMaxConnect.Enabled = false;
			this.txtMaxConnect.Location = new System.Drawing.Point(392, 10);
			this.txtMaxConnect.Name = "txtMaxConnect";
			this.txtMaxConnect.Size = new System.Drawing.Size(60, 22);
			this.txtMaxConnect.TabIndex = 5;
			this.txtMaxConnect.Text = "10";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 279);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(53, 12);
			this.label5.TabIndex = 12;
			this.label5.Text = "訊息發送";
			// 
			// txtSendMsg
			// 
			this.txtSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSendMsg.Location = new System.Drawing.Point(69, 274);
			this.txtSendMsg.Name = "txtSendMsg";
			this.txtSendMsg.Size = new System.Drawing.Size(375, 22);
			this.txtSendMsg.TabIndex = 13;
			this.txtSendMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSendMsg_KeyDown);
			// 
			// rtbConsole
			// 
			this.rtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtbConsole.AutoWordSelection = true;
			this.rtbConsole.BackColor = System.Drawing.SystemColors.Window;
			this.rtbConsole.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtbConsole.Location = new System.Drawing.Point(12, 68);
			this.rtbConsole.Name = "rtbConsole";
			this.rtbConsole.Size = new System.Drawing.Size(524, 191);
			this.rtbConsole.TabIndex = 10;
			this.rtbConsole.Text = "";
			// 
			// chkHexString
			// 
			this.chkHexString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkHexString.AutoSize = true;
			this.chkHexString.Location = new System.Drawing.Point(451, 277);
			this.chkHexString.Name = "chkHexString";
			this.chkHexString.Size = new System.Drawing.Size(84, 16);
			this.chkHexString.TabIndex = 11;
			this.chkHexString.Text = "16進位字串";
			this.chkHexString.UseVisualStyleBackColor = true;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(24, 45);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(29, 12);
			this.label4.TabIndex = 0;
			this.label4.Text = "憑證";
			// 
			// txtPfx
			// 
			this.txtPfx.BackColor = System.Drawing.SystemColors.Window;
			this.txtPfx.Location = new System.Drawing.Point(69, 40);
			this.txtPfx.Name = "txtPfx";
			this.txtPfx.Size = new System.Drawing.Size(173, 22);
			this.txtPfx.TabIndex = 1;
			this.txtPfx.Text = "CJF.Net.SslTcpServer.pfx";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(264, 45);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(53, 12);
			this.label6.TabIndex = 4;
			this.label6.Text = "憑證密碼";
			// 
			// txtPfxPwd
			// 
			this.txtPfxPwd.Location = new System.Drawing.Point(323, 40);
			this.txtPfxPwd.Name = "txtPfxPwd";
			this.txtPfxPwd.Size = new System.Drawing.Size(129, 22);
			this.txtPfxPwd.TabIndex = 5;
			this.txtPfxPwd.Text = "CJF.Net.SslTcpServer";
			// 
			// FSslTcpServer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(548, 308);
			this.Controls.Add(this.chkHexString);
			this.Controls.Add(this.rtbConsole);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtPfxPwd);
			this.Controls.Add(this.txtMaxConnect);
			this.Controls.Add(this.txtSendMsg);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtPfx);
			this.Controls.Add(this.txtIP);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FSslTcpServer";
			this.Text = "SSL TCP 伺服端測試程式";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtIP;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtMaxConnect;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtSendMsg;
		private System.Windows.Forms.RichTextBox rtbConsole;
		private System.Windows.Forms.CheckBox chkHexString;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtPfx;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtPfxPwd;
	}
}