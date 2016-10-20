namespace Tester
{
	partial class FAsyncClient
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
			this.btnStop = new System.Windows.Forms.Button();
			this.btnStart = new System.Windows.Forms.Button();
			this.txtBuffer = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.txtIP = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.rtbConsole = new System.Windows.Forms.RichTextBox();
			this.txtSendMsg = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.chkHexString = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(719, 13);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 7;
			this.btnStop.Text = "斷線";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(638, 13);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 6;
			this.btnStart.Text = "連線";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// txtBuffer
			// 
			this.txtBuffer.Location = new System.Drawing.Point(415, 13);
			this.txtBuffer.Name = "txtBuffer";
			this.txtBuffer.Size = new System.Drawing.Size(60, 22);
			this.txtBuffer.TabIndex = 5;
			this.txtBuffer.Text = "1000";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(344, 18);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(65, 12);
			this.label4.TabIndex = 4;
			this.label4.Text = "緩衝區大小";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(267, 13);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(60, 22);
			this.txtPort.TabIndex = 3;
			// 
			// txtIP
			// 
			this.txtIP.BackColor = System.Drawing.SystemColors.Window;
			this.txtIP.Location = new System.Drawing.Point(93, 13);
			this.txtIP.Name = "txtIP";
			this.txtIP.Size = new System.Drawing.Size(100, 22);
			this.txtIP.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(208, 18);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "通訊埠號";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 18);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "伺服器IP位址";
			// 
			// rtbConsole
			// 
			this.rtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtbConsole.AutoWordSelection = true;
			this.rtbConsole.BackColor = System.Drawing.SystemColors.Window;
			this.rtbConsole.Font = new System.Drawing.Font("細明體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.rtbConsole.Location = new System.Drawing.Point(12, 52);
			this.rtbConsole.Name = "rtbConsole";
			this.rtbConsole.ReadOnly = true;
			this.rtbConsole.Size = new System.Drawing.Size(782, 313);
			this.rtbConsole.TabIndex = 8;
			this.rtbConsole.Text = "";
			// 
			// txtSendMsg
			// 
			this.txtSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSendMsg.Location = new System.Drawing.Point(69, 380);
			this.txtSendMsg.Name = "txtSendMsg";
			this.txtSendMsg.Size = new System.Drawing.Size(635, 22);
			this.txtSendMsg.TabIndex = 11;
			this.txtSendMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSendMsg_KeyDown);
			// 
			// label5
			// 
			this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(10, 385);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(53, 12);
			this.label5.TabIndex = 10;
			this.label5.Text = "訊息發送";
			// 
			// chkHexString
			// 
			this.chkHexString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.chkHexString.AutoSize = true;
			this.chkHexString.Location = new System.Drawing.Point(710, 382);
			this.chkHexString.Name = "chkHexString";
			this.chkHexString.Size = new System.Drawing.Size(84, 16);
			this.chkHexString.TabIndex = 9;
			this.chkHexString.Text = "16進位字串";
			this.chkHexString.UseVisualStyleBackColor = true;
			// 
			// FAsyncClient
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(807, 415);
			this.Controls.Add(this.chkHexString);
			this.Controls.Add(this.rtbConsole);
			this.Controls.Add(this.txtSendMsg);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtBuffer);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtPort);
			this.Controls.Add(this.txtIP);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FAsyncClient";
			this.Text = "AsyncClient 客戶端測試程式";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.TextBox txtBuffer;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.TextBox txtIP;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RichTextBox rtbConsole;
		private System.Windows.Forms.TextBox txtSendMsg;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox chkHexString;
	}
}