namespace Tester
{
	partial class MainEntry
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
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.button6 = new System.Windows.Forms.Button();
			this.txtConvUnit = new System.Windows.Forms.TextBox();
			this.btnUnitConv = new System.Windows.Forms.Button();
			this.txtGetBytes = new System.Windows.Forms.TextBox();
			this.txtGetBytesResult = new System.Windows.Forms.TextBox();
			this.btnGetBytes = new System.Windows.Forms.Button();
			this.chkBigEndian = new System.Windows.Forms.CheckBox();
			this.cbDateType = new System.Windows.Forms.ComboBox();
			this.button7 = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button8 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.labConvResult = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.labCrc16TimeT = new System.Windows.Forms.Label();
			this.labCrc16ResT = new System.Windows.Forms.Label();
			this.labCrc16TimeC = new System.Windows.Forms.Label();
			this.labCrc16ResC = new System.Windows.Forms.Label();
			this.labCrc16TimeN = new System.Windows.Forms.Label();
			this.labCrc16ByteT = new System.Windows.Forms.Label();
			this.labCrc16ByteC = new System.Windows.Forms.Label();
			this.labCrc16ByteN = new System.Windows.Forms.Label();
			this.labCrc16ResN = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtCrcSource = new System.Windows.Forms.TextBox();
			this.btnCRC16 = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.cbLogLevel = new System.Windows.Forms.ComboBox();
			this.btnSaveLog = new System.Windows.Forms.Button();
			this.button11 = new System.Windows.Forms.Button();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.rbFindIndexes = new System.Windows.Forms.RadioButton();
			this.rbFindIndex = new System.Windows.Forms.RadioButton();
			this.rbIndexOfBytes = new System.Windows.Forms.RadioButton();
			this.btnFile = new System.Windows.Forms.Button();
			this.txtHexStr = new System.Windows.Forms.TextBox();
			this.btnSearchHex = new System.Windows.Forms.Button();
			this.txtFile = new System.Windows.Forms.TextBox();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.txtTarget = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.txtSource = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.txtIV = new System.Windows.Forms.TextBox();
			this.txtKey = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.btnDecrypt = new System.Windows.Forms.Button();
			this.btnEncrypt = new System.Windows.Forms.Button();
			this.button10 = new System.Windows.Forms.Button();
			this.button12 = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(22, 21);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(107, 50);
			this.button1.TabIndex = 0;
			this.button1.Text = "AsyncServer";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(22, 77);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(107, 50);
			this.button2.TabIndex = 1;
			this.button2.Text = "AsyncClient";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(22, 133);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(107, 50);
			this.button3.TabIndex = 2;
			this.button3.Text = "AsyncUdpServer";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(22, 189);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(107, 50);
			this.button4.TabIndex = 3;
			this.button4.Text = "Multicast Test";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(22, 245);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(107, 50);
			this.button5.TabIndex = 4;
			this.button5.Text = "Ping Test";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(135, 133);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(107, 50);
			this.button6.TabIndex = 5;
			this.button6.Text = "ThreadSafeList Test";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// txtConvUnit
			// 
			this.txtConvUnit.Location = new System.Drawing.Point(9, 21);
			this.txtConvUnit.Name = "txtConvUnit";
			this.txtConvUnit.Size = new System.Drawing.Size(92, 22);
			this.txtConvUnit.TabIndex = 7;
			this.txtConvUnit.Text = "1000";
			// 
			// btnUnitConv
			// 
			this.btnUnitConv.Location = new System.Drawing.Point(107, 21);
			this.btnUnitConv.Name = "btnUnitConv";
			this.btnUnitConv.Size = new System.Drawing.Size(75, 22);
			this.btnUnitConv.TabIndex = 8;
			this.btnUnitConv.Text = "Convert";
			this.btnUnitConv.UseVisualStyleBackColor = true;
			this.btnUnitConv.Click += new System.EventHandler(this.btnUnitConv_Click);
			// 
			// txtGetBytes
			// 
			this.txtGetBytes.Location = new System.Drawing.Point(9, 21);
			this.txtGetBytes.Name = "txtGetBytes";
			this.txtGetBytes.Size = new System.Drawing.Size(92, 22);
			this.txtGetBytes.TabIndex = 10;
			this.txtGetBytes.Text = "1";
			// 
			// txtGetBytesResult
			// 
			this.txtGetBytesResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtGetBytesResult.Location = new System.Drawing.Point(9, 49);
			this.txtGetBytesResult.Name = "txtGetBytesResult";
			this.txtGetBytesResult.Size = new System.Drawing.Size(240, 22);
			this.txtGetBytesResult.TabIndex = 14;
			// 
			// btnGetBytes
			// 
			this.btnGetBytes.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnGetBytes.Location = new System.Drawing.Point(255, 49);
			this.btnGetBytes.Name = "btnGetBytes";
			this.btnGetBytes.Size = new System.Drawing.Size(75, 23);
			this.btnGetBytes.TabIndex = 11;
			this.btnGetBytes.Text = "Convert";
			this.btnGetBytes.UseVisualStyleBackColor = true;
			this.btnGetBytes.Click += new System.EventHandler(this.btnGetBytes_Click);
			// 
			// chkBigEndian
			// 
			this.chkBigEndian.AutoSize = true;
			this.chkBigEndian.Checked = true;
			this.chkBigEndian.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkBigEndian.Location = new System.Drawing.Point(188, 21);
			this.chkBigEndian.Name = "chkBigEndian";
			this.chkBigEndian.Size = new System.Drawing.Size(77, 16);
			this.chkBigEndian.TabIndex = 13;
			this.chkBigEndian.Text = "Big Endian";
			this.chkBigEndian.UseVisualStyleBackColor = true;
			// 
			// cbDateType
			// 
			this.cbDateType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbDateType.FormattingEnabled = true;
			this.cbDateType.Items.AddRange(new object[] {
            "Int",
            "Short",
            "Long",
            "Double",
            "Float"});
			this.cbDateType.Location = new System.Drawing.Point(107, 21);
			this.cbDateType.Name = "cbDateType";
			this.cbDateType.Size = new System.Drawing.Size(75, 20);
			this.cbDateType.TabIndex = 12;
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(135, 245);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(107, 31);
			this.button7.TabIndex = 15;
			this.button7.Text = "ANSI String";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 12;
			this.listBox1.Location = new System.Drawing.Point(135, 274);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(107, 136);
			this.listBox1.TabIndex = 16;
			// 
			// button8
			// 
			this.button8.Location = new System.Drawing.Point(22, 301);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(107, 50);
			this.button8.TabIndex = 4;
			this.button8.Text = "Active Ports";
			this.button8.UseVisualStyleBackColor = true;
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button9
			// 
			this.button9.Location = new System.Drawing.Point(135, 21);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(107, 50);
			this.button9.TabIndex = 4;
			this.button9.Text = "Telnet Server";
			this.button9.UseVisualStyleBackColor = true;
			this.button9.Click += new System.EventHandler(this.button9_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.labConvResult);
			this.groupBox1.Controls.Add(this.txtConvUnit);
			this.groupBox1.Controls.Add(this.btnUnitConv);
			this.groupBox1.Location = new System.Drawing.Point(261, 21);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(336, 50);
			this.groupBox1.TabIndex = 17;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "計量單位轉換";
			// 
			// labConvResult
			// 
			this.labConvResult.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.labConvResult.BackColor = System.Drawing.SystemColors.Window;
			this.labConvResult.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labConvResult.Location = new System.Drawing.Point(188, 21);
			this.labConvResult.Name = "labConvResult";
			this.labConvResult.Size = new System.Drawing.Size(142, 22);
			this.labConvResult.TabIndex = 9;
			this.labConvResult.Text = "0";
			this.labConvResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.txtGetBytes);
			this.groupBox2.Controls.Add(this.btnGetBytes);
			this.groupBox2.Controls.Add(this.cbDateType);
			this.groupBox2.Controls.Add(this.chkBigEndian);
			this.groupBox2.Controls.Add(this.txtGetBytesResult);
			this.groupBox2.Location = new System.Drawing.Point(261, 77);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(336, 81);
			this.groupBox2.TabIndex = 18;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Number GetByte Array";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.labCrc16TimeT);
			this.groupBox3.Controls.Add(this.labCrc16ResT);
			this.groupBox3.Controls.Add(this.labCrc16TimeC);
			this.groupBox3.Controls.Add(this.labCrc16ResC);
			this.groupBox3.Controls.Add(this.labCrc16TimeN);
			this.groupBox3.Controls.Add(this.labCrc16ByteT);
			this.groupBox3.Controls.Add(this.labCrc16ByteC);
			this.groupBox3.Controls.Add(this.labCrc16ByteN);
			this.groupBox3.Controls.Add(this.labCrc16ResN);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.label2);
			this.groupBox3.Controls.Add(this.label1);
			this.groupBox3.Controls.Add(this.txtCrcSource);
			this.groupBox3.Controls.Add(this.btnCRC16);
			this.groupBox3.Location = new System.Drawing.Point(261, 166);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(336, 129);
			this.groupBox3.TabIndex = 19;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "CRC16 calculate";
			// 
			// labCrc16TimeT
			// 
			this.labCrc16TimeT.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16TimeT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16TimeT.Location = new System.Drawing.Point(252, 98);
			this.labCrc16TimeT.Name = "labCrc16TimeT";
			this.labCrc16TimeT.Size = new System.Drawing.Size(75, 22);
			this.labCrc16TimeT.TabIndex = 21;
			this.labCrc16TimeT.Text = "0 ms";
			this.labCrc16TimeT.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labCrc16ResT
			// 
			this.labCrc16ResT.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16ResT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16ResT.Location = new System.Drawing.Point(67, 98);
			this.labCrc16ResT.Name = "labCrc16ResT";
			this.labCrc16ResT.Size = new System.Drawing.Size(90, 22);
			this.labCrc16ResT.TabIndex = 20;
			this.labCrc16ResT.Text = "0";
			this.labCrc16ResT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labCrc16TimeC
			// 
			this.labCrc16TimeC.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16TimeC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16TimeC.Location = new System.Drawing.Point(252, 72);
			this.labCrc16TimeC.Name = "labCrc16TimeC";
			this.labCrc16TimeC.Size = new System.Drawing.Size(75, 22);
			this.labCrc16TimeC.TabIndex = 19;
			this.labCrc16TimeC.Text = "0 ms";
			this.labCrc16TimeC.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labCrc16ResC
			// 
			this.labCrc16ResC.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16ResC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16ResC.Location = new System.Drawing.Point(67, 72);
			this.labCrc16ResC.Name = "labCrc16ResC";
			this.labCrc16ResC.Size = new System.Drawing.Size(90, 22);
			this.labCrc16ResC.TabIndex = 18;
			this.labCrc16ResC.Text = "0";
			this.labCrc16ResC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labCrc16TimeN
			// 
			this.labCrc16TimeN.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16TimeN.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16TimeN.Location = new System.Drawing.Point(252, 46);
			this.labCrc16TimeN.Name = "labCrc16TimeN";
			this.labCrc16TimeN.Size = new System.Drawing.Size(75, 22);
			this.labCrc16TimeN.TabIndex = 17;
			this.labCrc16TimeN.Text = "0 ms";
			this.labCrc16TimeN.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// labCrc16ByteT
			// 
			this.labCrc16ByteT.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16ByteT.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16ByteT.Location = new System.Drawing.Point(159, 98);
			this.labCrc16ByteT.Name = "labCrc16ByteT";
			this.labCrc16ByteT.Size = new System.Drawing.Size(90, 22);
			this.labCrc16ByteT.TabIndex = 17;
			this.labCrc16ByteT.Text = "0";
			this.labCrc16ByteT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labCrc16ByteC
			// 
			this.labCrc16ByteC.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16ByteC.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16ByteC.Location = new System.Drawing.Point(159, 72);
			this.labCrc16ByteC.Name = "labCrc16ByteC";
			this.labCrc16ByteC.Size = new System.Drawing.Size(90, 22);
			this.labCrc16ByteC.TabIndex = 17;
			this.labCrc16ByteC.Text = "0";
			this.labCrc16ByteC.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labCrc16ByteN
			// 
			this.labCrc16ByteN.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16ByteN.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16ByteN.Location = new System.Drawing.Point(159, 46);
			this.labCrc16ByteN.Name = "labCrc16ByteN";
			this.labCrc16ByteN.Size = new System.Drawing.Size(90, 22);
			this.labCrc16ByteN.TabIndex = 17;
			this.labCrc16ByteN.Text = "0";
			this.labCrc16ByteN.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// labCrc16ResN
			// 
			this.labCrc16ResN.BackColor = System.Drawing.SystemColors.Window;
			this.labCrc16ResN.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.labCrc16ResN.Location = new System.Drawing.Point(67, 46);
			this.labCrc16ResN.Name = "labCrc16ResN";
			this.labCrc16ResN.Size = new System.Drawing.Size(90, 22);
			this.labCrc16ResN.TabIndex = 17;
			this.labCrc16ResN.Text = "0";
			this.labCrc16ResN.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(7, 103);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(31, 12);
			this.label3.TabIndex = 16;
			this.label3.Text = "Table";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(7, 77);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(39, 12);
			this.label2.TabIndex = 16;
			this.label2.Text = "CCITT";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 51);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(40, 12);
			this.label1.TabIndex = 16;
			this.label1.Text = "Normal";
			// 
			// txtCrcSource
			// 
			this.txtCrcSource.Location = new System.Drawing.Point(9, 21);
			this.txtCrcSource.Name = "txtCrcSource";
			this.txtCrcSource.Size = new System.Drawing.Size(240, 22);
			this.txtCrcSource.TabIndex = 15;
			// 
			// btnCRC16
			// 
			this.btnCRC16.Location = new System.Drawing.Point(252, 21);
			this.btnCRC16.Name = "btnCRC16";
			this.btnCRC16.Size = new System.Drawing.Size(75, 23);
			this.btnCRC16.TabIndex = 11;
			this.btnCRC16.Text = "Convert";
			this.btnCRC16.UseVisualStyleBackColor = true;
			this.btnCRC16.Click += new System.EventHandler(this.btnCRC16_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.txtLog);
			this.groupBox4.Controls.Add(this.cbLogLevel);
			this.groupBox4.Controls.Add(this.btnSaveLog);
			this.groupBox4.Location = new System.Drawing.Point(261, 301);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(336, 83);
			this.groupBox4.TabIndex = 21;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Log4Net Test";
			// 
			// txtLog
			// 
			this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtLog.Location = new System.Drawing.Point(8, 21);
			this.txtLog.Name = "txtLog";
			this.txtLog.Size = new System.Drawing.Size(322, 22);
			this.txtLog.TabIndex = 1;
			// 
			// cbLogLevel
			// 
			this.cbLogLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbLogLevel.FormattingEnabled = true;
			this.cbLogLevel.Items.AddRange(new object[] {
            "DEBUG",
            "INFO",
            "WARN",
            "ERROR",
            "FATAL"});
			this.cbLogLevel.Location = new System.Drawing.Point(9, 49);
			this.cbLogLevel.Name = "cbLogLevel";
			this.cbLogLevel.Size = new System.Drawing.Size(92, 20);
			this.cbLogLevel.TabIndex = 0;
			// 
			// btnSaveLog
			// 
			this.btnSaveLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSaveLog.Location = new System.Drawing.Point(255, 49);
			this.btnSaveLog.Name = "btnSaveLog";
			this.btnSaveLog.Size = new System.Drawing.Size(75, 23);
			this.btnSaveLog.TabIndex = 11;
			this.btnSaveLog.Text = "Save";
			this.btnSaveLog.UseVisualStyleBackColor = true;
			this.btnSaveLog.Click += new System.EventHandler(this.btnSaveLog_Click);
			// 
			// button11
			// 
			this.button11.Location = new System.Drawing.Point(135, 77);
			this.button11.Name = "button11";
			this.button11.Size = new System.Drawing.Size(107, 50);
			this.button11.TabIndex = 5;
			this.button11.Text = "HttpService Test";
			this.button11.UseVisualStyleBackColor = true;
			this.button11.Click += new System.EventHandler(this.button11_Click);
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.rbFindIndexes);
			this.groupBox5.Controls.Add(this.rbFindIndex);
			this.groupBox5.Controls.Add(this.rbIndexOfBytes);
			this.groupBox5.Controls.Add(this.btnFile);
			this.groupBox5.Controls.Add(this.txtHexStr);
			this.groupBox5.Controls.Add(this.btnSearchHex);
			this.groupBox5.Controls.Add(this.txtFile);
			this.groupBox5.Location = new System.Drawing.Point(603, 21);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(364, 106);
			this.groupBox5.TabIndex = 22;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Byte Array Search";
			// 
			// rbFindIndexes
			// 
			this.rbFindIndexes.AutoSize = true;
			this.rbFindIndexes.Location = new System.Drawing.Point(224, 22);
			this.rbFindIndexes.Name = "rbFindIndexes";
			this.rbFindIndexes.Size = new System.Drawing.Size(124, 16);
			this.rbFindIndexes.TabIndex = 13;
			this.rbFindIndexes.TabStop = true;
			this.rbFindIndexes.Text = "IndexesOfBytesInFile";
			this.rbFindIndexes.UseVisualStyleBackColor = true;
			this.rbFindIndexes.CheckedChanged += new System.EventHandler(this.rbFunc_CheckedChanged);
			// 
			// rbFindIndex
			// 
			this.rbFindIndex.AutoSize = true;
			this.rbFindIndex.Location = new System.Drawing.Point(103, 22);
			this.rbFindIndex.Name = "rbFindIndex";
			this.rbFindIndex.Size = new System.Drawing.Size(115, 16);
			this.rbFindIndex.TabIndex = 13;
			this.rbFindIndex.TabStop = true;
			this.rbFindIndex.Text = "IndexOfBytesInFile";
			this.rbFindIndex.UseVisualStyleBackColor = true;
			this.rbFindIndex.CheckedChanged += new System.EventHandler(this.rbFunc_CheckedChanged);
			// 
			// rbIndexOfBytes
			// 
			this.rbIndexOfBytes.AutoSize = true;
			this.rbIndexOfBytes.Location = new System.Drawing.Point(9, 22);
			this.rbIndexOfBytes.Name = "rbIndexOfBytes";
			this.rbIndexOfBytes.Size = new System.Drawing.Size(88, 16);
			this.rbIndexOfBytes.TabIndex = 13;
			this.rbIndexOfBytes.TabStop = true;
			this.rbIndexOfBytes.Text = "IndexOfBytes";
			this.rbIndexOfBytes.UseVisualStyleBackColor = true;
			this.rbIndexOfBytes.CheckedChanged += new System.EventHandler(this.rbFunc_CheckedChanged);
			// 
			// btnFile
			// 
			this.btnFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFile.Location = new System.Drawing.Point(333, 44);
			this.btnFile.Name = "btnFile";
			this.btnFile.Size = new System.Drawing.Size(25, 23);
			this.btnFile.TabIndex = 12;
			this.btnFile.Text = "...";
			this.btnFile.UseVisualStyleBackColor = true;
			this.btnFile.Click += new System.EventHandler(this.btnFile_Click);
			// 
			// txtHexStr
			// 
			this.txtHexStr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtHexStr.Location = new System.Drawing.Point(9, 72);
			this.txtHexStr.Name = "txtHexStr";
			this.txtHexStr.Size = new System.Drawing.Size(268, 22);
			this.txtHexStr.TabIndex = 2;
			this.txtHexStr.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			// 
			// btnSearchHex
			// 
			this.btnSearchHex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearchHex.Location = new System.Drawing.Point(283, 73);
			this.btnSearchHex.Name = "btnSearchHex";
			this.btnSearchHex.Size = new System.Drawing.Size(75, 23);
			this.btnSearchHex.TabIndex = 11;
			this.btnSearchHex.Text = "Search Next";
			this.btnSearchHex.UseVisualStyleBackColor = true;
			this.btnSearchHex.Click += new System.EventHandler(this.btnSearchHex_Click);
			// 
			// txtFile
			// 
			this.txtFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtFile.Location = new System.Drawing.Point(9, 44);
			this.txtFile.Name = "txtFile";
			this.txtFile.Size = new System.Drawing.Size(318, 22);
			this.txtFile.TabIndex = 2;
			this.txtFile.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.txtTarget);
			this.groupBox6.Controls.Add(this.label7);
			this.groupBox6.Controls.Add(this.txtSource);
			this.groupBox6.Controls.Add(this.label6);
			this.groupBox6.Controls.Add(this.txtIV);
			this.groupBox6.Controls.Add(this.txtKey);
			this.groupBox6.Controls.Add(this.label5);
			this.groupBox6.Controls.Add(this.label4);
			this.groupBox6.Controls.Add(this.btnDecrypt);
			this.groupBox6.Controls.Add(this.btnEncrypt);
			this.groupBox6.Location = new System.Drawing.Point(603, 133);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(364, 107);
			this.groupBox6.TabIndex = 23;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Security";
			// 
			// txtTarget
			// 
			this.txtTarget.Location = new System.Drawing.Point(54, 78);
			this.txtTarget.Name = "txtTarget";
			this.txtTarget.Size = new System.Drawing.Size(304, 22);
			this.txtTarget.TabIndex = 7;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(11, 82);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(35, 12);
			this.label7.TabIndex = 6;
			this.label7.Text = "Target";
			// 
			// txtSource
			// 
			this.txtSource.Location = new System.Drawing.Point(54, 50);
			this.txtSource.Name = "txtSource";
			this.txtSource.Size = new System.Drawing.Size(304, 22);
			this.txtSource.TabIndex = 5;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(11, 54);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(37, 12);
			this.label6.TabIndex = 4;
			this.label6.Text = "Source";
			// 
			// txtIV
			// 
			this.txtIV.Location = new System.Drawing.Point(167, 21);
			this.txtIV.MaxLength = 8;
			this.txtIV.Name = "txtIV";
			this.txtIV.Size = new System.Drawing.Size(75, 22);
			this.txtIV.TabIndex = 3;
			this.txtIV.Text = "12345678";
			// 
			// txtKey
			// 
			this.txtKey.Location = new System.Drawing.Point(54, 21);
			this.txtKey.MaxLength = 8;
			this.txtKey.Name = "txtKey";
			this.txtKey.Size = new System.Drawing.Size(75, 22);
			this.txtKey.TabIndex = 1;
			this.txtKey.Text = "12345678";
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(144, 26);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(17, 12);
			this.label5.TabIndex = 2;
			this.label5.Text = "IV";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(23, 26);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(24, 12);
			this.label4.TabIndex = 0;
			this.label4.Text = "Key";
			// 
			// btnDecrypt
			// 
			this.btnDecrypt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDecrypt.Location = new System.Drawing.Point(306, 21);
			this.btnDecrypt.Name = "btnDecrypt";
			this.btnDecrypt.Size = new System.Drawing.Size(52, 23);
			this.btnDecrypt.TabIndex = 9;
			this.btnDecrypt.Text = "Decrypt";
			this.btnDecrypt.UseVisualStyleBackColor = true;
			this.btnDecrypt.Click += new System.EventHandler(this.btnDecrypt_Click);
			// 
			// btnEncrypt
			// 
			this.btnEncrypt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnEncrypt.Location = new System.Drawing.Point(248, 21);
			this.btnEncrypt.Name = "btnEncrypt";
			this.btnEncrypt.Size = new System.Drawing.Size(52, 23);
			this.btnEncrypt.TabIndex = 8;
			this.btnEncrypt.Text = "Encrypt";
			this.btnEncrypt.UseVisualStyleBackColor = true;
			this.btnEncrypt.Click += new System.EventHandler(this.btnEncrypt_Click);
			// 
			// button10
			// 
			this.button10.Enabled = false;
			this.button10.Location = new System.Drawing.Point(22, 357);
			this.button10.Name = "button10";
			this.button10.Size = new System.Drawing.Size(107, 50);
			this.button10.TabIndex = 4;
			this.button10.Text = "ModbusTCP Master\r\n(Unfinish)";
			this.button10.UseVisualStyleBackColor = true;
			this.button10.Click += new System.EventHandler(this.button10_Click);
			// 
			// button12
			// 
			this.button12.Location = new System.Drawing.Point(135, 189);
			this.button12.Name = "button12";
			this.button12.Size = new System.Drawing.Size(107, 50);
			this.button12.TabIndex = 24;
			this.button12.Text = "WebSocket Server";
			this.button12.UseVisualStyleBackColor = true;
			this.button12.Click += new System.EventHandler(this.button12_Click);
			// 
			// MainEntry
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(981, 421);
			this.Controls.Add(this.button12);
			this.Controls.Add(this.groupBox6);
			this.Controls.Add(this.groupBox5);
			this.Controls.Add(this.groupBox4);
			this.Controls.Add(this.groupBox3);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.button7);
			this.Controls.Add(this.button11);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button9);
			this.Controls.Add(this.button10);
			this.Controls.Add(this.button8);
			this.Controls.Add(this.button5);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MainEntry";
			this.Text = "Main Form";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.TextBox txtConvUnit;
		private System.Windows.Forms.Button btnUnitConv;
		private System.Windows.Forms.TextBox txtGetBytes;
		private System.Windows.Forms.TextBox txtGetBytesResult;
		private System.Windows.Forms.Button btnGetBytes;
		private System.Windows.Forms.CheckBox chkBigEndian;
		private System.Windows.Forms.ComboBox cbDateType;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button9;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label labConvResult;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.TextBox txtCrcSource;
		private System.Windows.Forms.Label labCrc16TimeT;
		private System.Windows.Forms.Label labCrc16ResT;
		private System.Windows.Forms.Label labCrc16TimeC;
		private System.Windows.Forms.Label labCrc16ResC;
		private System.Windows.Forms.Label labCrc16TimeN;
		private System.Windows.Forms.Label labCrc16ResN;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnCRC16;
		private System.Windows.Forms.Label labCrc16ByteT;
		private System.Windows.Forms.Label labCrc16ByteC;
		private System.Windows.Forms.Label labCrc16ByteN;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.ComboBox cbLogLevel;
		private System.Windows.Forms.Button btnSaveLog;
		private System.Windows.Forms.Button button11;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Button btnFile;
		private System.Windows.Forms.TextBox txtHexStr;
		private System.Windows.Forms.Button btnSearchHex;
		private System.Windows.Forms.TextBox txtFile;
		private System.Windows.Forms.RadioButton rbFindIndex;
		private System.Windows.Forms.RadioButton rbIndexOfBytes;
		private System.Windows.Forms.RadioButton rbFindIndexes;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.TextBox txtIV;
		private System.Windows.Forms.TextBox txtKey;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button btnEncrypt;
		private System.Windows.Forms.Button btnDecrypt;
		private System.Windows.Forms.TextBox txtTarget;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txtSource;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button button10;
		private System.Windows.Forms.Button button12;
	}
}