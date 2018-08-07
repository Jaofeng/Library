namespace Tester
{
	partial class FTelnetServer
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkPopupCommands = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnListen = new System.Windows.Forms.Button();
            this.txtAuth = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.gbClients = new System.Windows.Forms.GroupBox();
            this.lbRemotes = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.rtbConsole = new System.Windows.Forms.RichTextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.txtSendMsg = new System.Windows.Forms.TextBox();
            this.chkHexString = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.gbClients.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkPopupCommands);
            this.groupBox1.Controls.Add(this.btnStop);
            this.groupBox1.Controls.Add(this.btnListen);
            this.groupBox1.Controls.Add(this.txtAuth);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtPort);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 168);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Port";
            // 
            // chkPopupCommands
            // 
            this.chkPopupCommands.AutoSize = true;
            this.chkPopupCommands.Location = new System.Drawing.Point(43, 63);
            this.chkPopupCommands.Name = "chkPopupCommands";
            this.chkPopupCommands.Size = new System.Drawing.Size(220, 24);
            this.chkPopupCommands.TabIndex = 2;
            this.chkPopupCommands.Text = "Popup Telnet Commands";
            this.chkPopupCommands.UseVisualStyleBackColor = true;
            this.chkPopupCommands.CheckedChanged += new System.EventHandler(this.chkPopupCommands_CheckedChanged);
            // 
            // btnStop
            // 
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(145, 132);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(80, 28);
            this.btnStop.TabIndex = 6;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnListen
            // 
            this.btnListen.Location = new System.Drawing.Point(59, 132);
            this.btnListen.Name = "btnListen";
            this.btnListen.Size = new System.Drawing.Size(80, 28);
            this.btnListen.TabIndex = 5;
            this.btnListen.Text = "Listen";
            this.btnListen.UseVisualStyleBackColor = true;
            this.btnListen.Click += new System.EventHandler(this.btnListen_Click);
            // 
            // txtAuth
            // 
            this.txtAuth.Location = new System.Drawing.Point(59, 93);
            this.txtAuth.Name = "txtAuth";
            this.txtAuth.Size = new System.Drawing.Size(204, 29);
            this.txtAuth.TabIndex = 4;
            this.txtAuth.Text = "root,1234,admin,P@ssW0rd";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 98);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 20);
            this.label4.TabIndex = 3;
            this.label4.Text = "Auth.";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(59, 28);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(56, 29);
            this.txtPort.TabIndex = 1;
            this.txtPort.Text = "23";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Port";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.gbClients);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(5, 5);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(271, 441);
            this.panel1.TabIndex = 1;
            // 
            // gbClients
            // 
            this.gbClients.Controls.Add(this.lbRemotes);
            this.gbClients.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbClients.Location = new System.Drawing.Point(0, 173);
            this.gbClients.Name = "gbClients";
            this.gbClients.Size = new System.Drawing.Size(271, 268);
            this.gbClients.TabIndex = 2;
            this.gbClients.TabStop = false;
            this.gbClients.Text = "Clients : 0";
            // 
            // lbRemotes
            // 
            this.lbRemotes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbRemotes.FormattingEnabled = true;
            this.lbRemotes.ItemHeight = 20;
            this.lbRemotes.Location = new System.Drawing.Point(3, 25);
            this.lbRemotes.Name = "lbRemotes";
            this.lbRemotes.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lbRemotes.Size = new System.Drawing.Size(265, 240);
            this.lbRemotes.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Top;
            this.label2.Location = new System.Drawing.Point(0, 168);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(271, 5);
            this.label2.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.Dock = System.Windows.Forms.DockStyle.Left;
            this.label3.Location = new System.Drawing.Point(276, 5);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(5, 441);
            this.label3.TabIndex = 2;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.panel2);
            this.groupBox2.Controls.Add(this.panel3);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(281, 5);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(8, 3, 8, 7);
            this.groupBox2.Size = new System.Drawing.Size(608, 441);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Content";
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.rtbConsole);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(8, 25);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(592, 367);
            this.panel2.TabIndex = 0;
            // 
            // rtbConsole
            // 
            this.rtbConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbConsole.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbConsole.Font = new System.Drawing.Font("細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtbConsole.Location = new System.Drawing.Point(0, 0);
            this.rtbConsole.Name = "rtbConsole";
            this.rtbConsole.Size = new System.Drawing.Size(590, 365);
            this.rtbConsole.TabIndex = 1;
            this.rtbConsole.Text = "";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.txtSendMsg);
            this.panel3.Controls.Add(this.chkHexString);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(8, 392);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(592, 42);
            this.panel3.TabIndex = 1;
            // 
            // txtSendMsg
            // 
            this.txtSendMsg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSendMsg.Font = new System.Drawing.Font("細明體", 12F);
            this.txtSendMsg.Location = new System.Drawing.Point(3, 8);
            this.txtSendMsg.Name = "txtSendMsg";
            this.txtSendMsg.Size = new System.Drawing.Size(472, 27);
            this.txtSendMsg.TabIndex = 1;
            this.txtSendMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSendMsg_KeyDown);
            // 
            // chkHexString
            // 
            this.chkHexString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkHexString.AutoSize = true;
            this.chkHexString.Location = new System.Drawing.Point(481, 9);
            this.chkHexString.Name = "chkHexString";
            this.chkHexString.Size = new System.Drawing.Size(108, 24);
            this.chkHexString.TabIndex = 0;
            this.chkHexString.Text = "HEX String";
            this.chkHexString.UseVisualStyleBackColor = true;
            // 
            // FTelnetServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(894, 451);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("微軟正黑體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Name = "FTelnetServer";
            this.Padding = new System.Windows.Forms.Padding(5);
            this.Text = "FTelnetServer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FTelnetServer_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.gbClients.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnListen;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox gbClients;
		private System.Windows.Forms.ListBox lbRemotes;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.RichTextBox rtbConsole;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.TextBox txtSendMsg;
		private System.Windows.Forms.CheckBox chkHexString;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.CheckBox chkPopupCommands;
        private System.Windows.Forms.TextBox txtAuth;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
    }
}