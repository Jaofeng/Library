using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CJF.Utility.WinKits
{
	#region Private Class : MsgDialog
	class MsgDialog : Form
	{
		#region DLL Import
		//[DllImport("user32.dll")]
		//static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
		[DllImport("user32.dll")]
		static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
		[DllImport("user32.dll")]
		static extern IntPtr RemoveMenu(IntPtr hMenu, uint nPosition, uint wFlags);
		internal const uint SC_CLOSE = 0xF060;
		internal const uint MF_GRAYED = 0x00000001;
		internal const uint MF_BYCOMMAND = 0x00000000;
		#endregion

		#region Controls
		/// <summary>Required designer variable.</summary>
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.Button btn3;
		private System.Windows.Forms.Button btn2;
		private System.Windows.Forms.Button btn1;
		private System.Windows.Forms.Panel panCanvas;
		private System.Windows.Forms.GroupBox gbSpareLine;
		private System.Windows.Forms.FlowLayoutPanel flpButtons;
		#endregion

		#region Internal Static Variables
		/// <summary>視窗最大範圍</summary>
		internal static Size DialogMaxSize = Size.Empty;
		/// <summary>視窗最大範圍</summary>
		internal static Size DialogMinSize = Size.Empty;
		/// <summary>視窗文字字型</summary>
		internal static Font TextFont = SystemFonts.MessageBoxFont;

		internal static string ButtonOK = "確定(&O)";
		internal static string ButtonCancel = "取消(&C)";
		internal static string ButtonYes = "是(&Y)";
		internal static string ButtonNo = "否(&N)";
		internal static string ButtonRetry = "重試(&R)";
		internal static string ButtonAbort = "中止(&A)";
		internal static string ButtonIgnore = "忽略(&I)";
		#endregion

		#region Private Variables
		string _Message = string.Empty;
		string _PureText = string.Empty;

		MessageBoxIcon _MsgIcon = MessageBoxIcon.None;
		SizeF _TextSize = SizeF.Empty;
		bool _HasANSI = false;
		int _MaxTextAreaWidth = 0;
		Icon _Icon = null;
		Size _MsgArea = Size.Empty;
		Size _BtnArea = Size.Empty;
		#endregion

		#region Construct Method : dgMsg(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
		/// <summary>建立 CJF.Utility.WinKits.MsgDialog 類別</summary>
		/// <param name="text">欲顯示的文字訊息，可傳入 AnsiString 產生的 ANSI Code(\x1B[xx) 字串</param>
		/// <param name="caption">欲顯示的視窗標題文字</param>
		/// <param name="button">欲顯示的按鈕</param>
		/// <param name="icon">欲顯示的圖示</param>
		/// <param name="defButton">預設按鈕</param>
		private MsgDialog(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
		{
			InitializeComponent();
			panCanvas.Paint += new PaintEventHandler(panCanvas_Paint);

			this.MaximumSize = DialogMaxSize;
			this.MinimumSize = DialogMinSize;
			this.DialogResult = DialogResult.None;
			this.Text = caption;
			if (TextFont != null)
				this.Font = TextFont;
			else
				this.Font = SystemFonts.MessageBoxFont;
			this.Size = Size.Empty;

			SetButtons(button, defButton);

			_Message = text;
			_MsgIcon = icon;
			_PureText = AnsiString.RemoveANSI(text);
			_Icon = GetIcon(icon);
			Graphics g = panCanvas.CreateGraphics();
			Rectangle sr = Screen.GetWorkingArea(this);
			if (DialogMaxSize.IsEmpty)
				_MaxTextAreaWidth = sr.Width / 3;
			else
				_MaxTextAreaWidth = DialogMaxSize.Width;
			_MaxTextAreaWidth -= SystemInformation.Border3DSize.Width * 2 - panCanvas.Padding.Horizontal;
			if (_MsgIcon != MessageBoxIcon.None)
				_MaxTextAreaWidth -= _Icon.Width + btn1.Margin.Left;
			_TextSize = g.MeasureString(_PureText, this.Font, _MaxTextAreaWidth);
			_HasANSI = !_PureText.Equals(_Message);

			_BtnArea = new Size(0, flpButtons.Height);

			// 設定訊息顯示區大小
			if (_Icon != null)
			{
				_MsgArea.Width = (int)_TextSize.Width + _Icon.Width + btn1.Margin.Left;
				_MsgArea.Height = ((_TextSize.Height > _Icon.Height) ? (int)_TextSize.Height : _Icon.Height);
			}
			else
			{
				_MsgArea.Width = (int)_TextSize.Width;
				_MsgArea.Height = (int)_TextSize.Height;
			}

			// 計算按鍵區寬度
			_BtnArea.Width = btn1.Width + (btn2.Visible ? btn2.Width + btn2.Margin.Left : 0) + (btn3.Visible ? btn3.Width + btn3.Margin.Left : 0);
			_BtnArea.Width += flpButtons.Padding.Right * 2 + flpButtons.Margin.Horizontal;

			this.Height = flpButtons.Height + gbSpareLine.Height + _MsgArea.Height + SystemInformation.Border3DSize.Height + SystemInformation.CaptionHeight + panCanvas.Padding.Vertical;
			this.Width = (_BtnArea.Width > _MsgArea.Width ? _BtnArea.Width : _MsgArea.Width) + SystemInformation.Border3DSize.Width * 2 + panCanvas.Padding.Horizontal;

			if (this.Parent != null || this.Owner != null)
				this.CenterToParent();
			else
				this.CenterToScreen();


		}
		#endregion

		#region Private Method : void panCanvas_Paint(object sender, PaintEventArgs e)
		private void panCanvas_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			DrawCanvas(g, _Message);
		}
		#endregion

		#region Private Method : void Buttons_Click(object sender, EventArgs e)
		private void Buttons_Click(object sender, EventArgs e)
		{
			Button btn = (Button)sender;
			this.DialogResult = btn.DialogResult;
			this.Close();
		}
		#endregion

		#region Private Method : Icon GetIcon(MessageBoxIcon icon)
		private Icon GetIcon(MessageBoxIcon icon)
		{
			switch (icon)
			{
				case MessageBoxIcon.None:
					return null;
				case MessageBoxIcon.Information:
					return SystemIcons.Information;
				case MessageBoxIcon.Error:
					return SystemIcons.Error;
				case MessageBoxIcon.Warning:
					return SystemIcons.Warning;
				case MessageBoxIcon.Question:
					return SystemIcons.Question;
				default:
					throw new InvalidEnumArgumentException();
			}
		}
		#endregion

		#region Private Method : void SetButtons(MessageBoxButtons btns = MessageBoxButtons.OK)
		private void SetButtons(MessageBoxButtons btns, MessageBoxDefaultButton defbtn)
		{
			bool enableCloseButton = true;

			#region Setting Button Attribs
			switch (btns)
			{
				case MessageBoxButtons.OK:
					btn1.DialogResult = DialogResult.OK;
					btn2.DialogResult = DialogResult.None;
					btn3.DialogResult = DialogResult.None;
					btn1.Text = ButtonOK;
					btn2.Text = string.Empty;
					btn3.Text = string.Empty;
					btn1.Visible = true;
					btn2.Visible = btn3.Visible = false;
					this.CancelButton = this.AcceptButton = btn1;
					enableCloseButton = true;
					break;
				case MessageBoxButtons.OKCancel:
					btn1.DialogResult = DialogResult.OK;
					btn2.DialogResult = DialogResult.Cancel;
					btn3.DialogResult = DialogResult.None;
					btn1.Text = ButtonOK;
					btn2.Text = ButtonCancel;
					btn3.Text = string.Empty;
					btn1.Visible = btn2.Visible = true;
					btn3.Visible = false;
					this.AcceptButton = btn1;
					this.CancelButton = btn2;
					enableCloseButton = true;
					break;
				case MessageBoxButtons.YesNo:
					btn1.DialogResult = DialogResult.Yes;
					btn2.DialogResult = DialogResult.No;
					btn3.DialogResult = DialogResult.None;
					btn1.Text = ButtonYes;
					btn2.Text = ButtonNo;
					btn3.Text = string.Empty;
					btn1.Visible = btn2.Visible = true;
					btn3.Visible = false;
					this.AcceptButton = null;
					this.CancelButton = null;
					enableCloseButton = false;
					break;
				case MessageBoxButtons.YesNoCancel:
					btn1.DialogResult = DialogResult.Yes;
					btn2.DialogResult = DialogResult.No;
					btn3.DialogResult = DialogResult.Cancel;
					btn1.Text = ButtonYes;
					btn2.Text = ButtonNo;
					btn3.Text = ButtonCancel;
					btn1.Visible = btn2.Visible = btn3.Visible = true;
					this.AcceptButton = null;
					this.CancelButton = btn3;
					enableCloseButton = true;
					break;
				case MessageBoxButtons.RetryCancel:
					btn1.DialogResult = DialogResult.Retry;
					btn2.DialogResult = DialogResult.Cancel;
					btn3.DialogResult = DialogResult.None;
					btn1.Text = ButtonRetry;
					btn2.Text = ButtonCancel;
					btn3.Text = string.Empty;
					btn1.Visible = btn2.Visible = true;
					btn3.Visible = false;
					this.AcceptButton = btn1;
					this.CancelButton = btn2;
					enableCloseButton = true;
					break;
				case MessageBoxButtons.AbortRetryIgnore:
					btn1.DialogResult = DialogResult.Abort;
					btn2.DialogResult = DialogResult.Retry;
					btn3.DialogResult = DialogResult.Ignore;
					btn1.Text = ButtonAbort;
					btn2.Text = ButtonRetry;
					btn3.Text = ButtonIgnore;
					btn1.Visible = btn2.Visible = btn3.Visible = true;
					this.AcceptButton = null;
					this.CancelButton = null;
					enableCloseButton = false;
					break;
				default:
					throw new InvalidEnumArgumentException();
			}
			#endregion

			#region Setting Default Button
			switch (defbtn)
			{
				case MessageBoxDefaultButton.Button2:
					btn2.Focus();
					break;
				case MessageBoxDefaultButton.Button3:
					btn3.Focus();
					break;
				case MessageBoxDefaultButton.Button1:
					btn1.Focus();
					break;
				default:
					throw new InvalidEnumArgumentException();
			}
			#endregion

			#region Disable Window Close Button
			if (!enableCloseButton)
			{
				IntPtr hMenu = this.Handle;
				IntPtr hSystemMenu = GetSystemMenu(hMenu, false);
				RemoveMenu(hSystemMenu, SC_CLOSE, MF_BYCOMMAND);
			}
			#endregion
		}
		#endregion

		#region Private Method : void DrawCanvas(Graphics grap, string text)
		private void DrawCanvas(Graphics grap, string text)
		{
			grap.Clear(SystemColors.Window);
			Point leftTop = new Point(panCanvas.Padding.Left, panCanvas.Padding.Top);
			SizeF sf = grap.MeasureString(text, this.Font, _MaxTextAreaWidth);

			#region Draw Icon
			Icon ico = GetIcon(_MsgIcon);
			if (ico != null)
				grap.DrawIcon(ico, leftTop.X, leftTop.Y);
			#endregion

			#region For Debug
#if DEBUG
			grap.DrawRectangle(new Pen(Color.LightGray), new Rectangle(leftTop, _MsgArea));
#endif
			#endregion

			Rectangle rect = new Rectangle(leftTop, new Size(10, 10));
			Point msgLoc = leftTop;
			if (ico != null)
			{
				if (sf.Height < ico.Height)
					msgLoc.Offset(ico.Width + btn1.Margin.Left, (int)((ico.Height - sf.Height) / 2));
				else
					msgLoc.Offset(ico.Width + btn1.Margin.Left, 0);
			}
			grap.DrawString(text, this.Font, SystemBrushes.WindowText, new RectangleF(msgLoc, sf));
		}
		#endregion

		#region Internal Static Method : DialogResult ShowBox(...)
		internal static DialogResult ShowBox(
			IWin32Window owner,
			string text,
			string caption = "",
			MessageBoxButtons button = MessageBoxButtons.OK,
			MessageBoxIcon icon = MessageBoxIcon.None,
			MessageBoxDefaultButton defButton = MessageBoxDefaultButton.Button1)
		{
			using (MsgDialog dg = new MsgDialog(text, caption, button, icon, defButton))
			{
				DialogResult res = DialogResult.None;
				if (Application.MessageLoop)
				{
					if (owner != null)
						res = dg.ShowDialog(owner);
					else
						res = dg.ShowDialog();
				}
				else
				{
					Application.Run(dg);
					res = dg.DialogResult;
				}
				return res;
			}
		}
		#endregion

		#region Protected Override Method : void Dispose(bool disposing)
		/// <summary>Clean up any resources being used.</summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btn3 = new System.Windows.Forms.Button();
			this.flpButtons = new System.Windows.Forms.FlowLayoutPanel();
			this.btn2 = new System.Windows.Forms.Button();
			this.btn1 = new System.Windows.Forms.Button();
			this.gbSpareLine = new System.Windows.Forms.GroupBox();
			this.panCanvas = new System.Windows.Forms.Panel();
			this.flpButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// btn3
			// 
			this.btn3.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn3.Location = new System.Drawing.Point(188, 11);
			this.btn3.Margin = new System.Windows.Forms.Padding(8, 1, 1, 1);
			this.btn3.Name = "btn3";
			this.btn3.Size = new System.Drawing.Size(80, 28);
			this.btn3.TabIndex = 2;
			this.btn3.Text = "取消(&C)";
			this.btn3.UseVisualStyleBackColor = true;
			this.btn3.Click += new System.EventHandler(this.Buttons_Click);
			// 
			// flpButtons
			// 
			this.flpButtons.Controls.Add(this.btn3);
			this.flpButtons.Controls.Add(this.btn2);
			this.flpButtons.Controls.Add(this.btn1);
			this.flpButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.flpButtons.Location = new System.Drawing.Point(0, 89);
			this.flpButtons.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.flpButtons.Name = "flpButtons";
			this.flpButtons.Padding = new System.Windows.Forms.Padding(0, 10, 10, 10);
			this.flpButtons.Size = new System.Drawing.Size(279, 50);
			this.flpButtons.TabIndex = 0;
			this.flpButtons.WrapContents = false;
			// 
			// btn2
			// 
			this.btn2.Location = new System.Drawing.Point(99, 11);
			this.btn2.Margin = new System.Windows.Forms.Padding(8, 1, 1, 1);
			this.btn2.Name = "btn2";
			this.btn2.Size = new System.Drawing.Size(80, 28);
			this.btn2.TabIndex = 1;
			this.btn2.Text = "否(&N)";
			this.btn2.UseVisualStyleBackColor = true;
			this.btn2.Click += new System.EventHandler(this.Buttons_Click);
			// 
			// btn1
			// 
			this.btn1.Location = new System.Drawing.Point(10, 11);
			this.btn1.Margin = new System.Windows.Forms.Padding(8, 1, 1, 1);
			this.btn1.Name = "btn1";
			this.btn1.Size = new System.Drawing.Size(80, 28);
			this.btn1.TabIndex = 0;
			this.btn1.Text = "是(&Y)";
			this.btn1.UseVisualStyleBackColor = true;
			this.btn1.Click += new System.EventHandler(this.Buttons_Click);
			// 
			// gbSpareLine
			// 
			this.gbSpareLine.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.gbSpareLine.Location = new System.Drawing.Point(0, 86);
			this.gbSpareLine.Margin = new System.Windows.Forms.Padding(1);
			this.gbSpareLine.Name = "gbSpareLine";
			this.gbSpareLine.Padding = new System.Windows.Forms.Padding(1);
			this.gbSpareLine.Size = new System.Drawing.Size(279, 3);
			this.gbSpareLine.TabIndex = 1;
			this.gbSpareLine.TabStop = false;
			// 
			// panCanvas
			// 
			this.panCanvas.BackColor = System.Drawing.SystemColors.Window;
			this.panCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panCanvas.Location = new System.Drawing.Point(0, 0);
			this.panCanvas.Margin = new System.Windows.Forms.Padding(1);
			this.panCanvas.Name = "panCanvas";
			this.panCanvas.Padding = new System.Windows.Forms.Padding(25, 30, 25, 30);
			this.panCanvas.Size = new System.Drawing.Size(279, 86);
			this.panCanvas.TabIndex = 2;
			// 
			// MsgDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(279, 139);
			this.Controls.Add(this.panCanvas);
			this.Controls.Add(this.gbSpareLine);
			this.Controls.Add(this.flpButtons);
			this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MsgDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Caption";
			this.flpButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

	}
	#endregion

	#region Public Static Class : MessageBox
	/// <summary>自訂類別 MessageBox</summary>
	public static class MessageBox
	{
		#region Static Property : Font Font(R/W)
		/// <summary>設定或取得訊息窗的字型</summary>
		public static Font Font
		{
			get { return MsgDialog.TextFont; }
			set { MsgDialog.TextFont = value; }
		}
		#endregion

		#region Static Property : Size MaximumSize(R/W)
		/// <summary>設定或取得視窗最大範圍</summary>
		public static Size MaximumSize
		{
			get { return MsgDialog.DialogMaxSize; }
			set { MsgDialog.DialogMaxSize = value; }
		}
		#endregion

		#region Static Property : string ButtonOK
		/// <summary>設定或取得「確定」鈕的文字</summary>
		public static string ButtonOK
		{
			get { return MsgDialog.ButtonOK; }
			set { MsgDialog.ButtonOK = value; }
		}
		#endregion

		#region Static Property : string ButtonCancel
		/// <summary>設定或取得「取消」鈕的文字</summary>
		public static string ButtonCancel
		{
			get { return MsgDialog.ButtonCancel; }
			set { MsgDialog.ButtonCancel = value; }
		}
		#endregion

		#region Static Property : string ButtonYes
		/// <summary>設定或取得「是」鈕的文字</summary>
		public static string ButtonYes
		{
			get { return MsgDialog.ButtonYes; }
			set { MsgDialog.ButtonYes = value; }
		}
		#endregion

		#region Static Property : string ButtonNo
		/// <summary>設定或取得「否」鈕的文字</summary>
		public static string ButtonNo
		{
			get { return MsgDialog.ButtonNo; }
			set { MsgDialog.ButtonNo = value; }
		}
		#endregion

		#region Static Property : string ButtonRetry
		/// <summary>設定或取得「重試」鈕的文字</summary>
		public static string ButtonRetry
		{
			get { return MsgDialog.ButtonRetry; }
			set { MsgDialog.ButtonRetry = value; }
		}
		#endregion

		#region Static Property : string ButtonAbort
		/// <summary>設定或取得「中止」鈕的文字</summary>
		public static string ButtonAbort
		{
			get { return MsgDialog.ButtonAbort; }
			set { MsgDialog.ButtonAbort = value; }
		}
		#endregion

		#region Static Property : string ButtonIgnore
		/// <summary>設定或取得「忽略」鈕的文字</summary>
		public static string ButtonIgnore
		{
			get { return MsgDialog.ButtonIgnore; }
			set { MsgDialog.ButtonIgnore = value; }
		}
		#endregion

		#region Public Static Method : DialogResult Show(string text)
		/// <summary>顯示含有指定文字的訊息方塊。</summary>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		public static DialogResult Show(string text)
		{
			return MsgDialog.ShowBox(null, text);
		}
		#endregion

		#region Public Static Method : DialogResult Show(string text, string caption)
		/// <summary>顯示含有指定文字和標題的訊息方塊。</summary>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		public static DialogResult Show(string text, string caption)
		{
			return MsgDialog.ShowBox(null, text, caption);
		}
		#endregion

		#region Public Static Method : DialogResult Show(string text, string caption, MessageBoxButtons button)
		/// <summary>顯示含有指定文字、標題和按鈕的訊息方塊。</summary>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <param name="button">其中一個 System.Windows.Forms.MessageBoxButtons 值，指定要在訊息方塊中顯示哪些按鈕。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
		/// 指定的 buttons 參數不是 System.Windows.Forms.MessageBoxButtons 的成員。
		/// </exception>
		public static DialogResult Show(string text, string caption, MessageBoxButtons button)
		{
			return MsgDialog.ShowBox(null, text, caption, button);
		}
		#endregion

		#region Public Static Method : DialogResult Show(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
		/// <summary>顯示含有指定文字、標題、按鈕和圖示的訊息方塊。</summary>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <param name="button">其中一個 System.Windows.Forms.MessageBoxButtons 值，指定要在訊息方塊中顯示哪些按鈕。</param>
		/// <param name="icon">其中一個 System.Windows.Forms.MessageBoxIcon 值，指定那個圖示要顯示在訊息方塊中。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
		/// 指定的 buttons 參數不是 System.Windows.Forms.MessageBoxButtons 的成員。
		/// <para>-或-指定的 icon 參數不是 System.Windows.Forms.MessageBoxIcon 的成員。</para>
		/// </exception>
		public static DialogResult Show(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
		{
			return MsgDialog.ShowBox(null, text, caption, button, icon);
		}
		#endregion

		#region Public Static Method : DialogResult Show(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
		/// <summary>顯示含有指定文字、標題、圖示和預設按鈕的訊息方塊。</summary>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <param name="button">其中一個 System.Windows.Forms.MessageBoxButtons 值，指定要在訊息方塊中顯示哪些按鈕。</param>
		/// <param name="icon">其中一個 System.Windows.Forms.MessageBoxIcon 值，指定那個圖示要顯示在訊息方塊中。</param>
		/// <param name="defButton">其中一個 System.Windows.Forms.MessageBoxDefaultButton 值，指定訊息方塊的預設按鈕。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
		/// 指定的 buttons 參數不是 System.Windows.Forms.MessageBoxButtons 的成員。
		/// <para>-或-指定的 icon 參數不是 System.Windows.Forms.MessageBoxIcon 的成員。</para>
		/// </exception>
		public static DialogResult Show(string text, string caption, MessageBoxButtons button, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
		{
			return MsgDialog.ShowBox(null, text, caption, button, icon, defButton);
		}
		#endregion

		#region Public Static Method : DialogResult Show(IWin32Window owner, string text)
		/// <summary>在指定物件的前面顯示含有指定文字的訊息方塊。</summary>
		/// <param name="owner">System.Windows.Forms.IWin32Window 實作，將擁有強制回應對話方塊。</param>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		public static DialogResult Show(IWin32Window owner, string text)
		{
			return MsgDialog.ShowBox(owner, text);
		}
		#endregion

		#region Public Static Method : DialogResult Show(IWin32Window owner, string text, string caption)
		/// <summary>在指定物件的前面顯示含有指定文字和標題的訊息方塊。</summary>
		/// <param name="owner">System.Windows.Forms.IWin32Window 實作，將擁有強制回應對話方塊。</param>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		public static DialogResult Show(IWin32Window owner, string text, string caption)
		{
			return MsgDialog.ShowBox(owner, text, caption);
		}
		#endregion

		#region Public Static Method : DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons button)
		/// <summary>在指定物件的前面顯示含有指定文字、標題和按鈕的訊息方塊。</summary>
		/// <param name="owner">System.Windows.Forms.IWin32Window 實作，將擁有強制回應對話方塊。</param>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <param name="button">其中一個 System.Windows.Forms.MessageBoxButtons 值，指定要在訊息方塊中顯示哪些按鈕。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
		/// 指定的 buttons 參數不是 System.Windows.Forms.MessageBoxButtons 的成員。
		/// </exception>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons button)
		{
			return MsgDialog.ShowBox(owner, text, caption, button);
		}
		#endregion

		#region Public Static Method : DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
		/// <summary>在指定物件的前面顯示含有指定文字、標題、按鈕和圖示的訊息方塊。</summary>
		/// <param name="owner">System.Windows.Forms.IWin32Window 實作，將擁有強制回應對話方塊。</param>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <param name="button">其中一個 System.Windows.Forms.MessageBoxButtons 值，指定要在訊息方塊中顯示哪些按鈕。</param>
		/// <param name="icon">其中一個 System.Windows.Forms.MessageBoxIcon 值，指定那個圖示要顯示在訊息方塊中。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
		/// 指定的 buttons 參數不是 System.Windows.Forms.MessageBoxButtons 的成員。
		/// <para>-或-指定的 icon 參數不是 System.Windows.Forms.MessageBoxIcon 的成員。</para>
		/// </exception>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons button, MessageBoxIcon icon)
		{
			return MsgDialog.ShowBox(owner, text, caption, button, icon);
		}
		#endregion

		#region Public Static Method : DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons button, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
		/// <summary>在指定物件的前面顯示含有指定文字、標題、圖示和預設按鈕的訊息方塊。</summary>
		/// <param name="owner">System.Windows.Forms.IWin32Window 實作，將擁有強制回應對話方塊。</param>
		/// <param name="text">要顯示在訊息方塊中的文字。</param>
		/// <param name="caption">要顯示在訊息方塊標題列中的文字。</param>
		/// <param name="button">其中一個 System.Windows.Forms.MessageBoxButtons 值，指定要在訊息方塊中顯示哪些按鈕。</param>
		/// <param name="icon">其中一個 System.Windows.Forms.MessageBoxIcon 值，指定那個圖示要顯示在訊息方塊中。</param>
		/// <param name="defButton">其中一個 System.Windows.Forms.MessageBoxDefaultButton 值，指定訊息方塊的預設按鈕。</param>
		/// <returns>其中一個 System.Windows.Forms.DialogResult 值。</returns>
		/// <exception cref="System.ComponentModel.InvalidEnumArgumentException">
		/// 指定的 buttons 參數不是 System.Windows.Forms.MessageBoxButtons 的成員。
		/// <para>-或-指定的 icon 參數不是 System.Windows.Forms.MessageBoxIcon 的成員。</para>
		/// </exception>
		public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons button, MessageBoxIcon icon, MessageBoxDefaultButton defButton)
		{
			return MsgDialog.ShowBox(owner, text, caption, button, icon, defButton);
		}
		#endregion
	}
	#endregion
}
