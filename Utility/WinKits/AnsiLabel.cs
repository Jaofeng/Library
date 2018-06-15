using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CJF.Utility.Ansi;

namespace CJF.Utility.WinKits
{
	/// <summary>表示可支援 ANSI CSI Color Code 的標籤(Label)。</summary>
	[DefaultEvent("Click"), DefaultProperty("Text")]
	[Description("可支援 ANSI CSI Color Code 的標籤(Label)。")]
	[ClassInterface(ClassInterfaceType.AutoDispatch)]
	[ComVisible(true)]
	[DefaultBindingProperty("Text")]
	[Designer("System.Windows.Forms.Design.LabelDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ToolboxItem("System.Windows.Forms.Design.AutoSizeToolboxItem,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	[ToolboxItemFilter("System.Windows.Forms")]
	public class AnsiLabel : Control
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
		/// <summary>發生於 CJF.Utility.WinKits.AnsiViewer.BackgroundImage 屬性值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler BackgroundImageChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiViewer.BackgroundImageLayout 屬性值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler BackgroundImageLayoutChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiViewer.CausesValidation 屬性的值變更時。這個事件與這個類別無關。</summary>
		[EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		new public event EventHandler CausesValidationChanged;
		/// <summary>發生於 CJF.Utility.WinKits.AnsiViewer.RightToLeft 屬性的值變更時。這個事件與這個類別無關。</summary>
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
		/// <summary>發生於 CJF.Utility.WinKits.AnsiViewer.ImeMode 屬性的值變更時。這個事件與這個類別無關。</summary>
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
		/// <summary>引發 CJF.Utility.WinKits.AnsiViewer.CausesValidationChanged 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnCausesValidationChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiViewer.ImeModeChanged 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnImeModeChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiViewer.RightToLeftChanged 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnRightToLeftChanged(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiViewer.Validated 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnValidated(EventArgs e) { }
		/// <summary>引發 CJF.Utility.WinKits.AnsiViewer.Validating 事件。這個事件方法與這個類別無關。</summary>
		/// <param name="e">包含事件資料的 System.EventArgs。</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected override void OnValidating(CancelEventArgs e) { }
		#endregion
		#endregion

		#region 內部變數
		private IContainer components = null;
		StringFormat _DefFormat = new StringFormat(StringFormat.GenericTypographic);
		float _LineHeight = 0;
		PointF _LastLocation = PointF.Empty;
		#endregion

		#region Construct Method : AnsiLabel()
		/// <summary>以預設值初始化 CJF.Utility.WinKits.AnsiLabel 類別的新執行個體。</summary>
		public AnsiLabel()
		{
			InitializeComponent();
		}
		#endregion

		#region Public Override Property : bool AutoSize(R/W)
		/// <summary>取得或設定值，指出控制項是否自動調整大小以顯示其全部內容。</summary>
		[Browsable(true)]
		[EditorBrowsable(EditorBrowsableState.Always)]
		[RefreshProperties(RefreshProperties.All)]
		[Localizable(true)]
		[DefaultValue(true)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override bool AutoSize
		{
			get { return base.AutoSize; }
			set
			{
				if (base.AutoSize != value)
				{
					base.AutoSize = value;
					if (value)
						this.SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
					this.Refresh();
				}
			}
		}
		#endregion

		#region Public Override Property : string Text(R/W)
		/// <summary>取得或設定這個控制項的相關文字。</summary>
		[SettingsBindable(true)]
		[Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion

		#region Public Override Property : Font Font(R/W)
		/// <summary>取得或設定控制項顯示之文字字型。</summary>
		public override Font Font
		{
			get { return base.Font; }
			set
			{
				if (!base.Font.Equals(value))
				{
					base.Font = value;
					this.Refresh();
				}
			}
		}
		#endregion


		#region Public Property : BorderStyle BorderStyle(R/W)
		BorderStyle _BorderStyle = BorderStyle.None;
		/// <summary>取得或設定控制項的框線樣式。</summary>
		[DefaultValue(BorderStyle.None), Browsable(true)]
		[Description("控制項的框線樣式。"), Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get { return _BorderStyle; }
			set
			{
				if (_BorderStyle != value)
				{
					_BorderStyle = value;
					this.Refresh();
				}
			}
		}
		#endregion

		
		#region New Public Property : Padding Padding(R/W)
		Padding _Padding = new Padding(3);
		/// <summary>設定或取得文字與邊框間的指定間距 (以像素為單位)。</summary>
		[DefaultValue(typeof(Padding), "3, 3, 3, 3")]
		[Category("Layout")]
		[Localizable(true)]
		[Description("文字與邊框間的指定間距 (以像素為單位)。")]
		new public Padding Padding
		{
			get { return _Padding; }
			set
			{
				if (!_Padding.Equals(value))
				{
					_Padding = value;
					this.Refresh();
				}
			}
		}
		#endregion

		#region New Public Peoperty : Rectangle ClientRectangle(R)
		/// <summary>取得表示控制項文字工作區的矩形。</summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		new public Rectangle ClientRectangle
		{
			get
			{
				Rectangle rect = new Rectangle(Point.Empty, this.Size);
				rect.Offset(_Padding.Left, _Padding.Top);
				rect.Height -= _Padding.Vertical;
				rect.Width -= _Padding.Horizontal;
				return rect;
			}
		}
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


		#region Protected Override Property : Size DefaultSize(R)
		/// <summary>取得控制項的預設大小。</summary>
		[DefaultValue(typeof(Size), "300, 50")]
		protected override Size DefaultSize
		{
			get { return new Size(300, 50); }
		}
		#endregion

		#region Protected Override Property : Padding DefaultPadding(R)
		/// <summary>取得控制項內容的內部間距 (以像素為單位)。</summary>
		protected override Padding DefaultPadding
		{
			get { return new Padding(3); }
		}
		#endregion


		#region Protected Override Method : void OnPaint(PaintEventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiLabel.Paint 事件。</summary>
		/// <param name="e">包含事件資料的 System.Windows.Forms.PaintEventArgs。</param>
		protected override void OnPaint(PaintEventArgs e)
		{
			string text = this.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
			_LastLocation = DrawAnsiCsiSgrText(e.Graphics, text, new Point(_Padding.Left, _Padding.Top));

#if DEBUG
			e.Graphics.DrawRectangle(new Pen(Color.Red, 2), new Rectangle(Point.Round(_LastLocation), new Size(2, 2)));
#endif

			#region 畫框線
			switch (this.BorderStyle)
			{
				case BorderStyle.FixedSingle:
					{
						e.Graphics.DrawRectangle(new Pen(SystemBrushes.ControlDarkDark, 1), new Rectangle(0, 0, this.Width - 1, this.Height - 1));
						break;
					}
				case BorderStyle.Fixed3D:
					{
						e.Graphics.DrawLines(new Pen(SystemBrushes.ControlDark, 1), new Point[] { new Point(this.Width, 0), new Point(0, 0), new Point(0, this.Height) });
						e.Graphics.DrawLines(new Pen(SystemBrushes.ControlLight, 1), new Point[] { new Point(this.Width - 1, 0), new Point(this.Width - 1, this.Height - 1), new Point(0, this.Height - 1) });
						break;
					}
			}
			#endregion

			base.OnPaint(e);
		}
		#endregion

		#region Protected Override Method : void OnTextChanged(EventArgs e)
		/// <summary>引發 CJF.Utility.WinKits.AnsiLabel.TextChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.Windows.Forms.EventArgs。</param>
		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			this.Refresh();
		}
		#endregion

		#region Protected Override Method : void OnParentFontChanged(EventArgs e)
		/// <summary>當控制項容器的 System.Windows.Forms.Control.Font 屬性值變更時，會引發 CJF.Utility.WinKits.AnsiLabel.FontChanged 事件。</summary>
		/// <param name="e">包含事件資料的 System.Windows.Forms.EventArgs。</param>
		protected override void OnParentFontChanged(EventArgs e)
		{
			base.OnParentFontChanged(e);
			this.Refresh();
		}
		#endregion


		#region Public Override Method : void Refresh()
		/// <summary>強制控制項使其工作區失效，並且立即重繪其本身和任何子控制項。</summary>
		public override void Refresh()
		{
			using (Graphics grp = this.CreateGraphics())
			{
				Font fText = this.Font;
				SizeF tf = Size.Empty;
				_LineHeight = grp.MeasureString("\r", fText, Point.Empty, _DefFormat).Height;
				string text = this.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
				if (!this.AutoSize)
				{
					tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText, this.ClientRectangle.Width);
					this.MaximumSize = this.MinimumSize = Size.Empty;
				}
				else
				{
					tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText);
					this.MaximumSize = this.MinimumSize = new Size((int)tf.Width + _Padding.Horizontal, (int)tf.Height + _Padding.Horizontal);
					this.Size = new Size((int)tf.Width + _Padding.Horizontal, (int)tf.Height + _Padding.Horizontal);
				}
			}
			base.Refresh();
		}
		#endregion

		#region Protected Override Method : void Dispose(bool disposing)
		/// <summary> 清除任何使用中的資源。</summary>
		/// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				components = null;
			}
			base.Dispose(disposing);
		}
		#endregion


		#region Private Method : void DrawAllText(Graphics grp) - No used
		private void DrawAllText(Graphics grp)
		{
			Font fText = this.Font;
			SizeF tf = Size.Empty;
			_LineHeight = grp.MeasureString("\r", fText, Point.Empty, _DefFormat).Height;
			string text = this.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
			if (!this.AutoSize)
				tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText, this.ClientRectangle.Width);
			else
			{
				tf = grp.MeasureString(CsiBuilder.GetPureText(text), fText);
				this.Size = new Size((int)tf.Width + _Padding.Horizontal, (int)tf.Height + _Padding.Horizontal);
			}
			_LastLocation = DrawAnsiCsiSgrText(grp, text, new Point(_Padding.Left, _Padding.Top));
#if DEBUG
			grp.DrawRectangle(new Pen(Color.Red, 2), new Rectangle(Point.Round(_LastLocation), new Size(2, 2)));
#endif
		}
		#endregion

		#region Private Method : PointF DrawAnsiCsiSgrText(Graphics grp, string text, PointF loc)
		private PointF DrawAnsiCsiSgrText(Graphics grp, string text, PointF loc)
		{
			Rectangle rect = this.ClientRectangle;
			Font fText = this.Font;
			Brush bFore = new SolidBrush(this.ForeColor);
			Brush bBack = new SolidBrush(this.BackColor);
			text = Regex.Replace(text, "\x1B\\[(\\d+)*;?(\\d+)?([ABCDEFGHJKSTfsu])", "");
			MatchCollection mc = Regex.Matches(text, "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?m", RegexOptions.Multiline);
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
			//    this.Height += (int)(loc.Y + lineHeight - rect.Bottom);
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
			Point leftTop = new Point(_Padding.Left, _Padding.Top);
			Rectangle rect = this.ClientRectangle;
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

		#region Private Method : Color Get8bitsColor(int v)
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

		#region Private Method : void InitializeComponent()
		private void InitializeComponent()
		{
			components = new Container();
			this.Size = this.DefaultSize;
			this.AutoSize = true;
			this.DoubleBuffered = true;
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
			_DefFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
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
