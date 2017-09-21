namespace Tester
{
	partial class FWebSocketServer
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
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(24, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "IP位址";
			// 
			// txtIP
			// 
			this.txtIP.BackColor = System.Drawing.SystemColors.Window;
			this.txtIP.Location = new System.Drawing.Point(69, 12);
			this.txtIP.Name = "txtIP";
			this.txtIP.Size = new System.Drawing.Size(88, 22);
			this.txtIP.TabIndex = 1;
			this.txtIP.Text = "192.168.127.100";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(180, 17);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "通訊埠號";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(239, 12);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(60, 22);
			this.txtPort.TabIndex = 3;
			this.txtPort.Text = "8089";
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(466, 12);
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
			this.btnStop.Location = new System.Drawing.Point(547, 12);
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
			this.label3.Location = new System.Drawing.Point(321, 17);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(65, 12);
			this.label3.TabIndex = 4;
			this.label3.Text = "最大連線數";
			// 
			// txtMaxConnect
			// 
			this.txtMaxConnect.Location = new System.Drawing.Point(392, 12);
			this.txtMaxConnect.Name = "txtMaxConnect";
			this.txtMaxConnect.Size = new System.Drawing.Size(60, 22);
			this.txtMaxConnect.TabIndex = 5;
			this.txtMaxConnect.Text = "10";
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 308);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(53, 12);
			this.label5.TabIndex = 12;
			this.label5.Text = "訊息發送";
			// 
			// txtSendMsg
			// 
			this.txtSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSendMsg.Location = new System.Drawing.Point(69, 303);
			this.txtSendMsg.Name = "txtSendMsg";
			this.txtSendMsg.Size = new System.Drawing.Size(461, 22);
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
			this.rtbConsole.Location = new System.Drawing.Point(12, 50);
			this.rtbConsole.Name = "rtbConsole";
			this.rtbConsole.Size = new System.Drawing.Size(610, 238);
			this.rtbConsole.TabIndex = 10;
			this.rtbConsole.Text = "";
			// 
			// chkHexString
			// 
			this.chkHexString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkHexString.AutoSize = true;
			this.chkHexString.Location = new System.Drawing.Point(537, 306);
			this.chkHexString.Name = "chkHexString";
			this.chkHexString.Size = new System.Drawing.Size(84, 16);
			this.chkHexString.TabIndex = 11;
			this.chkHexString.Text = "16進位字串";
			this.chkHexString.UseVisualStyleBackColor = true;
			// 
			// FWebSocketServer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(634, 337);
			this.Controls.Add(this.chkHexString);
			this.Controls.Add(this.rtbConsole);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtMaxConnect);
			this.Controls.Add(this.txtSendMsg);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtIP);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FWebSocketServer";
			this.Text = "WebSocket Server 伺服端測試程式";
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
	}
}