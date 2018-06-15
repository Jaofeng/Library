using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Data;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CJF.Utility.Ansi;

namespace CJF.Utility.WinKits
{
	/// <summary>表示可支援顯示 ANSI CSI Color Code 的可捲式控制項。</summary>
	[DefaultEvent("Click"), DefaultProperty("Text")]
	[Description("可支援顯示 ANSI CSI Color Code 的可捲式控制項。")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	[DefaultBindingProperty("Text")]
	[Designer("System.Windows.Forms.Design.LabelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ToolboxItemFilter("System.Windows.Forms")]
	public class AnsiBox : Control
	{
		#region 隱藏的父屬性、事件與方法
		#region 屬性
		/// <summary>取得或設定在控制項中顯示的背景影像。這個屬性與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override Image BackgroundImage { get; set; }
		/// <summary>取得或設定 System.Windows.Forms.ImageLayout 列舉型別 (Enumeration) 所定義的背景影像配置。這個屬性與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override ImageLayout BackgroundImageLayout { get; set; }
		/// <summary> 取得或設定值，指出控制項的元素是否對齊，以支援使用由右至左字型的地區設定。這個屬性與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override RightToLeft RightToLeft { get; set; }
		/// <summary> 取得或設定值，指出控制項取得焦點時，是否會在任何需要驗證的控制項上執行驗證。這個屬性與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[DefaultValue(false)]
		new public bool CausesValidation { get; set; }
		/// <summary> 取得或設定控制項的輸入法 (IME) 模式。這個屬性與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public ImeMode ImeMode { get; set; }
		#endregion

		#region 事件
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.BackgroundImage 屬性值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler BackgroundImageChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.BackgroundImageLayout 屬性值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler BackgroundImageLayoutChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.CausesValidation 屬性的值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler CausesValidationChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.RightToLeft 屬性的值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler RightToLeftChanged;
		/// <summary>發生於控制項完成驗證時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler Validated;
		/// <summary>發生於控制項進行驗證時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event CancelEventHandler Validating;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.ImeMode 屬性的值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler ImeModeChanged;
		#endregion

		#region 事件方法
		/// <summary>這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnBackgroundImageChanged(EventArgs e) { }
		/// <summary>這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnBackgroundImageLayoutChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.CausesValidationChanged 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnCausesValidationChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.ImeModeChanged 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnImeModeChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.RightToLeftChanged 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnRightToLeftChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.Validated 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnValidated(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.Validating 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnValidating(CancelEventArgs e) { }
		#endregion
		#endregion

		#region 擴充事件
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.BorderStyle 屬性值變更時。</summary>
		public event EventHandler BorderStyleChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiBox.ScrollBars 屬性值變更時。</summary>
		public event EventHandler ScrollBarsChanged;
		#endregion

		#region 內部控制項元件
		private IContainer components = null;
		private VScrollBar vsBar = null;
		private HScrollBar hsBar = null;
		private Panel pWin = null;
		private PictureBox pPaper = null;
		private Label labRD = null;
		#endregion

		#region 內部變數
		StringFormat _DefFormat = new StringFormat(StringFormat.GenericTypographic);
		float _LineHeight = 0;
		PointF _LastLocation = PointF.Empty;
		#endregion

		#region Construct Method : AnsiBox()
		/// <summary>以預設值初始化 CJF.Utility.WinKits.AnsiBox 類別的新執行個體。</summary>
		public AnsiBox()
		{
			InitializeComponent();
			PropertyInfo info = this.pPaper.GetType().GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
			info.SetValue(this.pPaper, true, null);
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			this.UpdateStyles();
			ArrangeControls();
			_DefFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
		}
		#endregion

		#region Default Property Values
		/// <summary>取得控制項的預設大小。</summary>
		protected override Size DefaultSize { get { return new Size(300, 100); } }
		/// <summary>取得控制項內容的內部間距 (以像素為單位)。</summary>
		protected override Padding DefaultPadding { get { return new Padding(3); } }
		#endregion


		#region Public Property : BorderStyle BorderStyle(R/W)
		BorderStyle _BorderStyle = BorderStyle.Fixed3D;
		/// <summary>取得或設定控制項的框線樣式。</summary>
		[DefaultValue(BorderStyle.Fixed3D)]
		[Description("控制項的框線樣式。"), Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get { return _BorderStyle; }
			set
			{
				if (_BorderStyle != value)
				{
					_BorderStyle = value;
					OnBorderStyleChanged(new EventArgs());
				}
			}
		}
		#endregion

		#region Public Property : ScrollBars ScrollBars(R/W)
		ScrollBars _ScrollBars = ScrollBars.Both;
		/// <summary>取得或設定在控制項中應該顯示的捲軸。</summary>
		[DefaultValue(ScrollBars.Both), Localizable(true)]
		[Description("控制項中應該顯示的捲軸。"), Category("Appearance")]
		public ScrollBars ScrollBars
		{
			get { return _ScrollBars; }
			set
			{
				if (_ScrollBars != value)
				{
					_ScrollBars = value;
					OnScrollBarsChanged(new EventArgs());
				}
			}
		}
		#endregion

		#region New Public Property : Padding Padding(R/W)
		/// <summary>取得或設定控制項內的邊框距離。</summary>
		[DefaultValue(typeof(Padding), "3, 3, 3, 3")]
		new public Padding Padding
		{
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		#endregion

		#region New Public Property : Size PreferredSize(R)
		/// <summary>取得能夠容納控制項的矩形區域的大小。</summary>
		[Browsable(false)]
		new public Size PreferredSize { get { return this.pWin.Size; } }
		#endregion

		#region New Public Property : string ProductName(R)
		/// <summary>取得包含控制項的組件的產品名稱。</summary>
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public string ProductName { get { return this.GetType().ToString(); } }
		#endregion

		#region New Public Property : string ProductVersion(R)
		/// <summary>取得包含控制項的組件的版本。</summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		new public string ProductVersion
		{
			get
			{
				object[] atts = new System.Diagnostics.StackFrame().GetMethod().DeclaringType.Assembly.GetCustomAttributes(false);
				foreach (object att in atts)
				{
					if (att.GetType().Equals(typeof(System.Reflection.AssemblyFileVersionAttribute)))
						return ((System.Reflection.AssemblyFileVersionAttribute)att).Version;
				}
				return null;
			}
		}
		#endregion


		#region Public Override Property : Color BackColor(R/W)
		/// <summary>取得或設定控制項的背景色彩。</summary>
		[DefaultValue(typeof(Color), "Window")]
		public override Color BackColor
		{
			get { return base.BackColor; }
			set { base.BackColor = value; }
		}
		#endregion

		#region Public Override Property : Font Font(R/W)
		/// <summary>取得或設定控制項顯示之文字字型。</summary>
		public override Font Font
		{
			get { return base.Font; }
			set { base.Font = value; }
		}
		#endregion

		#region Public Override Property : string Text(R/W)
		/// <summary>取得或設定這個控制項的相關文字。</summary>
		[Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion



		#region Protected Virtual Method : void OnBorderStyleChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.BorderStyleChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected virtual void OnBorderStyleChanged(EventArgs e)
		{
			ArrangeControls();
			if (this.BorderStyleChanged != null)
				this.BorderStyleChanged.Invoke(this, e);
		}
		#endregion

		#region Protected Virtual Method : void OnScrollBarsChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.ScrollBarsChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected virtual void OnScrollBarsChanged(EventArgs e)
		{
			ArrangeControls();
			this.Refresh();
			if (this.ScrollBarsChanged != null)
				this.ScrollBarsChanged.Invoke(this, e);
		}
		#endregion

		#region Protected Override Method : void OnBackColorChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.BackColorChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected override void OnBackColorChanged(EventArgs e)
		{
			if (this.pWin != null && this.pPaper != null)
				this.pWin.BackColor = this.pPaper.BackColor = this.BackColor;
			base.OnBackColorChanged(e);
		}
		#endregion

		#region Protected Override Method : void OnFontChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.FontChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected override void OnFontChanged(EventArgs e)
		{
			if (_DefFormat != null && this.vsBar != null && this.pPaper != null)
			{
				this.vsBar.SmallChange = this.pPaper.CreateGraphics().MeasureString("\r", this.Font, PointF.Empty, _DefFormat).ToSize().Height;
				this.Refresh();
			}
			base.OnFontChanged(e);
		}
		#endregion

		#region Protected Override Method : void OnPaint(PaintEventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.Paint 事件。</summary>
		/// <param name="e">包含事件資料的 System.PaintEventArgs。</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			#region 畫框線
			Graphics g = e.Graphics;
			switch (_BorderStyle)
			{
				case BorderStyle.FixedSingle:
					{
						g.DrawRectangle(new Pen(SystemBrushes.ControlDarkDark, 1), new Rectangle(0, 0, this.Width - 1, this.Height - 1));
						break;
					}
				case BorderStyle.Fixed3D:
					{
						g.DrawLines(new Pen(SystemBrushes.ControlDark, 1), new Point[] { new Point(this.Width, 0), new Point(0, 0), new Point(0, this.Height) });
						g.DrawLines(new Pen(SystemBrushes.ControlLight, 1), new Point[] { new Point(this.Width - 1, 0), new Point(this.Width - 1, this.Height - 1), new Point(0, this.Height - 1) });
						break;
					}
			}
			#endregion

			base.OnPaint(e);
		}
		#endregion

		#region Protected Override Method : void OnPaddingChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiBox.PaddingChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		protected override void OnPaddingChanged(EventArgs e)
		{
			if (this.pPaper != null)
				this.pPaper.Refresh();
			base.OnPaddingChanged(e);
		}
		#endregion

		#region Protected Override Method : void OnResize(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiLabel.Resize 事件。</summary>
		/// <param name="e">包含事件資料的 System.Windows.Forms.EventArgs。</param>
		protected override void OnResize(EventArgs e)
		{
			ArrangeControls();
			base.OnResize(e);
		}
		#endregion

		#region Protected Override Method : void OnTextChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiLabel.TextChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.Windows.Forms.EventArgs。</param>
		protected override void OnTextChanged(EventArgs e)
		{
			this.Refresh();
			base.OnTextChanged(e);
		}
		#endregion

		#region Public Override Method : void Refresh()
		/// <summary>強制控制項使其工作區失效，並且立即重繪其本身和任何子控制項。</summary>
		public override void Refresh()
		{
			Font fText = this.Font;
			SizeF tf = Size.Empty;
			string text = this.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
			using (Graphics grp = this.pPaper.CreateGraphics())
			{
				_LineHeight = grp.MeasureString("\r", fText, Point.Empty, _DefFormat).Height;
				if (_ScrollBars == ScrollBars.None || _ScrollBars == ScrollBars.Vertical)
					tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText, this.pWin.Width - this.Padding.Horizontal);
				else
					tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText);
			}
			this.pPaper.Size = new Size((int)tf.Width + this.Padding.Horizontal, (int)tf.Height + this.Padding.Horizontal + (int)_LineHeight);
			if (!string.IsNullOrEmpty(text))
			{
				text = text.Replace("\n", "\r").Replace("\r\r", "\r");
				int idx = text.Length - 1;
				while (text.Substring(idx, 1).Equals("\r"))
					idx--;
				if (idx != text.Length - 1)
					this.pPaper.Height += (int)_LineHeight * (text.Length - 1 - idx);
			}
			this.pPaper.Refresh();
			base.Refresh();
		}
		#endregion

		#region Protected Override Method : void Dispose(bool disposing)
		/// <summary> 清除任何使用中的資源。</summary>
		/// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				this.vsBar.Dispose();
				this.vsBar = null;
				this.hsBar.Dispose();
				this.hsBar = null;
				this.pPaper.Dispose();
				this.pPaper = null;
				this.pWin.Dispose();
				this.pWin = null;
				this.Controls.Clear();
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion



		#region Private Method : void ArrangeControls()
		private void ArrangeControls()
		{
			this.pWin.Size = this.ClientRectangle.Size;
			this.pWin.Location = this.ClientRectangle.Location;
			if (_BorderStyle != BorderStyle.None)
			{
				this.pWin.Location = new Point(1, 1);
				this.pWin.Height -= 2;
				this.pWin.Width -= 2;
			}

			#region ScrollBars
			switch (_ScrollBars)
			{
				case ScrollBars.Vertical:
					this.pWin.Width -= this.vsBar.Width;
					this.vsBar.Height = this.pWin.Height;
					this.vsBar.Location = new Point(this.pWin.Right, this.pWin.Top);
					this.vsBar.Visible = true;
					this.hsBar.Visible = this.labRD.Visible = false;
					break;
				case ScrollBars.Horizontal:
					this.pWin.Height -= this.hsBar.Height;
					this.hsBar.Width = this.pWin.Width;
					this.hsBar.Location = new Point(this.pWin.Left, this.pWin.Bottom);
					this.hsBar.Visible = true;
					this.vsBar.Visible = this.labRD.Visible = false;
					break;
				case ScrollBars.Both:
					this.pWin.Width -= this.vsBar.Width;
					this.pWin.Height -= this.hsBar.Height;
					this.vsBar.Height = this.pWin.Height;
					this.vsBar.Location = new Point(this.pWin.Right, this.pWin.Top);
					this.hsBar.Width = this.pWin.Width;
					this.hsBar.Location = new Point(this.pWin.Left, this.pWin.Bottom);
					this.labRD.Location = new Point(this.pWin.Right, this.pWin.Bottom);
					this.labRD.Size = new Size(this.vsBar.Width, this.hsBar.Height);
					this.vsBar.Visible = this.hsBar.Visible = this.labRD.Visible = true;
					break;
				default:
					this.vsBar.Visible = this.hsBar.Visible = this.labRD.Visible = false;
					break;
			}
			#endregion
		}
		#endregion

		#region Private Method : void DrawAllText() - No used
		private void DrawAllText()
		{
			Font fText = this.Font;
			SizeF tf = Size.Empty;
			Graphics grp = this.pPaper.CreateGraphics();
			_LineHeight = grp.MeasureString("\r", fText, Point.Empty, _DefFormat).Height;
			string text = this.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
			if (_ScrollBars == ScrollBars.Vertical || _ScrollBars == ScrollBars.None)
				tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText, this.pWin.Width - this.Padding.Horizontal);
			else
				tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText);
			this.pPaper.Size = new Size((int)tf.Width + this.Padding.Horizontal, (int)tf.Height + this.Padding.Horizontal + (int)_LineHeight);

			_LastLocation = DrawAnsiCsiSgrText(grp, text, new Point(this.Padding.Left, this.Padding.Top));
#if DEBUG
			using (Pen p = new Pen(Color.Red, 2))
				grp.DrawRectangle(p, new Rectangle(Point.Round(_LastLocation), new Size(2, 2)));
#endif
		}
		#endregion

		#region Private Method : void DrawNewText(string text) - No used
		private void DrawNewText(string text)
		{
			Font fText = this.Font;
			SizeF tf = Size.Empty;
			Graphics grp = this.pPaper.CreateGraphics();
			Rectangle rect = this.pPaper.ClientRectangle;
			rect.Offset(this.Padding.Left, this.Padding.Top);
			rect.Height -= this.Padding.Vertical;
			rect.Width -= this.Padding.Horizontal;

			text = text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
			if (_ScrollBars == ScrollBars.Vertical || _ScrollBars == ScrollBars.None)
				tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText, this.pWin.Width - this.Padding.Horizontal);
			else
				tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText);
			if (this.pPaper.Width < tf.Width + this.Padding.Horizontal)
				this.pPaper.Width = (int)(tf.Width + this.Padding.Horizontal);
			if (_LastLocation.Y + tf.Height > rect.Bottom)
				this.pPaper.Height += (int)(_LastLocation.Y + tf.Height - rect.Bottom);

			_LastLocation = DrawAnsiCsiSgrText(grp, text, _LastLocation);

#if DEBUG
			using (Pen p = new Pen(Color.Red, 2))
				grp.DrawRectangle(p, new Rectangle(Point.Round(_LastLocation), new Size(2, 2)));
#endif
		}
		#endregion

		#region Private Method : PointF DrawAnsiCsiSgrText(Graphics grp, string text, PointF loc)
		private PointF DrawAnsiCsiSgrText(Graphics grp, string text, PointF loc)
		{
			Font fText = this.Font;
			Brush bFore = new SolidBrush(this.ForeColor);
			Brush bBack = new SolidBrush(this.BackColor);
			text = Regex.Replace(text, "\x1B\\[(\\d+)*;?(\\d+)?([ABCDEFGHJKSTfsu])", "");
			MatchCollection mc = Regex.Matches(text, "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?m", RegexOptions.Multiline);
			//PointF loc = leftTop;
			int idx = 0;
			SizeF f1 = SizeF.Empty, f2 = SizeF.Empty, f3 = SizeF.Empty;
			string txt = string.Empty, tmp = string.Empty, st = string.Empty;
			SizeF lastSize = SizeF.Empty;
			List<string> line = new List<string>();
			float lineHeight = grp.MeasureString("\r", fText, loc, _DefFormat).Height;
			bool colorReverse = false;
			foreach (Match m in mc)
			{
				if (!m.Success) continue;
				if (idx != m.Index)
				{
					txt = text.Substring(idx, m.Index - idx);
					loc = DrawWrapCsiSgrText(grp, txt, fText, bFore, bBack, loc);
				}

				#region 設定顏色與字型
				ushort v = 0;
				for (int i = 1; i < m.Groups.Count; i++)
				{
					if (!m.Groups[i].Success)
						continue;
					v = Convert.ToUInt16(m.Groups[i].Value);
					if (v >= 30 && v <= 37 || v >= 90 && v <= 97)
						bFore = new SolidBrush(((SgrColors)v).ToColor());			// 3/4 位元前景色
					else if (v >= 40 && v <= 47 || v >= 100 && v <= 107)
						bBack = new SolidBrush(((SgrColors)(v - 10)).ToColor());	// 3/4 位元背景色
					else
					{
						#region 其餘 SGR 參數
						switch (v)
						{
							case 0:		// 重設/正常
								bFore = new SolidBrush(this.ForeColor);
								bBack = new SolidBrush(this.BackColor);
								fText = this.Font;
								colorReverse = false;
								break;
							case 1:		// 粗體
								fText = new Font(fText.FontFamily, fText.Size, fText.Style | FontStyle.Bold);
								break;
							case 4:		// 底線
								fText = new Font(fText.FontFamily, fText.Size, fText.Style | FontStyle.Underline);
								break;
							case 7:		// 前景與背景色互換
								if (!colorReverse)
								{
									colorReverse = true;
									Brush b = bFore;
									bFore = bBack;
									bBack = b;
								}
								break;
							case 9:		// 刪除線
								fText = new Font(fText.FontFamily, fText.Size, fText.Style | FontStyle.Strikeout);
								break;
							case 21:	// 關閉粗體
								fText = new Font(fText.FontFamily, fText.Size, fText.Style ^ FontStyle.Bold);
								break;
							case 22:	// 正常字型
								fText = new Font(fText.FontFamily, fText.Size);
								break;
							case 24:	// 關閉底線
								fText = new Font(fText.FontFamily, fText.Size, fText.Style ^ FontStyle.Underline);
								break;
							case 29:	// 關閉刪除線
								fText = new Font(fText.FontFamily, fText.Size, fText.Style ^ FontStyle.Strikeout);
								break;
							case 27:
								if (colorReverse)
								{
									colorReverse = false;
									Brush b = bFore;
									bFore = bBack;
									bBack = b;
								}
								break;
							case 38:	// 8 位元前景色
								if (i + 1 >= m.Groups.Count)
									break;	// 錯誤格式，忽略
								v = Convert.ToUInt16(m.Groups[i + 1].Value);
								if (v == 5)
								{
									// 8-bits Colors
									if (i + 2 >= m.Groups.Count)
										break;	// 錯誤格式，忽略
									v = Convert.ToUInt16(m.Groups[i + 2].Value);
									if (v < 0 || v > 255)
										break;	// 顏色值錯誤，忽略
									bFore = new SolidBrush(Get8bitsColor(v));
									i += 2;
								}
								else if (v == 2)
								{
									// 24-bits Colors
									if (i + 4 >= m.Groups.Count)
										break;	// 錯誤格式，忽略
									bFore = new SolidBrush(Color.FromArgb(Convert.ToUInt16(m.Groups[i + 2].Value), Convert.ToUInt16(m.Groups[i + 3].Value), Convert.ToUInt16(m.Groups[i + 4].Value)));
									i += 4;
								}
								break;
							case 39:	// 還原預設前景色
								bFore = new SolidBrush(this.ForeColor);
								break;
							case 48:	// 8 位元背景色
								if (i + 1 >= m.Groups.Count)
									break;	// 錯誤格式，忽略
								v = Convert.ToUInt16(m.Groups[i + 1].Value);
								if (v == 5)
								{
									// 8-bits Colors
									if (i + 2 >= m.Groups.Count)
										break;	// 錯誤格式，忽略
									v = Convert.ToUInt16(m.Groups[i + 2].Value);
									if (v < 0 || v > 255)
										break;	// 顏色值錯誤，忽略
									bBack = new SolidBrush(Get8bitsColor(v));
									i += 2;
								}
								else if (v == 2)
								{
									// 24-bits Colors
									if (i + 4 >= m.Groups.Count)
										break;	// 錯誤格式，忽略
									bBack = new SolidBrush(Color.FromArgb(Convert.ToUInt16(m.Groups[i + 2].Value), Convert.ToUInt16(m.Groups[i + 3].Value), Convert.ToUInt16(m.Groups[i + 4].Value)));
									i += 4;
								}
								break;
							case 49:	// 還原預設背景色
								bBack = new SolidBrush(this.BackColor);
								break;
							default:
								break;
						}
						#endregion
					}
				}
				#endregion

				// 偏移位置
				idx = m.Index + m.Length;
			}
			if (idx < text.Length)
			{
				txt = text.Substring(idx, text.Length - idx);
				loc = DrawWrapCsiSgrText(grp, txt, fText, bFore, bBack, loc);
			}

			//if (loc.Y + lineHeight > rect.Bottom)
			//    this.pbContent.Height += (int)(loc.Y + lineHeight - rect.Bottom);

			bFore.Dispose();
			bFore = null;
			bBack.Dispose();
			bBack = null;

			return loc;
		}
		#endregion

		#region Private Method : PointF DrawWrapCsiSgrText(Graphics g, string txt, Font font, Brush bFore, Brush bBack, PointF loc)
		private PointF DrawWrapCsiSgrText(Graphics g, string txt, Font font, Brush bFore, Brush bBack, PointF loc)
		{
			int idx = 0;
			SizeF f1, f2;
			string tmp, st = string.Empty;
			MatchCollection mc;
			RectangleF tRect;
			Point leftTop = new Point(this.Padding.Left, this.Padding.Top);
			Rectangle rect = this.pPaper.ClientRectangle;
			rect.Offset(leftTop);
			rect.Height -= this.Padding.Vertical;
			rect.Width -= this.Padding.Horizontal;
			PointF backOffset = new PointF(0, 0);
			string[] lines = null;
			float lineHeight = g.MeasureString("\r", font, loc, _DefFormat).Height;

			txt = txt.Replace("\n", "\r").Replace("\r\r", "\r");

			#region 處理行首的換行符號
			while (txt.StartsWith("\r"))
			{
				loc.X = leftTop.X;
				loc.Y += lineHeight;
				txt = txt.Substring("\r".Length);
			}
			#endregion

			if (txt.IndexOf("\r") != -1)
			{
				#region 處理多行文字
				lines = txt.Split('\r');
				for (int i = 0; i < lines.Length; i++)
				{
					if (string.IsNullOrEmpty(lines[i]))
					{
						loc.X = leftTop.X;
						loc.Y += lineHeight;
					}
					else
					{
						loc = DrawWrapCsiSgrText(g, lines[i], font, bFore, bBack, loc);
						if (i < lines.Length - 1)	//  && !string.IsNullOrEmpty(lines[i + 1])
						{
							loc.X = leftTop.X;
							loc.Y += lineHeight;
						}
					}
				}
				#endregion
			}
			else
			{
				#region 處理單行文字
				f1 = g.MeasureString(txt, font, loc, _DefFormat);
				if (f1.Width + loc.X < rect.Right)
				{
					#region 文字需要的大小，小於等於剩餘空間
					tRect = new RectangleF(loc, f1);
					tRect.Offset(backOffset);
					g.FillRectangle(bBack, tRect);
					g.DrawString(txt, font, bFore, loc, _DefFormat);
					loc.X += f1.Width;
					#endregion
				}
				else
				{
					#region 文字將會超過剩餘可用空間
					tmp = string.Empty;
					idx = 0;
					mc = Regex.Matches(txt, "\\W");
					if (mc.Count != 0)
					{
						#region 處理含有符號的字串
						foreach (Match mm in mc)
						{
							#region 處理符號前字串
							if (idx != mm.Index)
							{
								st = txt.Substring(idx, mm.Index - idx);
								f2 = g.MeasureString(tmp + st, font, PointF.Empty, _DefFormat);
								if (f2.Width + loc.X > rect.Right)
								{
									// 加上字串會超過右限，先繪製之前的字串
									f2 = g.MeasureString(tmp, font, PointF.Empty, _DefFormat);
									tRect = new RectangleF(loc, f2);
									tRect.Offset(backOffset);
									g.FillRectangle(bBack, tRect);
									g.DrawString(tmp, font, bFore, loc, _DefFormat);
									loc.X = leftTop.X;
									loc.Y += lineHeight;
									tmp = st;
								}
								else
									tmp += st;
								idx += st.Length;
							}
							#endregion

							#region 檢查加上符號時，是否會超過範圍
							f2 = g.MeasureString(tmp + mm.Value, font, PointF.Empty, _DefFormat);
							if (f2.Width + loc.X > rect.Right)
							{
								// 加上字串會超過右限，先繪製之前的字串
								f2 = g.MeasureString(tmp, font, PointF.Empty, _DefFormat);
								tRect = new RectangleF(loc, f2);
								tRect.Offset(backOffset);
								g.FillRectangle(bBack, tRect);
								g.DrawString(tmp, font, bFore, loc, _DefFormat);
								loc.X = leftTop.X;
								loc.Y += lineHeight;
								if (!mm.Value.Equals(" "))
									tmp = mm.Value;
								else
									tmp = string.Empty;
							}
							else
								tmp += mm.Value;
							idx += mm.Length;
							#endregion
						}
						if (!string.IsNullOrEmpty(tmp))
							loc = DrawWrapCsiSgrText(g, tmp, font, bFore, bBack, loc);

						#region 剩下的字串
						if (idx < txt.Length)
						{
							st = txt.Substring(idx, txt.Length - idx);
							loc = DrawWrapCsiSgrText(g, st, font, bFore, bBack, loc);
						}
						#endregion
						#endregion
					}
					else
					{
						#region 處理不含符號的字串
						for (int i = 0; i < txt.Length; i++)
						{
							f1 = g.MeasureString(txt.Substring(idx, i + 1 - idx), font, loc, _DefFormat);
							if (f1.Width + loc.X > rect.Right)
							{
								f1 = g.MeasureString(txt.Substring(idx, i - idx), font, loc, _DefFormat);
								tRect = new RectangleF(loc, f1);
								tRect.Offset(backOffset);
								g.FillRectangle(bBack, tRect);
								g.DrawString(txt.Substring(idx, i - idx), font, bFore, loc, _DefFormat);
								loc.X = leftTop.X;
								loc.Y += lineHeight;
								idx = i;
							}
						}
						if (idx != txt.Length - 1)
						{
							f1 = g.MeasureString(txt.Substring(idx), font, loc, _DefFormat);
							tRect = new RectangleF(loc, f1);
							tRect.Offset(backOffset);
							g.FillRectangle(bBack, tRect);
							g.DrawString(txt.Substring(idx), font, bFore, loc, _DefFormat);
							loc.X += g.MeasureString(txt.Substring(idx), font, loc, _DefFormat).Width;
						}
						#endregion
					}
					#endregion
				}
				#endregion
			}
			return loc;
		}
		#endregion

		#region Private Color : Color Get8bitsColor(int v)
		private Color Get8bitsColor(int v)
		{
			if (v >= 0 && v <= 15)
				return ((SgrColors)(v % 8 + 30 + (v / 8 * 60))).ToColor();
			else if (v >= 16 && v <= 231)
			{
				byte[] steps = { 0x00, 0x5F, 0x87, 0xAF, 0xD7, 0xFF };
				int r = (v - 16) / 36;
				int g = (v - 16 - r * 36) / 6;
				int b = v - 16 - r * 36 - g * 6;
				return Color.FromArgb(r * 0x28 + (r != 0 ? 0x37 : 0), g * 0x28 + (g != 0 ? 0x37 : 0), b * 0x28 + (b != 0 ? 0x37 : 0));
			}
			else if (v >= 232 && v <= 255)
			{
				int c = (v - 232) * 10 + 0x08;
				return Color.FromArgb(c, c, c);
			}
			else
				return Color.Empty;
		}
		#endregion


		#region Private Method : void pPaper_Paint(object sender, PaintEventArgs e)
		private void pPaper_Paint(object sender, PaintEventArgs e)
		{
			string text = this.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
			e.Graphics.Clear(this.BackColor);
			_LastLocation = DrawAnsiCsiSgrText(e.Graphics, text, new Point(this.Padding.Left, this.Padding.Top));
#if DEBUG
			using (Pen p = new Pen(Color.Red, 2))
				e.Graphics.DrawRectangle(p, new Rectangle(Point.Round(_LastLocation), new Size(2, 2)));
#endif
		}
		#endregion

		#region Private Method : void pPaper_Resize(object sender, EventArgs e)
		private void pPaper_Resize(object sender, EventArgs e)
		{
			if (this.pPaper.Height > this.pWin.ClientSize.Height)
			{
				vsBar.Maximum = (int)(((float)this.pPaper.Height - this.pWin.ClientSize.Height) / _LineHeight);
				vsBar.Maximum += this.vsBar.LargeChange - this.vsBar.Maximum % this.vsBar.LargeChange;
			}
			else
				vsBar.Maximum = 0;
			if (this.pPaper.Width > this.pWin.ClientSize.Width)
				hsBar.Maximum = this.pPaper.Width - this.pWin.ClientSize.Width;
			else
				hsBar.Maximum = 0;
		}
		#endregion

		#region Private Method : void hsBar_ValueChanged(object sender, EventArgs e)
		private void hsBar_ValueChanged(object sender, EventArgs e)
		{
			this.pPaper.Left = -this.hsBar.Value;
		}
		#endregion

		#region Private Method : void vsBar_ValueChanged(object sender, EventArgs e)
		private void vsBar_ValueChanged(object sender, EventArgs e)
		{
			this.pPaper.Top = (int)(-this.vsBar.Value * _LineHeight);
		}
		#endregion

		#region Private Method : void InitializeComponent
		private void InitializeComponent()
		{
			components = new Container();
			this.BackColor = SystemColors.Window;
			// PictureBox 
			this.pPaper = new PictureBox();
			this.pPaper.Paint += new PaintEventHandler(pPaper_Paint);
			this.pPaper.Resize += new EventHandler(pPaper_Resize);
			// Canvas Panel
			this.pWin = new Panel();
			this.pWin.Controls.Add(this.pPaper);
			// Label
			this.labRD = new Label();
			this.labRD.BackColor = SystemColors.Control;
			this.labRD.AutoSize = false;
			this.labRD.Visible = false;
			// VScroll Bar
			this.vsBar = new VScrollBar();
			this.vsBar.Visible = false;
			this.vsBar.Location = new Point(this.ClientRectangle.Width - this.vsBar.Width, 0);
			this.vsBar.Height = this.ClientRectangle.Height - 2;
			this.vsBar.Maximum = 0;
			this.vsBar.SmallChange = 1;
			this.vsBar.LargeChange = 1;
			this.vsBar.ValueChanged += new EventHandler(vsBar_ValueChanged);
			// HScroll Bar
			this.hsBar = new HScrollBar();
			this.hsBar.Visible = false;
			this.hsBar.Location = new Point(0, this.ClientRectangle.Bottom - this.hsBar.Height);
			this.hsBar.Width = this.ClientRectangle.Width - 2;
			this.hsBar.Maximum = 0;
			this.hsBar.ValueChanged += new EventHandler(hsBar_ValueChanged);

			this.Controls.Add(vsBar);
			this.Controls.Add(hsBar);
			this.Controls.Add(pWin);
			this.Controls.Add(labRD);
		}
		#endregion


		#region Public Method : void ScrollToStart()
		/// <summary>捲動至內文一開始的地方。</summary>
		public void ScrollToStart()
		{
			this.vsBar.Value = 0;
		}
		#endregion

		#region Public Method : void ScrollToEnd()
		/// <summary>捲動至內文最後面的地方。</summary>
		public void ScrollToEnd()
		{
			this.vsBar.Value = this.vsBar.Maximum;
		}
		#endregion

		#region Public Method : string Append(string text)
		/// <summary>將指定字串的複本附加至這個這個執行個體 Text 屬性的尾端。</summary>
		/// <param name="text">要附加的字串。</param>
		/// <returns>附加後的字串內容。</returns>
		public string Append(string text)
		{
			this.Text += text;
			return this.Text;
		}
		#endregion

		#region Public Method : string AppendLine(string text)
		/// <summary>將後面接著預設行結束字元的指定字串複本附加至這個執行個體 Text 屬性的尾端。</summary>
		/// <param name="text">要附加的字串。</param>
		/// <returns>附加後的字串內容。</returns>
		public string AppendLine(string text)
		{
			this.Text += text + Environment.NewLine;
			return this.Text;
		}
		#endregion

		#region Public Method : string AppendFormat(string format, params object[] args)
		/// <summary>將處理複合格式字串所傳回的字串附加到這個執行個體 Text 屬性的尾端，該字串包含零個或多個格式項目。每一個格式項目都會取代為參數陣列中對應引數的字串表示。</summary>
		/// <param name="format">複合格式字串。</param>
		/// <param name="args">要格式化的物件陣列。</param>
		/// <returns>附加後的字串內容。</returns>
		public string AppendFormat(string format, params object[] args)
		{
			this.Text += string.Format(format, args);
			return this.Text;
		}
		#endregion
	}
}
