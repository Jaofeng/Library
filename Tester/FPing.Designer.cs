namespace Tester
{
	partial class FPing
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
			this.btnStart = new System.Windows.Forms.Button();
			this.txtTimeout = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.txtCycle = new System.Windows.Forms.TextBox();
			this.txtRemote = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.rtbConsole = new System.Windows.Forms.RichTextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.txtDataLength = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtTimes = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.btnStop = new System.Windows.Forms.Button();
			this.txtTTL = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnStart.Location = new System.Drawing.Point(36, 204);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 12;
			this.btnStart.Text = "開始";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// txtTimeout
			// 
			this.txtTimeout.Location = new System.Drawing.Point(93, 124);
			this.txtTimeout.Name = "txtTimeout";
			this.txtTimeout.Size = new System.Drawing.Size(37, 22);
			this.txtTimeout.TabIndex = 11;
			this.txtTimeout.Text = "120";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(13, 129);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(74, 12);
			this.label4.TabIndex = 10;
			this.label4.Text = "逾時時間(ms)";
			// 
			// txtCycle
			// 
			this.txtCycle.Location = new System.Drawing.Point(93, 96);
			this.txtCycle.Name = "txtCycle";
			this.txtCycle.Size = new System.Drawing.Size(39, 22);
			this.txtCycle.TabIndex = 9;
			this.txtCycle.Text = "3000";
			// 
			// txtRemote
			// 
			this.txtRemote.BackColor = System.Drawing.SystemColors.Window;
			this.txtRemote.Location = new System.Drawing.Point(93, 12);
			this.txtRemote.Name = "txtRemote";
			this.txtRemote.Size = new System.Drawing.Size(133, 22);
			this.txtRemote.TabIndex = 1;
			this.txtRemote.Text = "127.0.0.1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(13, 101);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(74, 12);
			this.label2.TabIndex = 8;
			this.label2.Text = "發送週期(ms)";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 17);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(75, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "主機名稱或IP";
			// 
			// rtbConsole
			// 
			this.rtbConsole.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.rtbConsole.AutoWordSelection = true;
			this.rtbConsole.BackColor = System.Drawing.SystemColors.Window;
			this.rtbConsole.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rtbConsole.Location = new System.Drawing.Point(232, 12);
			this.rtbConsole.Name = "rtbConsole";
			this.rtbConsole.ReadOnly = true;
			this.rtbConsole.Size = new System.Drawing.Size(422, 215);
			this.rtbConsole.TabIndex = 14;
			this.rtbConsole.Text = "";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(34, 73);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(53, 12);
			this.label3.TabIndex = 5;
			this.label3.Text = "資料長度";
			// 
			// txtDataLength
			// 
			this.txtDataLength.Location = new System.Drawing.Point(93, 68);
			this.txtDataLength.Name = "txtDataLength";
			this.txtDataLength.Size = new System.Drawing.Size(39, 22);
			this.txtDataLength.TabIndex = 6;
			this.txtDataLength.Text = "32";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(34, 45);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(53, 12);
			this.label5.TabIndex = 2;
			this.label5.Text = "發送次數";
			// 
			// txtTimes
			// 
			this.txtTimes.Location = new System.Drawing.Point(93, 40);
			this.txtTimes.Name = "txtTimes";
			this.txtTimes.Size = new System.Drawing.Size(37, 22);
			this.txtTimes.TabIndex = 3;
			this.txtTimes.Text = "0";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(138, 45);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(58, 12);
			this.label6.TabIndex = 4;
			this.label6.Text = "(0 不限制)";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(138, 73);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(73, 12);
			this.label7.TabIndex = 7;
			this.label7.Text = "(最大 65,500)";
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(117, 204);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 13;
			this.btnStop.Text = "停止";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// txtTTL
			// 
			this.txtTTL.Location = new System.Drawing.Point(93, 152);
			this.txtTTL.Name = "txtTTL";
			this.txtTTL.Size = new System.Drawing.Size(37, 22);
			this.txtTTL.TabIndex = 15;
			this.txtTTL.Text = "128";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(34, 157);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(53, 12);
			this.label8.TabIndex = 14;
			this.label8.Text = "轉送次數";
			// 
			// FPing
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(667, 239);
			this.Controls.Add(this.txtTTL);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.rtbConsole);
			this.Controls.Add(this.btnStop);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.txtTimes);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.txtTimeout);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.txtDataLength);
			this.Controls.Add(this.txtCycle);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtRemote);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "FPing";
			this.Text = "PingTester 測試程式";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.TextBox txtTimeout;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtCycle;
		private System.Windows.Forms.TextBox txtRemote;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RichTextBox rtbConsole;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtDataLength;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox txtTimes;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.TextBox txtTTL;
		private System.Windows.Forms.Label label8;
	}
}