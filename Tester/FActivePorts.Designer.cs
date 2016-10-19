namespace Tester
{
	partial class FActivePorts
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
			this.cbLocalIP = new System.Windows.Forms.ComboBox();
			this.dgvList = new System.Windows.Forms.DataGridView();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.dcLocal = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dcRemote = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dcStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dcProcessID = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.dcProcess = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbRemoteIP = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cbStatus = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.cbProcess = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.dgvList)).BeginInit();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "Local IP";
			// 
			// cbLocalIP
			// 
			this.cbLocalIP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbLocalIP.FormattingEnabled = true;
			this.cbLocalIP.Location = new System.Drawing.Point(17, 39);
			this.cbLocalIP.Name = "cbLocalIP";
			this.cbLocalIP.Size = new System.Drawing.Size(139, 20);
			this.cbLocalIP.Sorted = true;
			this.cbLocalIP.TabIndex = 1;
			// 
			// dgvList
			// 
			this.dgvList.AllowUserToAddRows = false;
			this.dgvList.AllowUserToDeleteRows = false;
			this.dgvList.AllowUserToResizeRows = false;
			this.dgvList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dgvList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
			this.dgvList.ColumnHeadersHeight = 24;
			this.dgvList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dcLocal,
            this.dcRemote,
            this.dcStatus,
            this.dcProcessID,
            this.dcProcess});
			this.dgvList.Location = new System.Drawing.Point(12, 94);
			this.dgvList.Name = "dgvList";
			this.dgvList.RowHeadersWidth = 24;
			this.dgvList.RowTemplate.Height = 24;
			this.dgvList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.dgvList.Size = new System.Drawing.Size(761, 426);
			this.dgvList.TabIndex = 2;
			// 
			// btnRefresh
			// 
			this.btnRefresh.Location = new System.Drawing.Point(698, 31);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(75, 40);
			this.btnRefresh.TabIndex = 3;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// dcLocal
			// 
			this.dcLocal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dcLocal.HeaderText = "Local Port";
			this.dcLocal.Name = "dcLocal";
			this.dcLocal.ReadOnly = true;
			this.dcLocal.Width = 78;
			// 
			// dcRemote
			// 
			this.dcRemote.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dcRemote.HeaderText = "Remote Port";
			this.dcRemote.Name = "dcRemote";
			this.dcRemote.ReadOnly = true;
			this.dcRemote.Width = 88;
			// 
			// dcStatus
			// 
			this.dcStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dcStatus.HeaderText = "Status";
			this.dcStatus.Name = "dcStatus";
			this.dcStatus.ReadOnly = true;
			this.dcStatus.Width = 57;
			// 
			// dcProcessID
			// 
			this.dcProcessID.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dcProcessID.HeaderText = "ProcessID";
			this.dcProcessID.Name = "dcProcessID";
			this.dcProcessID.ReadOnly = true;
			this.dcProcessID.Width = 76;
			// 
			// dcProcess
			// 
			this.dcProcess.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
			this.dcProcess.HeaderText = "Process Name";
			this.dcProcess.Name = "dcProcess";
			this.dcProcess.ReadOnly = true;
			this.dcProcess.Width = 94;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.cbProcess);
			this.groupBox1.Controls.Add(this.cbStatus);
			this.groupBox1.Controls.Add(this.cbRemoteIP);
			this.groupBox1.Controls.Add(this.cbLocalIP);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(667, 76);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Filter By";
			// 
			// cbRemoteIP
			// 
			this.cbRemoteIP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbRemoteIP.FormattingEnabled = true;
			this.cbRemoteIP.Location = new System.Drawing.Point(162, 39);
			this.cbRemoteIP.Name = "cbRemoteIP";
			this.cbRemoteIP.Size = new System.Drawing.Size(139, 20);
			this.cbRemoteIP.Sorted = true;
			this.cbRemoteIP.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(160, 24);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(54, 12);
			this.label2.TabIndex = 0;
			this.label2.Text = "Remote IP";
			// 
			// cbStatus
			// 
			this.cbStatus.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbStatus.FormattingEnabled = true;
			this.cbStatus.Location = new System.Drawing.Point(307, 39);
			this.cbStatus.Name = "cbStatus";
			this.cbStatus.Size = new System.Drawing.Size(139, 20);
			this.cbStatus.Sorted = true;
			this.cbStatus.TabIndex = 1;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(305, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(32, 12);
			this.label3.TabIndex = 0;
			this.label3.Text = "Status";
			// 
			// cbProcess
			// 
			this.cbProcess.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbProcess.FormattingEnabled = true;
			this.cbProcess.Location = new System.Drawing.Point(452, 39);
			this.cbProcess.Name = "cbProcess";
			this.cbProcess.Size = new System.Drawing.Size(195, 20);
			this.cbProcess.TabIndex = 1;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(450, 24);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(39, 12);
			this.label4.TabIndex = 0;
			this.label4.Text = "Process";
			// 
			// FActivePorts
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(785, 532);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.dgvList);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FActivePorts";
			this.Text = "FActivePorts";
			((System.ComponentModel.ISupportInitialize)(this.dgvList)).EndInit();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbLocalIP;
		private System.Windows.Forms.DataGridView dgvList;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcLocal;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcRemote;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcStatus;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcProcessID;
		private System.Windows.Forms.DataGridViewTextBoxColumn dcProcess;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cbProcess;
		private System.Windows.Forms.ComboBox cbStatus;
		private System.Windows.Forms.ComboBox cbRemoteIP;
	}
}