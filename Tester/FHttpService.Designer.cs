namespace Tester
{
	partial class FHttpService
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
			this.rtbLog = new System.Windows.Forms.RichTextBox();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.txtIP = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.txtSvcNames = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.btnStop = new System.Windows.Forms.Button();
			this.btnStart = new System.Windows.Forms.Button();
			this.txtPath = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.gbForm = new System.Windows.Forms.GroupBox();
			this.btnExtWebClientUpload = new System.Windows.Forms.Button();
			this.btnFile2 = new System.Windows.Forms.Button();
			this.txtFile2 = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.btnWebClientGet = new System.Windows.Forms.Button();
			this.btnWebClientPost = new System.Windows.Forms.Button();
			this.txtVal4 = new System.Windows.Forms.TextBox();
			this.pbPercentage = new System.Windows.Forms.ProgressBar();
			this.label13 = new System.Windows.Forms.Label();
			this.btnWebClientUpload = new System.Windows.Forms.Button();
			this.txtKey4 = new System.Windows.Forms.TextBox();
			this.btnFile1 = new System.Windows.Forms.Button();
			this.label14 = new System.Windows.Forms.Label();
			this.txtVal2 = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.txtFile1 = new System.Windows.Forms.TextBox();
			this.txtKey2 = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.txtKey1 = new System.Windows.Forms.TextBox();
			this.txtVal3 = new System.Windows.Forms.TextBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.txtKey3 = new System.Windows.Forms.TextBox();
			this.txtVal1 = new System.Windows.Forms.TextBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.gbForm.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.rtbLog);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.groupBox1.Location = new System.Drawing.Point(10, 278);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(838, 263);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Log Message";
			// 
			// rtbLog
			// 
			this.rtbLog.AutoWordSelection = true;
			this.rtbLog.BackColor = System.Drawing.SystemColors.Window;
			this.rtbLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.rtbLog.Font = new System.Drawing.Font("細明體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.rtbLog.Location = new System.Drawing.Point(3, 18);
			this.rtbLog.Name = "rtbLog";
			this.rtbLog.ReadOnly = true;
			this.rtbLog.Size = new System.Drawing.Size(832, 242);
			this.rtbLog.TabIndex = 0;
			this.rtbLog.Text = "";
			// 
			// txtPort
			// 
			this.txtPort.Location = new System.Drawing.Point(217, 21);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(50, 22);
			this.txtPort.TabIndex = 3;
			this.txtPort.Text = "8080";
			// 
			// txtIP
			// 
			this.txtIP.BackColor = System.Drawing.SystemColors.Window;
			this.txtIP.Location = new System.Drawing.Point(57, 21);
			this.txtIP.Name = "txtIP";
			this.txtIP.Size = new System.Drawing.Size(88, 22);
			this.txtIP.TabIndex = 1;
			this.txtIP.Text = "127.0.0.1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(158, 26);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(53, 12);
			this.label2.TabIndex = 2;
			this.label2.Text = "通訊埠號";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "IP位址";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtSvcNames);
			this.groupBox2.Controls.Add(this.label9);
			this.groupBox2.Controls.Add(this.btnStop);
			this.groupBox2.Controls.Add(this.btnStart);
			this.groupBox2.Controls.Add(this.txtIP);
			this.groupBox2.Controls.Add(this.txtPort);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox2.Location = new System.Drawing.Point(10, 10);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(838, 56);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Http Server";
			// 
			// txtSvcNames
			// 
			this.txtSvcNames.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSvcNames.Location = new System.Drawing.Point(342, 21);
			this.txtSvcNames.Name = "txtSvcNames";
			this.txtSvcNames.Size = new System.Drawing.Size(317, 22);
			this.txtSvcNames.TabIndex = 7;
			this.txtSvcNames.Text = "upload;remotelog";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(283, 26);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(53, 12);
			this.label9.TabIndex = 6;
			this.label9.Text = "允許服務";
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(746, 21);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(75, 23);
			this.btnStop.TabIndex = 5;
			this.btnStop.Text = "停止";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(665, 21);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(75, 23);
			this.btnStart.TabIndex = 4;
			this.btnStart.Text = "啟動";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// txtPath
			// 
			this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtPath.BackColor = System.Drawing.SystemColors.Window;
			this.txtPath.Location = new System.Drawing.Point(57, 21);
			this.txtPath.Name = "txtPath";
			this.txtPath.Size = new System.Drawing.Size(515, 22);
			this.txtPath.TabIndex = 1;
			this.txtPath.Text = "http://127.0.0.1:8080/upload";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(22, 26);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(29, 12);
			this.label4.TabIndex = 0;
			this.label4.Text = "網址";
			// 
			// gbForm
			// 
			this.gbForm.Controls.Add(this.btnExtWebClientUpload);
			this.gbForm.Controls.Add(this.btnFile2);
			this.gbForm.Controls.Add(this.txtFile2);
			this.gbForm.Controls.Add(this.label7);
			this.gbForm.Controls.Add(this.btnWebClientGet);
			this.gbForm.Controls.Add(this.btnWebClientPost);
			this.gbForm.Controls.Add(this.txtPath);
			this.gbForm.Controls.Add(this.label4);
			this.gbForm.Controls.Add(this.txtVal4);
			this.gbForm.Controls.Add(this.pbPercentage);
			this.gbForm.Controls.Add(this.label13);
			this.gbForm.Controls.Add(this.btnWebClientUpload);
			this.gbForm.Controls.Add(this.txtKey4);
			this.gbForm.Controls.Add(this.btnFile1);
			this.gbForm.Controls.Add(this.label14);
			this.gbForm.Controls.Add(this.txtVal2);
			this.gbForm.Controls.Add(this.label15);
			this.gbForm.Controls.Add(this.txtFile1);
			this.gbForm.Controls.Add(this.txtKey2);
			this.gbForm.Controls.Add(this.label3);
			this.gbForm.Controls.Add(this.label16);
			this.gbForm.Controls.Add(this.txtKey1);
			this.gbForm.Controls.Add(this.txtVal3);
			this.gbForm.Controls.Add(this.label8);
			this.gbForm.Controls.Add(this.label11);
			this.gbForm.Controls.Add(this.label10);
			this.gbForm.Controls.Add(this.txtKey3);
			this.gbForm.Controls.Add(this.txtVal1);
			this.gbForm.Controls.Add(this.label12);
			this.gbForm.Dock = System.Windows.Forms.DockStyle.Top;
			this.gbForm.Enabled = false;
			this.gbForm.Location = new System.Drawing.Point(10, 74);
			this.gbForm.Margin = new System.Windows.Forms.Padding(3, 10, 3, 3);
			this.gbForm.Name = "gbForm";
			this.gbForm.Size = new System.Drawing.Size(838, 196);
			this.gbForm.TabIndex = 2;
			this.gbForm.TabStop = false;
			this.gbForm.Text = "WebClient Post Data and File";
			// 
			// btnExtWebClientUpload
			// 
			this.btnExtWebClientUpload.Location = new System.Drawing.Point(579, 77);
			this.btnExtWebClientUpload.Name = "btnExtWebClientUpload";
			this.btnExtWebClientUpload.Size = new System.Drawing.Size(243, 23);
			this.btnExtWebClientUpload.TabIndex = 27;
			this.btnExtWebClientUpload.Text = "使用 ExtWebClient 上傳多個檔案與資料";
			this.btnExtWebClientUpload.UseVisualStyleBackColor = true;
			this.btnExtWebClientUpload.Click += new System.EventHandler(this.btnExtWebClientUpload_Click);
			// 
			// btnFile2
			// 
			this.btnFile2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFile2.Location = new System.Drawing.Point(547, 77);
			this.btnFile2.Name = "btnFile2";
			this.btnFile2.Size = new System.Drawing.Size(25, 23);
			this.btnFile2.TabIndex = 7;
			this.btnFile2.Text = "...";
			this.btnFile2.UseVisualStyleBackColor = true;
			this.btnFile2.Click += new System.EventHandler(this.btnFile_Click);
			// 
			// txtFile2
			// 
			this.txtFile2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile2.BackColor = System.Drawing.SystemColors.Window;
			this.txtFile2.Location = new System.Drawing.Point(57, 77);
			this.txtFile2.Name = "txtFile2";
			this.txtFile2.Size = new System.Drawing.Size(484, 22);
			this.txtFile2.TabIndex = 6;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(16, 82);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(35, 12);
			this.label7.TabIndex = 5;
			this.label7.Text = "檔案2";
			// 
			// btnWebClientGet
			// 
			this.btnWebClientGet.Location = new System.Drawing.Point(579, 133);
			this.btnWebClientGet.Name = "btnWebClientGet";
			this.btnWebClientGet.Size = new System.Drawing.Size(243, 23);
			this.btnWebClientGet.TabIndex = 25;
			this.btnWebClientGet.Text = "使用 WebClient Get 資料";
			this.btnWebClientGet.UseVisualStyleBackColor = true;
			this.btnWebClientGet.Click += new System.EventHandler(this.btnWebClientGet_Click);
			// 
			// btnWebClientPost
			// 
			this.btnWebClientPost.Location = new System.Drawing.Point(579, 106);
			this.btnWebClientPost.Name = "btnWebClientPost";
			this.btnWebClientPost.Size = new System.Drawing.Size(243, 23);
			this.btnWebClientPost.TabIndex = 24;
			this.btnWebClientPost.Text = "使用 WebClient POST 資料";
			this.btnWebClientPost.UseVisualStyleBackColor = true;
			this.btnWebClientPost.Click += new System.EventHandler(this.btnWebClientPost_Click);
			// 
			// txtVal4
			// 
			this.txtVal4.BackColor = System.Drawing.SystemColors.Window;
			this.txtVal4.Location = new System.Drawing.Point(484, 133);
			this.txtVal4.Name = "txtVal4";
			this.txtVal4.Size = new System.Drawing.Size(88, 22);
			this.txtVal4.TabIndex = 23;
			// 
			// pbPercentage
			// 
			this.pbPercentage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pbPercentage.Location = new System.Drawing.Point(14, 171);
			this.pbPercentage.Name = "pbPercentage";
			this.pbPercentage.Size = new System.Drawing.Size(808, 12);
			this.pbPercentage.TabIndex = 28;
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(443, 138);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(35, 12);
			this.label13.TabIndex = 22;
			this.label13.Text = "鍵值4";
			// 
			// btnWebClientUpload
			// 
			this.btnWebClientUpload.Location = new System.Drawing.Point(579, 49);
			this.btnWebClientUpload.Name = "btnWebClientUpload";
			this.btnWebClientUpload.Size = new System.Drawing.Size(243, 23);
			this.btnWebClientUpload.TabIndex = 26;
			this.btnWebClientUpload.Text = "使用 WebClient 上傳第一個檔案";
			this.btnWebClientUpload.UseVisualStyleBackColor = true;
			this.btnWebClientUpload.Click += new System.EventHandler(this.btnWebClientUpload_Click);
			// 
			// txtKey4
			// 
			this.txtKey4.BackColor = System.Drawing.SystemColors.Window;
			this.txtKey4.Location = new System.Drawing.Point(342, 133);
			this.txtKey4.Name = "txtKey4";
			this.txtKey4.Size = new System.Drawing.Size(88, 22);
			this.txtKey4.TabIndex = 21;
			// 
			// btnFile1
			// 
			this.btnFile1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFile1.Location = new System.Drawing.Point(547, 49);
			this.btnFile1.Name = "btnFile1";
			this.btnFile1.Size = new System.Drawing.Size(25, 23);
			this.btnFile1.TabIndex = 4;
			this.btnFile1.Text = "...";
			this.btnFile1.UseVisualStyleBackColor = true;
			this.btnFile1.Click += new System.EventHandler(this.btnFile_Click);
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(301, 138);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(35, 12);
			this.label14.TabIndex = 20;
			this.label14.Text = "鍵名4";
			// 
			// txtVal2
			// 
			this.txtVal2.BackColor = System.Drawing.SystemColors.Window;
			this.txtVal2.Location = new System.Drawing.Point(484, 105);
			this.txtVal2.Name = "txtVal2";
			this.txtVal2.Size = new System.Drawing.Size(88, 22);
			this.txtVal2.TabIndex = 15;
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(443, 110);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(35, 12);
			this.label15.TabIndex = 14;
			this.label15.Text = "鍵值2";
			// 
			// txtFile1
			// 
			this.txtFile1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile1.BackColor = System.Drawing.SystemColors.Window;
			this.txtFile1.Location = new System.Drawing.Point(57, 49);
			this.txtFile1.Name = "txtFile1";
			this.txtFile1.Size = new System.Drawing.Size(484, 22);
			this.txtFile1.TabIndex = 3;
			// 
			// txtKey2
			// 
			this.txtKey2.BackColor = System.Drawing.SystemColors.Window;
			this.txtKey2.Location = new System.Drawing.Point(342, 105);
			this.txtKey2.Name = "txtKey2";
			this.txtKey2.Size = new System.Drawing.Size(88, 22);
			this.txtKey2.TabIndex = 13;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(16, 54);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(35, 12);
			this.label3.TabIndex = 2;
			this.label3.Text = "檔案1";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(301, 110);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(35, 12);
			this.label16.TabIndex = 12;
			this.label16.Text = "鍵名2";
			// 
			// txtKey1
			// 
			this.txtKey1.BackColor = System.Drawing.SystemColors.Window;
			this.txtKey1.Location = new System.Drawing.Point(57, 105);
			this.txtKey1.Name = "txtKey1";
			this.txtKey1.Size = new System.Drawing.Size(88, 22);
			this.txtKey1.TabIndex = 9;
			// 
			// txtVal3
			// 
			this.txtVal3.BackColor = System.Drawing.SystemColors.Window;
			this.txtVal3.Location = new System.Drawing.Point(199, 133);
			this.txtVal3.Name = "txtVal3";
			this.txtVal3.Size = new System.Drawing.Size(88, 22);
			this.txtVal3.TabIndex = 19;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(16, 110);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(35, 12);
			this.label8.TabIndex = 8;
			this.label8.Text = "鍵名1";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(158, 138);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(35, 12);
			this.label11.TabIndex = 18;
			this.label11.Text = "鍵值3";
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(158, 110);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(35, 12);
			this.label10.TabIndex = 10;
			this.label10.Text = "鍵值1";
			// 
			// txtKey3
			// 
			this.txtKey3.BackColor = System.Drawing.SystemColors.Window;
			this.txtKey3.Location = new System.Drawing.Point(57, 133);
			this.txtKey3.Name = "txtKey3";
			this.txtKey3.Size = new System.Drawing.Size(88, 22);
			this.txtKey3.TabIndex = 17;
			// 
			// txtVal1
			// 
			this.txtVal1.BackColor = System.Drawing.SystemColors.Window;
			this.txtVal1.Location = new System.Drawing.Point(199, 105);
			this.txtVal1.Name = "txtVal1";
			this.txtVal1.Size = new System.Drawing.Size(88, 22);
			this.txtVal1.TabIndex = 11;
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(16, 138);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(35, 12);
			this.label12.TabIndex = 16;
			this.label12.Text = "鍵名3";
			// 
			// label5
			// 
			this.label5.Dock = System.Windows.Forms.DockStyle.Top;
			this.label5.Location = new System.Drawing.Point(10, 66);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(838, 8);
			this.label5.TabIndex = 1;
			// 
			// label6
			// 
			this.label6.Dock = System.Windows.Forms.DockStyle.Top;
			this.label6.Location = new System.Drawing.Point(10, 270);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(838, 8);
			this.label6.TabIndex = 3;
			// 
			// FHttpService
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(858, 551);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.gbForm);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.groupBox2);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FHttpService";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.Text = "FHttpService";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FHttpService_FormClosing);
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.gbForm.ResumeLayout(false);
			this.gbForm.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.TextBox txtIP;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.GroupBox gbForm;
		private System.Windows.Forms.Button btnWebClientUpload;
		private System.Windows.Forms.Button btnFile1;
		private System.Windows.Forms.TextBox txtPath;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox txtFile1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ProgressBar pbPercentage;
		private System.Windows.Forms.Button btnWebClientPost;
		private System.Windows.Forms.TextBox txtKey1;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.TextBox txtVal1;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox txtVal4;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.TextBox txtKey4;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TextBox txtVal2;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox txtKey2;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox txtVal3;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox txtKey3;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Button btnFile2;
		private System.Windows.Forms.TextBox txtFile2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.Button btnWebClientGet;
		private System.Windows.Forms.RichTextBox rtbLog;
		private System.Windows.Forms.Button btnExtWebClientUpload;
		private System.Windows.Forms.TextBox txtSvcNames;
		private System.Windows.Forms.Label label9;
	}
}