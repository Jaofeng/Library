using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CJF.Utility.Ansi;

namespace CJF.Utility.WinKits
{
	#region Private Class : MsgDialog
	class MsgDialog : Form
	{
		#region DLL Import
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
		private System.Windows.Forms.FlowLayoutPanel flpButtons;
		#endregion

		#region Internal Static Variables
		/// <summary>視窗最大範圍</summary>
		internal static int DialogMaxSize = 0;
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
		string _SgrText = string.Empty;
		string _PureText = string.Empty;

		MessageBoxIcon _MsgIcon = MessageBoxIcon.None;
		Size _TextSize = Size.Empty;
		bool _HasCsiSgr = false;
		int _MaxTextAreaWidth = 0;
		Icon _Icon = null;
		Size _CanvasArea = Size.Empty;
		Size _ButtonArea = Size.Empty;
		RectangleF _TextArea = RectangleF.Empty;
		StringFormat _DefFormat = new StringFormat(StringFormat.GenericTypographic);
		#endregion

		private readonly Padding _Padding = new Padding(25, 30, 25, 30);
		private const float MAX_WIDTH_PERCENTAGE = 0.33F;

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

			this.DialogResult = DialogResult.None;
			this.Text = caption;
			if (TextFont != null)
				this.Font = TextFont;
			else
				this.Font = SystemFonts.MessageBoxFont;
			this.Size = Size.Empty;

			SetButtons(button, defButton);
			_DefFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
			Graphics g = this.CreateGraphics();
			Rectangle sr = Screen.GetWorkingArea(this);

			_Message = text;
			_MsgIcon = icon;
			_SgrText = Regex.Replace(_Message, "\x1B\\[(\\d+)*;?(\\d+)?([ABCDEFGHJKSTfsu])", "");			// 移除非 SGR 指令
			_PureText = CsiBuilder.GetPureText(_Message);
			_Icon = GetIcon(icon);
			if (DialogMaxSize <= 0)
				_MaxTextAreaWidth = (int)(sr.Width * MAX_WIDTH_PERCENTAGE);
			else
				_MaxTextAreaWidth = DialogMaxSize;
			_MaxTextAreaWidth -= (SystemInformation.Border3DSize.Width * 2 + _Padding.Horizontal);
			if (_MsgIcon != MessageBoxIcon.None)
				_MaxTextAreaWidth -= (_Icon.Width + btn1.Margin.Left);
			_TextSize = g.MeasureString(_PureText, this.Font, _MaxTextAreaWidth, _DefFormat).ToSize();
			_TextSize.Height += 3;
			_HasCsiSgr = !_PureText.Equals(_Message);
			_ButtonArea = new Size(0, flpButtons.Height);

			Point msgLoc = new Point(_Padding.Left, _Padding.Top);
			if (_Icon != null)
			{
				if (_TextSize.Height < _Icon.Height)
					msgLoc.Offset(_Icon.Width + btn1.Margin.Left, (int)((_Icon.Height - _TextSize.Height) / 2));
				else
					msgLoc.Offset(_Icon.Width + btn1.Margin.Left, 0);
				_CanvasArea.Width = _TextSize.Width + _Icon.Width + btn1.Margin.Left;
				_CanvasArea.Height = ((_TextSize.Height > _Icon.Height) ? _TextSize.Height : _Icon.Height);
			}
			else
			{
				_CanvasArea.Width = _TextSize.Width;
				_CanvasArea.Height = _TextSize.Height;
			}
			_TextArea = new RectangleF(msgLoc, _TextSize);

			// 計算按鍵區寬度
			_ButtonArea.Width = btn1.Width + (btn2.Visible ? btn2.Width + btn2.Margin.Left : 0) + (btn3.Visible ? btn3.Width + btn3.Margin.Left : 0);
			_ButtonArea.Width += flpButtons.Padding.Right * 2 + flpButtons.Margin.Horizontal;

			this.Height = flpButtons.Height + _CanvasArea.Height + SystemInformation.Border3DSize.Height + SystemInformation.CaptionHeight + _Padding.Vertical;
			this.Width = (_ButtonArea.Width > _CanvasArea.Width ? _ButtonArea.Width : _CanvasArea.Width) + SystemInformation.Border3DSize.Width * 2 + _Padding.Horizontal;
		}
		#endregion

		#region Protected Override Method : void OnPaint(PaintEventArgs e)
		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			if (_HasCsiSgr)
				DrawAnsiCsiSgrText(g, _Message);
			else
				DrawNormalText(g, _Message);
		}
		#endregion

		#region Protected Override Method : void OnShown(EventArgs e)
		protected override void OnShown(EventArgs e)
		{
			switch (_MsgIcon)
			{
				case MessageBoxIcon.Asterisk:
					System.Media.SystemSounds.Asterisk.Play();
					break;
				case MessageBoxIcon.Error:
					System.Media.SystemSounds.Hand.Play();
					break;
				case MessageBoxIcon.Question:
					System.Media.SystemSounds.Question.Play();
					break;
				case MessageBoxIcon.Exclamation:
					System.Media.SystemSounds.Exclamation.Play();
					break;
			}
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

		#region Private Method : Size DrawNormalText(Graphics g, string text)
		private Size DrawNormalText(Graphics g, string text)
		{
			g.Clear(SystemColors.Window);
			g.DrawLine(new Pen(SystemBrushes.ControlDark, 1), new Point(0, flpButtons.Top - 2), new Point(flpButtons.Width, flpButtons.Top - 2));
			g.DrawLine(new Pen(SystemBrushes.Window, 1), new Point(0, flpButtons.Top - 1), new Point(flpButtons.Width, flpButtons.Top - 1));
			Point leftTop = new Point(_Padding.Left, _Padding.Top);
			if (_Icon != null)
				g.DrawIcon(_Icon, leftTop.X, leftTop.Y);
			// 純文字
			g.DrawString(_PureText, this.Font, SystemBrushes.WindowText, _TextArea, _DefFormat);

			#region For Debug - Draw Message Area Border
#if DEBUG
			g.DrawRectangle(new Pen(Color.Green), Rectangle.Round(_TextArea));
			g.DrawRectangle(new Pen(Color.Red), new Rectangle(leftTop, _CanvasArea));
#endif
			#endregion

			return _CanvasArea;
		}
		#endregion

		#region Private Method : Size DrawAnsiCsiSgrText(Graphics grp, string text)
		private Size DrawAnsiCsiSgrText(Graphics grp, string text)
		{
			grp.Clear(SystemColors.Window);
			grp.DrawLine(new Pen(SystemBrushes.ControlDark, 1), new Point(0, flpButtons.Top - 2), new Point(flpButtons.Width, flpButtons.Top - 2));
			grp.DrawLine(new Pen(SystemBrushes.Window, 1), new Point(0, flpButtons.Top - 1), new Point(flpButtons.Width, flpButtons.Top - 1));
			Point leftTop = new Point(_Padding.Left, _Padding.Top);

			// 繪製圖示
			if (_Icon != null)
				grp.DrawIcon(_Icon, leftTop.X, leftTop.Y);

			Brush bFore = SystemBrushes.ControlText;
			Brush bBack = SystemBrushes.Window;
			Font fText = this.Font;
			MatchCollection mc = Regex.Matches(_SgrText, "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?m", RegexOptions.Multiline);
			PointF loc = _TextArea.Location;
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
					txt = _SgrText.Substring(idx, m.Index - idx);
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
								bFore = new SolidBrush(SystemColors.ControlText);
								bBack = new SolidBrush(SystemColors.Window);
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
								bFore = new SolidBrush(SystemColors.ControlText);
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
								bBack = new SolidBrush(SystemColors.Window);
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
			if (idx < _SgrText.Length)
			{
				txt = _SgrText.Substring(idx, _SgrText.Length - idx);
				loc = DrawWrapCsiSgrText(grp, txt, fText, bFore, bBack, loc);
			}

			#region For Debug - Draw Message Area Border
#if DEBUG
			grp.DrawRectangle(new Pen(Color.Green), Rectangle.Round(_TextArea));
			grp.DrawRectangle(new Pen(Color.Red), new Rectangle(leftTop, _CanvasArea));
			grp.DrawRectangle(new Pen(Color.Blue, 3), loc.X, loc.Y, 3, 3);
#endif
			#endregion

			if (loc.Y + lineHeight > _TextArea.Bottom)
				_CanvasArea.Height += (int)(loc.Y + lineHeight - _TextArea.Bottom);

			return _CanvasArea;
		}
		#endregion

		#region Private Method : PointF DrawWrapCsiSgrText(Graphics g, string txt, Font font, Brush bFore, Brush bBack, PointF loc)
		private PointF DrawWrapCsiSgrText(Graphics g, string txt, Font font, Brush bFore, Brush bBack, PointF loc)
		{
			int idx = 0;
			SizeF lastSize, f1, f2;
			string tmp, st = string.Empty;
			MatchCollection mc;
			RectangleF tRect;
			PointF backOffset = new PointF(0, 0);
			string[] lines = null;
			float lineHeight = g.MeasureString("\r", font, loc, _DefFormat).Height;
			txt = txt.Replace("\n", "\r").Replace("\r\r", "\r");

			#region 處理行首的換行符號
			while (txt.StartsWith("\r"))
			{
				loc.X = _TextArea.Left;
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
						loc.X = _TextArea.Left;
						loc.Y += lineHeight;
					}
					else
					{
						loc = DrawWrapCsiSgrText(g, lines[i], font, bFore, bBack, loc);
						if (i < lines.Length - 1 && !string.IsNullOrEmpty(lines[i + 1]))
						{
							loc.X = _TextArea.Left;
							loc.Y += lineHeight;
						}
					}
				}
				#endregion
			}
			else
			{
				#region 處理單行文字

				lastSize = new SizeF(_TextArea.Width + _TextArea.Left - loc.X, 0);	// 右邊剩下的空間
				f1 = g.MeasureString(txt, font, loc, _DefFormat);
				if (lastSize.Width >= f1.Width)
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
								if (f2.Width + loc.X > _TextArea.Right)
								{
									// 加上字串會超過右限，先繪製之前的字串
									f2 = g.MeasureString(tmp, font, PointF.Empty, _DefFormat);
									tRect = new RectangleF(loc, f2);
									tRect.Offset(backOffset);
									g.FillRectangle(bBack, tRect);
									g.DrawString(tmp, font, bFore, loc, _DefFormat);
									loc.X = _TextArea.Left;
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
							if (f2.Width + loc.X > _TextArea.Right)
							{
								// 加上字串會超過右限，先繪製之前的字串
								f2 = g.MeasureString(tmp, font, PointF.Empty, _DefFormat);
								tRect = new RectangleF(loc, f2);
								tRect.Offset(backOffset);
								g.FillRectangle(bBack, tRect);
								g.DrawString(tmp, font, bFore, loc, _DefFormat);
								loc.X = _TextArea.Left;
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
							if (f1.Width + loc.X > _TextArea.Right)
							{
								f1 = g.MeasureString(txt.Substring(idx, i - idx), font, loc, _DefFormat);
								tRect = new RectangleF(loc, f1);
								tRect.Offset(backOffset);
								g.FillRectangle(bBack, tRect);
								g.DrawString(txt.Substring(idx, i - idx), font, bFore, loc, _DefFormat);
								loc.X = _TextArea.Left;
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
			// MsgDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ClientSize = new System.Drawing.Size(279, 139);
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
		#region Public Static Property : Font Font(R/W)
		/// <summary>設定或取得訊息窗的字型</summary>
		public static Font Font
		{
			get { return MsgDialog.TextFont; }
			set { MsgDialog.TextFont = value; }
		}
		#endregion

		#region Public Static Property : int DialogMaximumWidth(R/W)
		/// <summary>設定或取得視窗最大範圍</summary>
		public static int DialogMaximumWidth
		{
			get { return MsgDialog.DialogMaxSize; }
			set { MsgDialog.DialogMaxSize = value; }
		}
		#endregion

		#region Public Static Property : string ButtonOK
		/// <summary>設定或取得「確定」鈕的文字</summary>
		public static string ButtonOK
		{
			get { return MsgDialog.ButtonOK; }
			set { MsgDialog.ButtonOK = value; }
		}
		#endregion

		#region Public Static Property : string ButtonCancel
		/// <summary>設定或取得「取消」鈕的文字</summary>
		public static string ButtonCancel
		{
			get { return MsgDialog.ButtonCancel; }
			set { MsgDialog.ButtonCancel = value; }
		}
		#endregion

		#region Public Static Property : string ButtonYes
		/// <summary>設定或取得「是」鈕的文字</summary>
		public static string ButtonYes
		{
			get { return MsgDialog.ButtonYes; }
			set { MsgDialog.ButtonYes = value; }
		}
		#endregion

		#region Public Static Property : string ButtonNo
		/// <summary>設定或取得「否」鈕的文字</summary>
		public static string ButtonNo
		{
			get { return MsgDialog.ButtonNo; }
			set { MsgDialog.ButtonNo = value; }
		}
		#endregion

		#region Public Static Property : string ButtonRetry
		/// <summary>設定或取得「重試」鈕的文字</summary>
		public static string ButtonRetry
		{
			get { return MsgDialog.ButtonRetry; }
			set { MsgDialog.ButtonRetry = value; }
		}
		#endregion

		#region Public Static Property : string ButtonAbort
		/// <summary>設定或取得「中止」鈕的文字</summary>
		public static string ButtonAbort
		{
			get { return MsgDialog.ButtonAbort; }
			set { MsgDialog.ButtonAbort = value; }
		}
		#endregion

		#region Public Static Property : string ButtonIgnore
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

/*
 * 8bits SGR Colors
<table class="collapsible" style="text-align:center;font-size:80%;width:100%;background:#f6f6f6;cursor:default;" cellpadding="0" cellspacing="1" id="collapsibleTable0">
<tbody><tr>
<th colspan="36"><span class="collapseButton">[<a id="collapseButton0" href="#">隱藏</a>]</span>256色模式 — 前景色：<code>ESC[38;5;#m</code>&nbsp;&nbsp;&nbsp;背景色：<code>ESC[48;5;#m</code></th>
</tr>
<tr style="">
<td colspan="18">標準色</td>
<td colspan="18">高強度色</td>
</tr>
<tr style="">
<td colspan="36">
<table style="width:100%;text-align:center;font-weight:bold;">
<tbody><tr>
<td style="color:#ffffff;background:#000000;" title="#000000">&nbsp;0&nbsp;</td>
<td style="color:#ffffff;background:#800000;" title="#800000">&nbsp;1&nbsp;</td>
<td style="color:#ffffff;background:#008000;" title="#008000">&nbsp;2&nbsp;</td>
<td style="color:#ffffff;background:#808000;" title="#808000">&nbsp;3&nbsp;</td>
<td style="color:#ffffff;background:#000080;" title="#000080">&nbsp;4&nbsp;</td>
<td style="color:#ffffff;background:#800080;" title="#800080">&nbsp;5&nbsp;</td>
<td style="color:#ffffff;background:#008080;" title="#008080">&nbsp;6&nbsp;</td>
<td style="color:#ffffff;background:#c0c0c0;" title="#c0c0c0">&nbsp;7&nbsp;</td>
<td style="width:1em;"></td>
<td style="color:#000000;background:#808080;" title="#808080">&nbsp;8&nbsp;</td>
<td style="color:#000000;background:#ff0000;" title="#ff0000">&nbsp;9&nbsp;</td>
<td style="color:#000000;background:#00ff00;" title="#00ff00">10</td>
<td style="color:#000000;background:#ffff00;" title="#ffff00">11</td>
<td style="color:#000000;background:#0000ff;" title="#0000ff">12</td>
<td style="color:#000000;background:#ff00ff;" title="#ff00ff">13</td>
<td style="color:#000000;background:#00ffff;" title="#00ffff">14</td>
<td style="color:#000000;background:#ffffff;" title="#ffffff">15</td>
</tr>
</tbody></table>
</td>
</tr>
<tr style="">
<td colspan="36">216色</td>
</tr>
<tr style="">
<td style="color:#ffffff;background:#000000;" title="#000000">16</td>
<td style="color:#ffffff;background:#00005f;" title="#00005f">17</td>
<td style="color:#ffffff;background:#000087;" title="#000087">18</td>
<td style="color:#ffffff;background:#0000af;" title="#0000af">19</td>
<td style="color:#ffffff;background:#0000d7;" title="#0000d7">20</td>
<td style="color:#ffffff;background:#0000ff;" title="#0000ff">21</td>
<td style="color:#ffffff;background:#005f00;" title="#005f00">22</td>
<td style="color:#ffffff;background:#005f5f;" title="#005f5f">23</td>
<td style="color:#ffffff;background:#005f87;" title="#005f87">24</td>
<td style="color:#ffffff;background:#005faf;" title="#005faf">25</td>
<td style="color:#ffffff;background:#005fd7;" title="#005fd7">26</td>
<td style="color:#ffffff;background:#005fff;" title="#005fff">27</td>
<td style="color:#ffffff;background:#008700;" title="#008700">28</td>
<td style="color:#ffffff;background:#00875f;" title="#00875f">29</td>
<td style="color:#ffffff;background:#008787;" title="#008787">30</td>
<td style="color:#ffffff;background:#0087af;" title="#0087af">31</td>
<td style="color:#ffffff;background:#0087d7;" title="#0087d7">32</td>
<td style="color:#ffffff;background:#0087ff;" title="#0087ff">33</td>
<td style="color:#000000;background:#00af00;" title="#00af00">34</td>
<td style="color:#000000;background:#00af5f;" title="#00af5f">35</td>
<td style="color:#000000;background:#00af87;" title="#00af87">36</td>
<td style="color:#000000;background:#00afaf;" title="#00afaf">37</td>
<td style="color:#000000;background:#00afd7;" title="#00afd7">38</td>
<td style="color:#000000;background:#00afff;" title="#00afff">39</td>
<td style="color:#000000;background:#00d700;" title="#00d700">40</td>
<td style="color:#000000;background:#00d75f;" title="#00d75f">41</td>
<td style="color:#000000;background:#00d787;" title="#00d787">42</td>
<td style="color:#000000;background:#00d7af;" title="#00d7af">43</td>
<td style="color:#000000;background:#00d7d7;" title="#00d7d7">44</td>
<td style="color:#000000;background:#00d7ff;" title="#00d7ff">45</td>
<td style="color:#000000;background:#00ff00;" title="#00ff00">46</td>
<td style="color:#000000;background:#00ff5f;" title="#00ff5f">47</td>
<td style="color:#000000;background:#00ff87;" title="#00ff87">48</td>
<td style="color:#000000;background:#00ffaf;" title="#00ffaf">49</td>
<td style="color:#000000;background:#00ffd7;" title="#00ffd7">50</td>
<td style="color:#000000;background:#00ffff;" title="#00ffff">51</td>
</tr>
<tr style="">
<td style="color:#ffffff;background:#5f0000;" title="#5f0000">52</td>
<td style="color:#ffffff;background:#5f005f;" title="#5f005f">53</td>
<td style="color:#ffffff;background:#5f0087;" title="#5f0087">54</td>
<td style="color:#ffffff;background:#5f00af;" title="#5f00af">55</td>
<td style="color:#ffffff;background:#5f00d7;" title="#5f00d7">56</td>
<td style="color:#ffffff;background:#5f00ff;" title="#5f00ff">57</td>
<td style="color:#ffffff;background:#5f5f00;" title="#5f5f00">58</td>
<td style="color:#ffffff;background:#5f5f5f;" title="#5f5f5f">59</td>
<td style="color:#ffffff;background:#5f5f87;" title="#5f5f87">60</td>
<td style="color:#ffffff;background:#5f5faf;" title="#5f5faf">61</td>
<td style="color:#ffffff;background:#5f5fd7;" title="#5f5fd7">62</td>
<td style="color:#ffffff;background:#5f5fff;" title="#5f5fff">63</td>
<td style="color:#ffffff;background:#5f8700;" title="#5f8700">64</td>
<td style="color:#ffffff;background:#5f875f;" title="#5f875f">65</td>
<td style="color:#ffffff;background:#5f8787;" title="#5f8787">66</td>
<td style="color:#ffffff;background:#5f87af;" title="#5f87af">67</td>
<td style="color:#ffffff;background:#5f87d7;" title="#5f87d7">68</td>
<td style="color:#ffffff;background:#5f87ff;" title="#5f87ff">69</td>
<td style="color:#000000;background:#5faf00;" title="#5faf00">70</td>
<td style="color:#000000;background:#5faf5f;" title="#5faf5f">71</td>
<td style="color:#000000;background:#5faf87;" title="#5faf87">72</td>
<td style="color:#000000;background:#5fafaf;" title="#5fafaf">73</td>
<td style="color:#000000;background:#5fafd7;" title="#5fafd7">74</td>
<td style="color:#000000;background:#5fafff;" title="#5fafff">75</td>
<td style="color:#000000;background:#5fd700;" title="#5fd700">76</td>
<td style="color:#000000;background:#5fd75f;" title="#5fd75f">77</td>
<td style="color:#000000;background:#5fd787;" title="#5fd787">78</td>
<td style="color:#000000;background:#5fd7af;" title="#5fd7af">79</td>
<td style="color:#000000;background:#5fd7d7;" title="#5fd7d7">80</td>
<td style="color:#000000;background:#5fd7ff;" title="#5fd7ff">81</td>
<td style="color:#000000;background:#5fff00;" title="#5fff00">82</td>
<td style="color:#000000;background:#5fff5f;" title="#5fff5f">83</td>
<td style="color:#000000;background:#5fff87;" title="#5fff87">84</td>
<td style="color:#000000;background:#5fffaf;" title="#5fffaf">85</td>
<td style="color:#000000;background:#5fffd7;" title="#5fffd7">86</td>
<td style="color:#000000;background:#5fffff;" title="#5fffff">87</td>
</tr>
<tr style="">
<td style="color:#ffffff;background:#870000;" title="#870000">88</td>
<td style="color:#ffffff;background:#87005f;" title="#87005f">89</td>
<td style="color:#ffffff;background:#870087;" title="#870087">90</td>
<td style="color:#ffffff;background:#8700af;" title="#8700af">91</td>
<td style="color:#ffffff;background:#8700d7;" title="#8700d7">92</td>
<td style="color:#ffffff;background:#8700ff;" title="#8700ff">93</td>
<td style="color:#ffffff;background:#875f00;" title="#875f00">94</td>
<td style="color:#ffffff;background:#875f5f;" title="#875f5f">95</td>
<td style="color:#ffffff;background:#875f87;" title="#875f87">96</td>
<td style="color:#ffffff;background:#875faf;" title="#875faf">97</td>
<td style="color:#ffffff;background:#875fd7;" title="#875fd7">98</td>
<td style="color:#ffffff;background:#875fff;" title="#875fff">99</td>
<td style="color:#ffffff;background:#878700;" title="#878700">100</td>
<td style="color:#ffffff;background:#87875f;" title="#87875f">101</td>
<td style="color:#ffffff;background:#878787;" title="#878787">102</td>
<td style="color:#ffffff;background:#8787af;" title="#8787af">103</td>
<td style="color:#ffffff;background:#8787d7;" title="#8787d7">104</td>
<td style="color:#ffffff;background:#8787ff;" title="#8787ff">105</td>
<td style="color:#000000;background:#87af00;" title="#87af00">106</td>
<td style="color:#000000;background:#87af5f;" title="#87af5f">107</td>
<td style="color:#000000;background:#87af87;" title="#87af87">108</td>
<td style="color:#000000;background:#87afaf;" title="#87afaf">109</td>
<td style="color:#000000;background:#87afd7;" title="#87afd7">110</td>
<td style="color:#000000;background:#87afff;" title="#87afff">111</td>
<td style="color:#000000;background:#87d700;" title="#87d700">112</td>
<td style="color:#000000;background:#87d75f;" title="#87d75f">113</td>
<td style="color:#000000;background:#87d787;" title="#87d787">114</td>
<td style="color:#000000;background:#87d7af;" title="#87d7af">115</td>
<td style="color:#000000;background:#87d7d7;" title="#87d7d7">116</td>
<td style="color:#000000;background:#87d7ff;" title="#87d7ff">117</td>
<td style="color:#000000;background:#87ff00;" title="#87ff00">118</td>
<td style="color:#000000;background:#87ff5f;" title="#87ff5f">119</td>
<td style="color:#000000;background:#87ff87;" title="#87ff87">120</td>
<td style="color:#000000;background:#87ffaf;" title="#87ffaf">121</td>
<td style="color:#000000;background:#87ffd7;" title="#87ffd7">122</td>
<td style="color:#000000;background:#87ffff;" title="#87ffff">123</td>
</tr>
<tr style="">
<td style="color:#ffffff;background:#af0000;" title="#af0000">124</td>
<td style="color:#ffffff;background:#af005f;" title="#af005f">125</td>
<td style="color:#ffffff;background:#af0087;" title="#af0087">126</td>
<td style="color:#ffffff;background:#af00af;" title="#af00af">127</td>
<td style="color:#ffffff;background:#af00d7;" title="#af00d7">128</td>
<td style="color:#ffffff;background:#af00ff;" title="#af00ff">129</td>
<td style="color:#ffffff;background:#af5f00;" title="#af5f00">130</td>
<td style="color:#ffffff;background:#af5f5f;" title="#af5f5f">131</td>
<td style="color:#ffffff;background:#af5f87;" title="#af5f87">132</td>
<td style="color:#ffffff;background:#af5faf;" title="#af5faf">133</td>
<td style="color:#ffffff;background:#af5fd7;" title="#af5fd7">134</td>
<td style="color:#ffffff;background:#af5fff;" title="#af5fff">135</td>
<td style="color:#ffffff;background:#af8700;" title="#af8700">136</td>
<td style="color:#ffffff;background:#af875f;" title="#af875f">137</td>
<td style="color:#ffffff;background:#af8787;" title="#af8787">138</td>
<td style="color:#ffffff;background:#af87af;" title="#af87af">139</td>
<td style="color:#ffffff;background:#af87d7;" title="#af87d7">140</td>
<td style="color:#ffffff;background:#af87ff;" title="#af87ff">141</td>
<td style="color:#000000;background:#afaf00;" title="#afaf00">142</td>
<td style="color:#000000;background:#afaf5f;" title="#afaf5f">143</td>
<td style="color:#000000;background:#afaf87;" title="#afaf87">144</td>
<td style="color:#000000;background:#afafaf;" title="#afafaf">145</td>
<td style="color:#000000;background:#afafd7;" title="#afafd7">146</td>
<td style="color:#000000;background:#afafff;" title="#afafff">147</td>
<td style="color:#000000;background:#afd700;" title="#afd700">148</td>
<td style="color:#000000;background:#afd75f;" title="#afd75f">149</td>
<td style="color:#000000;background:#afd787;" title="#afd787">150</td>
<td style="color:#000000;background:#afd7af;" title="#afd7af">151</td>
<td style="color:#000000;background:#afd7d7;" title="#afd7d7">152</td>
<td style="color:#000000;background:#afd7ff;" title="#afd7ff">153</td>
<td style="color:#000000;background:#afff00;" title="#afff00">154</td>
<td style="color:#000000;background:#afff5f;" title="#afff5f">155</td>
<td style="color:#000000;background:#afff87;" title="#afff87">156</td>
<td style="color:#000000;background:#afffaf;" title="#afffaf">157</td>
<td style="color:#000000;background:#afffd7;" title="#afffd7">158</td>
<td style="color:#000000;background:#afffff;" title="#afffff">159</td>
</tr>
<tr style="">
<td style="color:#ffffff;background:#d70000;" title="#d70000">160</td>
<td style="color:#ffffff;background:#d7005f;" title="#d7005f">161</td>
<td style="color:#ffffff;background:#d70087;" title="#d70087">162</td>
<td style="color:#ffffff;background:#d700af;" title="#d700af">163</td>
<td style="color:#ffffff;background:#d700d7;" title="#d700d7">164</td>
<td style="color:#ffffff;background:#d700ff;" title="#d700ff">165</td>
<td style="color:#ffffff;background:#d75f00;" title="#d75f00">166</td>
<td style="color:#ffffff;background:#d75f5f;" title="#d75f5f">167</td>
<td style="color:#ffffff;background:#d75f87;" title="#d75f87">168</td>
<td style="color:#ffffff;background:#d75faf;" title="#d75faf">169</td>
<td style="color:#ffffff;background:#d75fd7;" title="#d75fd7">170</td>
<td style="color:#ffffff;background:#d75fff;" title="#d75fff">171</td>
<td style="color:#ffffff;background:#d78700;" title="#d78700">172</td>
<td style="color:#ffffff;background:#d7875f;" title="#d7875f">173</td>
<td style="color:#ffffff;background:#d78787;" title="#d78787">174</td>
<td style="color:#ffffff;background:#d787af;" title="#d787af">175</td>
<td style="color:#ffffff;background:#d787d7;" title="#d787d7">176</td>
<td style="color:#ffffff;background:#d787ff;" title="#d787ff">177</td>
<td style="color:#000000;background:#d7af00;" title="#d7af00">178</td>
<td style="color:#000000;background:#d7af5f;" title="#d7af5f">179</td>
<td style="color:#000000;background:#d7af87;" title="#d7af87">180</td>
<td style="color:#000000;background:#d7afaf;" title="#d7afaf">181</td>
<td style="color:#000000;background:#d7afd7;" title="#d7afd7">182</td>
<td style="color:#000000;background:#d7afff;" title="#d7afff">183</td>
<td style="color:#000000;background:#d7d700;" title="#d7d700">184</td>
<td style="color:#000000;background:#d7d75f;" title="#d7d75f">185</td>
<td style="color:#000000;background:#d7d787;" title="#d7d787">186</td>
<td style="color:#000000;background:#d7d7af;" title="#d7d7af">187</td>
<td style="color:#000000;background:#d7d7d7;" title="#d7d7d7">188</td>
<td style="color:#000000;background:#d7d7ff;" title="#d7d7ff">189</td>
<td style="color:#000000;background:#d7ff00;" title="#d7ff00">190</td>
<td style="color:#000000;background:#d7ff5f;" title="#d7ff5f">191</td>
<td style="color:#000000;background:#d7ff87;" title="#d7ff87">192</td>
<td style="color:#000000;background:#d7ffaf;" title="#d7ffaf">193</td>
<td style="color:#000000;background:#d7ffd7;" title="#d7ffd7">194</td>
<td style="color:#000000;background:#d7ffff;" title="#d7ffff">195</td>
</tr>
<tr style="">
<td style="color:#ffffff;background:#ff0000;" title="#ff0000">196</td>
<td style="color:#ffffff;background:#ff005f;" title="#ff005f">197</td>
<td style="color:#ffffff;background:#ff0087;" title="#ff0087">198</td>
<td style="color:#ffffff;background:#ff00af;" title="#ff00af">199</td>
<td style="color:#ffffff;background:#ff00d7;" title="#ff00d7">200</td>
<td style="color:#ffffff;background:#ff00ff;" title="#ff00ff">201</td>
<td style="color:#ffffff;background:#ff5f00;" title="#ff5f00">202</td>
<td style="color:#ffffff;background:#ff5f5f;" title="#ff5f5f">203</td>
<td style="color:#ffffff;background:#ff5f87;" title="#ff5f87">204</td>
<td style="color:#ffffff;background:#ff5faf;" title="#ff5faf">205</td>
<td style="color:#ffffff;background:#ff5fd7;" title="#ff5fd7">206</td>
<td style="color:#ffffff;background:#ff5fff;" title="#ff5fff">207</td>
<td style="color:#ffffff;background:#ff8700;" title="#ff8700">208</td>
<td style="color:#ffffff;background:#ff875f;" title="#ff875f">209</td>
<td style="color:#ffffff;background:#ff8787;" title="#ff8787">210</td>
<td style="color:#ffffff;background:#ff87af;" title="#ff87af">211</td>
<td style="color:#ffffff;background:#ff87d7;" title="#ff87d7">212</td>
<td style="color:#ffffff;background:#ff87ff;" title="#ff87ff">213</td>
<td style="color:#000000;background:#ffaf00;" title="#ffaf00">214</td>
<td style="color:#000000;background:#ffaf5f;" title="#ffaf5f">215</td>
<td style="color:#000000;background:#ffaf87;" title="#ffaf87">216</td>
<td style="color:#000000;background:#ffafaf;" title="#ffafaf">217</td>
<td style="color:#000000;background:#ffafd7;" title="#ffafd7">218</td>
<td style="color:#000000;background:#ffafff;" title="#ffafff">219</td>
<td style="color:#000000;background:#ffd700;" title="#ffd700">220</td>
<td style="color:#000000;background:#ffd75f;" title="#ffd75f">221</td>
<td style="color:#000000;background:#ffd787;" title="#ffd787">222</td>
<td style="color:#000000;background:#ffd7af;" title="#ffd7af">223</td>
<td style="color:#000000;background:#ffd7d7;" title="#ffd7d7">224</td>
<td style="color:#000000;background:#ffd7ff;" title="#ffd7ff">225</td>
<td style="color:#000000;background:#ffff00;" title="#ffff00">226</td>
<td style="color:#000000;background:#ffff5f;" title="#ffff5f">227</td>
<td style="color:#000000;background:#ffff87;" title="#ffff87">228</td>
<td style="color:#000000;background:#ffffaf;" title="#ffffaf">229</td>
<td style="color:#000000;background:#ffffd7;" title="#ffffd7">230</td>
<td style="color:#000000;background:#ffffff;" title="#ffffff">231</td>
</tr>
<tr style="">
<td colspan="36">灰度色</td>
</tr>
<tr style="">
<td colspan="36">
<table style="width:100%;text-align:center;font-weight:bold;">
<tbody><tr>
<td style="color:#ffffff;background:#080808;" title="#080808">232</td>
<td style="color:#ffffff;background:#121212;" title="#121212">233</td>
<td style="color:#ffffff;background:#1c1c1c;" title="#1c1c1c">234</td>
<td style="color:#ffffff;background:#262626;" title="#262626">235</td>
<td style="color:#ffffff;background:#303030;" title="#303030">236</td>
<td style="color:#ffffff;background:#3a3a3a;" title="#3a3a3a">237</td>
<td style="color:#ffffff;background:#444444;" title="#444444">238</td>
<td style="color:#ffffff;background:#4e4e4e;" title="#4e4e4e">239</td>
<td style="color:#ffffff;background:#585858;" title="#585858">240</td>
<td style="color:#ffffff;background:#626262;" title="#626262">241</td>
<td style="color:#ffffff;background:#6c6c6c;" title="#6c6c6c">242</td>
<td style="color:#ffffff;background:#767676;" title="#767676">243</td>
<td style="color:#000000;background:#808080;" title="#808080">244</td>
<td style="color:#000000;background:#8a8a8a;" title="#8a8a8a">245</td>
<td style="color:#000000;background:#949494;" title="#949494">246</td>
<td style="color:#000000;background:#9e9e9e;" title="#9e9e9e">247</td>
<td style="color:#000000;background:#a8a8a8;" title="#a8a8a8">248</td>
<td style="color:#000000;background:#b2b2b2;" title="#b2b2b2">249</td>
<td style="color:#000000;background:#bcbcbc;" title="#bcbcbc">250</td>
<td style="color:#000000;background:#c6c6c6;" title="#c6c6c6">251</td>
<td style="color:#000000;background:#d0d0d0;" title="#d0d0d0">252</td>
<td style="color:#000000;background:#dadada;" title="#dadada">253</td>
<td style="color:#000000;background:#e4e4e4;" title="#e4e4e4">254</td>
<td style="color:#000000;background:#eeeeee;" title="#eeeeee">255</td>
</tr>
</tbody></table>
</td>
</tr>
</tbody></table>
*/