namespace Tester
{
    partial class FSocketMonitor
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
            this.panTop = new System.Windows.Forms.Panel();
            this.cbIP = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExec = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvSkt = new System.Windows.Forms.ListView();
            this.chTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDest = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chProtocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chLength = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.cbFillter = new System.Windows.Forms.ComboBox();
            this.cbProtocol = new System.Windows.Forms.ComboBox();
            this.panTop.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panTop
            // 
            this.panTop.Controls.Add(this.label2);
            this.panTop.Controls.Add(this.cbProtocol);
            this.panTop.Controls.Add(this.cbFillter);
            this.panTop.Controls.Add(this.cbIP);
            this.panTop.Controls.Add(this.label1);
            this.panTop.Controls.Add(this.btnExec);
            this.panTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTop.Location = new System.Drawing.Point(10, 10);
            this.panTop.Name = "panTop";
            this.panTop.Size = new System.Drawing.Size(780, 31);
            this.panTop.TabIndex = 0;
            // 
            // cbIP
            // 
            this.cbIP.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbIP.FormattingEnabled = true;
            this.cbIP.Location = new System.Drawing.Point(25, 6);
            this.cbIP.Name = "cbIP";
            this.cbIP.Size = new System.Drawing.Size(137, 20);
            this.cbIP.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(15, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "IP";
            // 
            // btnExec
            // 
            this.btnExec.Location = new System.Drawing.Point(702, 5);
            this.btnExec.Name = "btnExec";
            this.btnExec.Size = new System.Drawing.Size(75, 23);
            this.btnExec.TabIndex = 0;
            this.btnExec.Text = "Start";
            this.btnExec.UseVisualStyleBackColor = true;
            this.btnExec.Click += new System.EventHandler(this.btnExec_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lvSkt);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(10, 41);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(780, 399);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "封包清單";
            // 
            // lvSkt
            // 
            this.lvSkt.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chTime,
            this.chSource,
            this.chDest,
            this.chProtocol,
            this.chLength});
            this.lvSkt.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSkt.FullRowSelect = true;
            this.lvSkt.LabelWrap = false;
            this.lvSkt.Location = new System.Drawing.Point(3, 18);
            this.lvSkt.Name = "lvSkt";
            this.lvSkt.Size = new System.Drawing.Size(774, 378);
            this.lvSkt.TabIndex = 0;
            this.lvSkt.UseCompatibleStateImageBehavior = false;
            this.lvSkt.View = System.Windows.Forms.View.Details;
            // 
            // chTime
            // 
            this.chTime.Text = "Time";
            this.chTime.Width = 107;
            // 
            // chSource
            // 
            this.chSource.Text = "Source";
            this.chSource.Width = 113;
            // 
            // chDest
            // 
            this.chDest.Text = "Destination";
            this.chDest.Width = 106;
            // 
            // chProtocol
            // 
            this.chProtocol.Text = "Protocol";
            this.chProtocol.Width = 100;
            // 
            // chLength
            // 
            this.chLength.Text = "Length";
            this.chLength.Width = 106;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(32, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Fillter";
            // 
            // cbFillter
            // 
            this.cbFillter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFillter.FormattingEnabled = true;
            this.cbFillter.Location = new System.Drawing.Point(216, 6);
            this.cbFillter.Name = "cbFillter";
            this.cbFillter.Size = new System.Drawing.Size(137, 20);
            this.cbFillter.TabIndex = 2;
            this.cbFillter.SelectedIndexChanged += new System.EventHandler(this.cbFillter_SelectedIndexChanged);
            // 
            // cbProtocol
            // 
            this.cbProtocol.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProtocol.FormattingEnabled = true;
            this.cbProtocol.Location = new System.Drawing.Point(359, 6);
            this.cbProtocol.Name = "cbProtocol";
            this.cbProtocol.Size = new System.Drawing.Size(78, 20);
            this.cbProtocol.TabIndex = 2;
            this.cbProtocol.SelectedIndexChanged += new System.EventHandler(this.cbFillter_SelectedIndexChanged);
            // 
            // FSocketMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panTop);
            this.Name = "FSocketMonitor";
            this.Padding = new System.Windows.Forms.Padding(10);
            this.Text = "FSocketMonitor";
            this.panTop.ResumeLayout(false);
            this.panTop.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panTop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnExec;
        private System.Windows.Forms.ListView lvSkt;
        private System.Windows.Forms.ColumnHeader chTime;
        private System.Windows.Forms.ColumnHeader chSource;
        private System.Windows.Forms.ColumnHeader chDest;
        private System.Windows.Forms.ColumnHeader chProtocol;
        private System.Windows.Forms.ColumnHeader chLength;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbFillter;
        private System.Windows.Forms.ComboBox cbProtocol;
    }
}