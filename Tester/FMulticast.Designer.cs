namespace Tester
{
	partial class FMulticast
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
			this.label5 = new System.Windows.Forms.Label();
			this.txtSendMsg = new System.Windows.Forms.TextBox();
			this.rtbConsole = new System.Windows.Forms.RichTextBox();
			this.txtSendIP = new System.Windows.Forms.TextBox();
			this.chkHexString = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(24, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(42, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "群組 IP";
			// 
			// txtIP
			// 
			this.txtIP.BackColor = System.Drawing.SystemColors.Window;
			this.txtIP.Location = new System.Drawing.Point(69, 12);
			this.txtIP.Name = "txtIP";
			this.txtIP.Size = new System.Drawing.Size(387, 22);
			this.txtIP.TabIndex = 1;
			this.txtIP.Text = "224.100.0.1,224.100.0.2";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(473, 19);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "通訊埠號";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(532, 14);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(60, 22);
			this.txtPort.TabIndex = 3;
			this.txtPort.Text = "9900";
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(614, 12);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 4;
			this.btnStart.Text = "啟動";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(695, 12);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 5;
			this.btnStop.Text = "停止";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 323);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(53, 12);
			this.label5.TabIndex = 6;
			this.label5.Text = "訊息發送";
			// 
			// txtSendMsg
			// 
			this.txtSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSendMsg.Location = new System.Drawing.Point(189, 318);
			this.txtSendMsg.Name = "txtSendMsg";
			this.txtSendMsg.Size = new System.Drawing.Size(491, 22);
			this.txtSendMsg.TabIndex = 9;
			this.txtSendMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSendMsg_KeyDown);
			// 
			// rtbConsole
			// 
			this.rtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtbConsole.AutoWordSelection = true;
			this.rtbConsole.BackColor = System.Drawing.SystemColors.Window;
			this.rtbConsole.Font = new System.Drawing.Font("細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.rtbConsole.Location = new System.Drawing.Point(12, 42);
			this.rtbConsole.Name = "rtbConsole";
			this.rtbConsole.ReadOnly = true;
			this.rtbConsole.Size = new System.Drawing.Size(758, 270);
			this.rtbConsole.TabIndex = 10;
			this.rtbConsole.TabStop = false;
			this.rtbConsole.Text = "";
			// 
			// txtSendIP
			// 
			this.txtSendIP.BackColor = System.Drawing.SystemColors.Window;
			this.txtSendIP.Location = new System.Drawing.Point(69, 318);
			this.txtSendIP.Name = "txtSendIP";
			this.txtSendIP.Size = new System.Drawing.Size(114, 22);
			this.txtSendIP.TabIndex = 7;
			this.txtSendIP.Text = "224.100.0.1:9900:3";
			this.txtSendIP.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSendIP_KeyDown);
			// 
			// chkHexString
			// 
			this.chkHexString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkHexString.AutoSize = true;
			this.chkHexString.Location = new System.Drawing.Point(686, 321);
			this.chkHexString.Name = "chkHexString";
			this.chkHexString.Size = new System.Drawing.Size(84, 16);
			this.chkHexString.TabIndex = 8;
			this.chkHexString.Text = "16進位字串";
			this.chkHexString.UseVisualStyleBackColor = true;
			// 
			// FMulticast
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(782, 352);
			this.Controls.Add(this.chkHexString);
			this.Controls.Add(this.rtbConsole);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtSendMsg);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtSendIP);
			this.Controls.Add(this.txtIP);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FMulticast";
			this.Text = "Multicast 測試程式";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FMulticast_FormClosed);
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
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtSendMsg;
		private System.Windows.Forms.RichTextBox rtbConsole;
		private System.Windows.Forms.TextBox txtSendIP;
		private System.Windows.Forms.CheckBox chkHexString;
	}
}