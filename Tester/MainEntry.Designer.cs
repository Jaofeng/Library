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
			this.label1 = new System.Windows.Forms.Label();
			this.btnUnitConv = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.txtGetBytes = new System.Windows.Forms.TextBox();
			this.txtGetBytesResult = new System.Windows.Forms.TextBox();
			this.btnGetBytes = new System.Windows.Forms.Button();
			this.chkBigEndian = new System.Windows.Forms.CheckBox();
			this.cbDateType = new System.Windows.Forms.ComboBox();
			this.button7 = new System.Windows.Forms.Button();
			this.listBox1 = new System.Windows.Forms.ListBox();
			this.button8 = new System.Windows.Forms.Button();
			this.button9 = new System.Windows.Forms.Button();
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
			this.button3.Location = new System.Drawing.Point(135, 21);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(107, 50);
			this.button3.TabIndex = 2;
			this.button3.Text = "AsyncUdpServer";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(135, 77);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(107, 50);
			this.button4.TabIndex = 3;
			this.button4.Text = "Multicast Test";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// button5
			// 
			this.button5.Location = new System.Drawing.Point(248, 21);
			this.button5.Name = "button5";
			this.button5.Size = new System.Drawing.Size(107, 50);
			this.button5.TabIndex = 4;
			this.button5.Text = "Ping Test";
			this.button5.UseVisualStyleBackColor = true;
			this.button5.Click += new System.EventHandler(this.button5_Click);
			// 
			// button6
			// 
			this.button6.Location = new System.Drawing.Point(248, 77);
			this.button6.Name = "button6";
			this.button6.Size = new System.Drawing.Size(107, 50);
			this.button6.TabIndex = 5;
			this.button6.Text = "ThreadSafeList Test";
			this.button6.UseVisualStyleBackColor = true;
			this.button6.Click += new System.EventHandler(this.button6_Click);
			// 
			// txtConvUnit
			// 
			this.txtConvUnit.Location = new System.Drawing.Point(135, 147);
			this.txtConvUnit.Name = "txtConvUnit";
			this.txtConvUnit.Size = new System.Drawing.Size(107, 22);
			this.txtConvUnit.TabIndex = 7;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(35, 146);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(80, 24);
			this.label1.TabIndex = 6;
			this.label1.Text = "單位轉換\r\n(UnitConverter)";
			// 
			// btnUnitConv
			// 
			this.btnUnitConv.Location = new System.Drawing.Point(248, 147);
			this.btnUnitConv.Name = "btnUnitConv";
			this.btnUnitConv.Size = new System.Drawing.Size(75, 23);
			this.btnUnitConv.TabIndex = 8;
			this.btnUnitConv.Text = "Convert";
			this.btnUnitConv.UseVisualStyleBackColor = true;
			this.btnUnitConv.Click += new System.EventHandler(this.btnUnitConv_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(35, 193);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(43, 12);
			this.label2.TabIndex = 9;
			this.label2.Text = "GetByte";
			// 
			// txtGetBytes
			// 
			this.txtGetBytes.Location = new System.Drawing.Point(135, 190);
			this.txtGetBytes.Name = "txtGetBytes";
			this.txtGetBytes.Size = new System.Drawing.Size(107, 22);
			this.txtGetBytes.TabIndex = 10;
			this.txtGetBytes.Text = "1";
			// 
			// txtGetBytesResult
			// 
			this.txtGetBytesResult.Location = new System.Drawing.Point(37, 303);
			this.txtGetBytesResult.Name = "txtGetBytesResult";
			this.txtGetBytesResult.Size = new System.Drawing.Size(286, 22);
			this.txtGetBytesResult.TabIndex = 14;
			// 
			// btnGetBytes
			// 
			this.btnGetBytes.Location = new System.Drawing.Point(248, 190);
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
			this.chkBigEndian.Location = new System.Drawing.Point(135, 278);
			this.chkBigEndian.Name = "chkBigEndian";
			this.chkBigEndian.Size = new System.Drawing.Size(77, 16);
			this.chkBigEndian.TabIndex = 13;
			this.chkBigEndian.Text = "Big Endian";
			this.chkBigEndian.UseVisualStyleBackColor = true;
			// 
			// cbDateType
			// 
			this.cbDateType.FormattingEnabled = true;
			this.cbDateType.Items.AddRange(new object[] {
            "Int",
            "Short",
            "Long",
            "Double",
            "Float"});
			this.cbDateType.Location = new System.Drawing.Point(37, 276);
			this.cbDateType.Name = "cbDateType";
			this.cbDateType.Size = new System.Drawing.Size(92, 20);
			this.cbDateType.TabIndex = 12;
			this.cbDateType.Text = "Int";
			// 
			// button7
			// 
			this.button7.Location = new System.Drawing.Point(361, 133);
			this.button7.Name = "button7";
			this.button7.Size = new System.Drawing.Size(107, 50);
			this.button7.TabIndex = 15;
			this.button7.Text = "ANSI String";
			this.button7.UseVisualStyleBackColor = true;
			this.button7.Click += new System.EventHandler(this.button7_Click);
			// 
			// listBox1
			// 
			this.listBox1.FormattingEnabled = true;
			this.listBox1.ItemHeight = 12;
			this.listBox1.Location = new System.Drawing.Point(361, 189);
			this.listBox1.Name = "listBox1";
			this.listBox1.Size = new System.Drawing.Size(107, 136);
			this.listBox1.TabIndex = 16;
			// 
			// button8
			// 
			this.button8.Location = new System.Drawing.Point(361, 21);
			this.button8.Name = "button8";
			this.button8.Size = new System.Drawing.Size(107, 50);
			this.button8.TabIndex = 4;
			this.button8.Text = "Active Ports";
			this.button8.UseVisualStyleBackColor = true;
			this.button8.Click += new System.EventHandler(this.button8_Click);
			// 
			// button9
			// 
			this.button9.Location = new System.Drawing.Point(361, 77);
			this.button9.Name = "button9";
			this.button9.Size = new System.Drawing.Size(107, 50);
			this.button9.TabIndex = 4;
			this.button9.Text = "Telnet Server";
			this.button9.UseVisualStyleBackColor = true;
			this.button9.Click += new System.EventHandler(this.button9_Click);
			// 
			// MainEntry
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(491, 353);
			this.Controls.Add(this.listBox1);
			this.Controls.Add(this.button7);
			this.Controls.Add(this.cbDateType);
			this.Controls.Add(this.chkBigEndian);
			this.Controls.Add(this.btnGetBytes);
			this.Controls.Add(this.txtGetBytesResult);
			this.Controls.Add(this.txtGetBytes);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnUnitConv);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtConvUnit);
			this.Controls.Add(this.button6);
			this.Controls.Add(this.button9);
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
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Button button5;
		private System.Windows.Forms.Button button6;
		private System.Windows.Forms.TextBox txtConvUnit;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnUnitConv;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtGetBytes;
		private System.Windows.Forms.TextBox txtGetBytesResult;
		private System.Windows.Forms.Button btnGetBytes;
		private System.Windows.Forms.CheckBox chkBigEndian;
		private System.Windows.Forms.ComboBox cbDateType;
		private System.Windows.Forms.Button button7;
		private System.Windows.Forms.ListBox listBox1;
		private System.Windows.Forms.Button button8;
		private System.Windows.Forms.Button button9;
	}
}