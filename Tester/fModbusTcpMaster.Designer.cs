namespace Tester
{
	partial class fModbusTcpMaster
	{
		/// <summary>
		/// 設計工具所需的變數。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 清除任何使用中的資源。
		/// </summary>
		/// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 設計工具產生的程式碼

		/// <summary>
		/// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
		/// 修改這個方法的內容。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.btnStart = new System.Windows.Forms.Button();
			this.gbConnect = new System.Windows.Forms.GroupBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtRemoteIP = new System.Windows.Forms.TextBox();
			this.txtPort = new System.Windows.Forms.TextBox();
			this.cbPause = new System.Windows.Forms.CheckBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.btnRequest = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.ndCycle = new System.Windows.Forms.NumericUpDown();
			this.btnStop = new System.Windows.Forms.Button();
			this.label7 = new System.Windows.Forms.Label();
			this.gbReadOnce = new System.Windows.Forms.GroupBox();
			this.rbFC04 = new System.Windows.Forms.RadioButton();
			this.rbFC03 = new System.Windows.Forms.RadioButton();
			this.rbFC02 = new System.Windows.Forms.RadioButton();
			this.rbFC01 = new System.Windows.Forms.RadioButton();
			this.txtLen = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.txtRegister = new System.Windows.Forms.TextBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.tmrRefrash = new System.Windows.Forms.Timer(this.components);
			this.gbData = new System.Windows.Forms.GroupBox();
			this.dgData = new System.Windows.Forms.DataGridView();
			this.dcAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dcDecimal = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dcHex = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.labUpdateTime = new System.Windows.Forms.Label();
			this.gbConnect.SuspendLayout();
			this.panel1.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ndCycle)).BeginInit();
			this.gbReadOnce.SuspendLayout();
			this.gbData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgData)).BeginInit();
			this.SuspendLayout();
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStart.Location = new System.Drawing.Point(114, 62);
			this.btnStart.Margin = new System.Windows.Forms.Padding(4);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(73, 36);
			this.btnStart.TabIndex = 3;
			this.btnStart.Text = "Start";
			this.btnStart.UseVisualStyleBackColor = true;
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// gbConnect
			// 
			this.gbConnect.Controls.Add(this.label1);
			this.gbConnect.Controls.Add(this.label2);
			this.gbConnect.Controls.Add(this.txtRemoteIP);
			this.gbConnect.Controls.Add(this.txtPort);
			this.gbConnect.Dock = System.Windows.Forms.DockStyle.Top;
			this.gbConnect.Location = new System.Drawing.Point(0, 0);
			this.gbConnect.Margin = new System.Windows.Forms.Padding(4);
			this.gbConnect.Name = "gbConnect";
			this.gbConnect.Padding = new System.Windows.Forms.Padding(4);
			this.gbConnect.Size = new System.Drawing.Size(285, 81);
			this.gbConnect.TabIndex = 0;
			this.gbConnect.TabStop = false;
			this.gbConnect.Text = "Connection";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 22);
			this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(78, 17);
			this.label1.TabIndex = 0;
			this.label1.Text = "IP Address";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(152, 22);
			this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 17);
			this.label2.TabIndex = 2;
			this.label2.Text = "Port";
			// 
			// txtRemoteIP
			// 
			this.txtRemoteIP.Font = new System.Drawing.Font("Arial", 11F);
			this.txtRemoteIP.Location = new System.Drawing.Point(12, 45);
			this.txtRemoteIP.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.txtRemoteIP.Name = "txtRemoteIP";
			this.txtRemoteIP.Size = new System.Drawing.Size(130, 24);
			this.txtRemoteIP.TabIndex = 1;
			this.txtRemoteIP.Text = "127.0.0.1";
			// 
			// txtPort
			// 
			this.txtPort.Font = new System.Drawing.Font("Arial", 11F);
			this.txtPort.Location = new System.Drawing.Point(152, 45);
			this.txtPort.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.txtPort.Name = "txtPort";
			this.txtPort.Size = new System.Drawing.Size(51, 24);
			this.txtPort.TabIndex = 3;
			this.txtPort.Text = "502";
			// 
			// cbPause
			// 
			this.cbPause.AutoSize = true;
			this.cbPause.Location = new System.Drawing.Point(172, 31);
			this.cbPause.Name = "cbPause";
			this.cbPause.Size = new System.Drawing.Size(69, 21);
			this.cbPause.TabIndex = 2;
			this.cbPause.Text = "Pause";
			this.cbPause.UseVisualStyleBackColor = true;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(14, 32);
			this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(76, 17);
			this.label6.TabIndex = 0;
			this.label6.Text = "Cycle(ms)";
			// 
			// label5
			// 
			this.label5.Dock = System.Windows.Forms.DockStyle.Left;
			this.label5.Location = new System.Drawing.Point(294, 10);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(7, 409);
			this.label5.TabIndex = 1;
			// 
			// btnRequest
			// 
			this.btnRequest.Location = new System.Drawing.Point(177, 153);
			this.btnRequest.Name = "btnRequest";
			this.btnRequest.Size = new System.Drawing.Size(100, 36);
			this.btnRequest.TabIndex = 10;
			this.btnRequest.Text = "Request";
			this.btnRequest.UseVisualStyleBackColor = true;
			this.btnRequest.Click += new System.EventHandler(this.btnRequest_Click);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.groupBox1);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Controls.Add(this.gbReadOnce);
			this.panel1.Controls.Add(this.label4);
			this.panel1.Controls.Add(this.label10);
			this.panel1.Controls.Add(this.gbConnect);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.panel1.Location = new System.Drawing.Point(9, 10);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(285, 409);
			this.panel1.TabIndex = 0;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.cbPause);
			this.groupBox1.Controls.Add(this.ndCycle);
			this.groupBox1.Controls.Add(this.btnStop);
			this.groupBox1.Controls.Add(this.btnStart);
			this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
			this.groupBox1.Location = new System.Drawing.Point(0, 301);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(285, 107);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Monitor";
			// 
			// ndCycle
			// 
			this.ndCycle.Location = new System.Drawing.Point(93, 30);
			this.ndCycle.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
			this.ndCycle.Minimum = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.ndCycle.Name = "ndCycle";
			this.ndCycle.Size = new System.Drawing.Size(73, 24);
			this.ndCycle.TabIndex = 1;
			this.ndCycle.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
			this.ndCycle.ValueChanged += new System.EventHandler(this.ndCycle_ValueChanged);
			// 
			// btnStop
			// 
			this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnStop.Enabled = false;
			this.btnStop.Location = new System.Drawing.Point(204, 62);
			this.btnStop.Margin = new System.Windows.Forms.Padding(4);
			this.btnStop.Name = "btnStop";
			this.btnStop.Size = new System.Drawing.Size(73, 36);
			this.btnStop.TabIndex = 4;
			this.btnStop.Text = "Stop";
			this.btnStop.UseVisualStyleBackColor = true;
			this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
			// 
			// label7
			// 
			this.label7.Dock = System.Windows.Forms.DockStyle.Top;
			this.label7.Location = new System.Drawing.Point(0, 296);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(285, 5);
			this.label7.TabIndex = 3;
			// 
			// gbReadOnce
			// 
			this.gbReadOnce.Controls.Add(this.rbFC04);
			this.gbReadOnce.Controls.Add(this.rbFC03);
			this.gbReadOnce.Controls.Add(this.rbFC02);
			this.gbReadOnce.Controls.Add(this.rbFC01);
			this.gbReadOnce.Controls.Add(this.txtLen);
			this.gbReadOnce.Controls.Add(this.label13);
			this.gbReadOnce.Controls.Add(this.txtRegister);
			this.gbReadOnce.Controls.Add(this.label11);
			this.gbReadOnce.Controls.Add(this.btnRequest);
			this.gbReadOnce.Dock = System.Windows.Forms.DockStyle.Top;
			this.gbReadOnce.Location = new System.Drawing.Point(0, 91);
			this.gbReadOnce.Name = "gbReadOnce";
			this.gbReadOnce.Size = new System.Drawing.Size(285, 205);
			this.gbReadOnce.TabIndex = 2;
			this.gbReadOnce.TabStop = false;
			this.gbReadOnce.Text = "Read Data";
			// 
			// rbFC04
			// 
			this.rbFC04.AutoSize = true;
			this.rbFC04.Location = new System.Drawing.Point(12, 108);
			this.rbFC04.Name = "rbFC04";
			this.rbFC04.Size = new System.Drawing.Size(221, 21);
			this.rbFC04.TabIndex = 3;
			this.rbFC04.Text = "FC:04/Input Register(Word/R)";
			this.rbFC04.UseVisualStyleBackColor = true;
			// 
			// rbFC03
			// 
			this.rbFC03.AutoSize = true;
			this.rbFC03.Location = new System.Drawing.Point(12, 81);
			this.rbFC03.Name = "rbFC03";
			this.rbFC03.Size = new System.Drawing.Size(253, 21);
			this.rbFC03.TabIndex = 2;
			this.rbFC03.Text = "FC:03/Holding Register(Word/RW)";
			this.rbFC03.UseVisualStyleBackColor = true;
			// 
			// rbFC02
			// 
			this.rbFC02.AutoSize = true;
			this.rbFC02.Location = new System.Drawing.Point(12, 54);
			this.rbFC02.Name = "rbFC02";
			this.rbFC02.Size = new System.Drawing.Size(189, 21);
			this.rbFC02.TabIndex = 1;
			this.rbFC02.Text = "FC:02/Input Status(Bit/R)";
			this.rbFC02.UseVisualStyleBackColor = true;
			// 
			// rbFC01
			// 
			this.rbFC01.AutoSize = true;
			this.rbFC01.Checked = true;
			this.rbFC01.Location = new System.Drawing.Point(12, 27);
			this.rbFC01.Name = "rbFC01";
			this.rbFC01.Size = new System.Drawing.Size(198, 21);
			this.rbFC01.TabIndex = 0;
			this.rbFC01.TabStop = true;
			this.rbFC01.Text = "FC:01/Coil Status(Bit/RW)";
			this.rbFC01.UseVisualStyleBackColor = true;
			// 
			// txtLen
			// 
			this.txtLen.Font = new System.Drawing.Font("Arial", 11F);
			this.txtLen.Location = new System.Drawing.Point(95, 165);
			this.txtLen.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.txtLen.Name = "txtLen";
			this.txtLen.Size = new System.Drawing.Size(68, 24);
			this.txtLen.TabIndex = 9;
			this.txtLen.Text = "1";
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(95, 142);
			this.label13.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(52, 17);
			this.label13.TabIndex = 8;
			this.label13.Text = "Length";
			// 
			// txtRegister
			// 
			this.txtRegister.Font = new System.Drawing.Font("Arial", 11F);
			this.txtRegister.Location = new System.Drawing.Point(12, 165);
			this.txtRegister.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
			this.txtRegister.Name = "txtRegister";
			this.txtRegister.Size = new System.Drawing.Size(68, 24);
			this.txtRegister.TabIndex = 7;
			this.txtRegister.Text = "1";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(12, 142);
			this.label11.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(62, 17);
			this.label11.TabIndex = 6;
			this.label11.Text = "Address";
			// 
			// label4
			// 
			this.label4.Dock = System.Windows.Forms.DockStyle.Top;
			this.label4.Location = new System.Drawing.Point(0, 86);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(285, 5);
			this.label4.TabIndex = 1;
			// 
			// label10
			// 
			this.label10.Dock = System.Windows.Forms.DockStyle.Top;
			this.label10.Location = new System.Drawing.Point(0, 81);
			this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(285, 5);
			this.label10.TabIndex = 17;
			// 
			// tmrRefrash
			// 
			this.tmrRefrash.Interval = 3000;
			this.tmrRefrash.Tick += new System.EventHandler(this.tmrRefrash_Tick);
			// 
			// gbData
			// 
			this.gbData.Controls.Add(this.dgData);
			this.gbData.Controls.Add(this.labUpdateTime);
			this.gbData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gbData.Location = new System.Drawing.Point(301, 10);
			this.gbData.Name = "gbData";
			this.gbData.Padding = new System.Windows.Forms.Padding(8);
			this.gbData.Size = new System.Drawing.Size(285, 409);
			this.gbData.TabIndex = 2;
			this.gbData.TabStop = false;
			this.gbData.Text = "Received Data (Address=PLC Address)";
			// 
			// dgData
			// 
			this.dgData.AllowUserToAddRows = false;
			this.dgData.AllowUserToDeleteRows = false;
			this.dgData.AllowUserToResizeRows = false;
			this.dgData.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
			this.dgData.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dgData.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dcAddress,
            this.dcDecimal,
            this.dcHex});
			this.dgData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgData.Location = new System.Drawing.Point(8, 25);
			this.dgData.Name = "dgData";
			this.dgData.RowHeadersVisible = false;
			this.dgData.RowTemplate.Height = 24;
			this.dgData.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgData.Size = new System.Drawing.Size(269, 352);
			this.dgData.TabIndex = 0;
			this.dgData.CurrentCellDirtyStateChanged += new System.EventHandler(this.dgData_CurrentCellDirtyStateChanged);
			// 
			// dcAddress
			// 
			this.dcAddress.Frozen = true;
			this.dcAddress.HeaderText = "Address";
			this.dcAddress.Name = "dcAddress";
			this.dcAddress.ReadOnly = true;
			this.dcAddress.Width = 80;
			// 
			// dcDecimal
			// 
			this.dcDecimal.Frozen = true;
			this.dcDecimal.HeaderText = "Decimal";
			this.dcDecimal.Name = "dcDecimal";
			this.dcDecimal.ReadOnly = true;
			this.dcDecimal.Width = 80;
			// 
			// dcHex
			// 
			this.dcHex.Frozen = true;
			this.dcHex.HeaderText = "Hex";
			this.dcHex.Name = "dcHex";
			this.dcHex.ReadOnly = true;
			this.dcHex.Width = 80;
			// 
			// labUpdateTime
			// 
			this.labUpdateTime.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.labUpdateTime.Location = new System.Drawing.Point(8, 377);
			this.labUpdateTime.Name = "labUpdateTime";
			this.labUpdateTime.Size = new System.Drawing.Size(269, 24);
			this.labUpdateTime.TabIndex = 1;
			this.labUpdateTime.Text = "Last update : ?";
			this.labUpdateTime.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// fModbusTcpMaster
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 17F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(595, 429);
			this.Controls.Add(this.gbData);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.panel1);
			this.Font = new System.Drawing.Font("Arial", 11F);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Margin = new System.Windows.Forms.Padding(4);
			this.MaximizeBox = false;
			this.Name = "fModbusTcpMaster";
			this.Padding = new System.Windows.Forms.Padding(9, 10, 9, 10);
			this.Text = "ModbusTCP Master";
			this.gbConnect.ResumeLayout(false);
			this.gbConnect.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ndCycle)).EndInit();
			this.gbReadOnce.ResumeLayout(false);
			this.gbReadOnce.PerformLayout();
			this.gbData.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgData)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.GroupBox gbConnect;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button btnRequest;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox gbReadOnce;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.TextBox txtRegister;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.RadioButton rbFC01;
		private System.Windows.Forms.RadioButton rbFC04;
		private System.Windows.Forms.RadioButton rbFC03;
		private System.Windows.Forms.RadioButton rbFC02;
		private System.Windows.Forms.TextBox txtLen;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Timer tmrRefrash;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtPort;
		private System.Windows.Forms.GroupBox gbData;
		private System.Windows.Forms.DataGridView dgData;
		private System.Windows.Forms.CheckBox cbPause;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcAddress;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcDecimal;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcHex;
		private System.Windows.Forms.TextBox txtRemoteIP;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown ndCycle;
		private System.Windows.Forms.Label labUpdateTime;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button btnStop;
		private System.Windows.Forms.Label label7;
	}
}

