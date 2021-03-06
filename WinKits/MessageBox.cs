﻿using CJF.Utility.Ansi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

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
        //[DllImport("user32.dll")]
        //static extern bool SetProcessDpiAwarenessContext(uint value);

        internal const uint SC_CLOSE = 0xF060;
        internal const uint MF_GRAYED = 0x00000001;
        internal const uint MF_BYCOMMAND = 0x00000000;
        const uint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 0x04;
        #endregion

        #region Controls
        /// <summary>Required designer variable.</summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btn3;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.FlowLayoutPanel flpButtons;
        #endregion

        #region Consts
        const float MAX_WIDTH_PERCENTAGE = 0.35F;
        static readonly SizeF DEF_DPI = new SizeF(96F, 96F);
        static readonly Size DEF_ICON_SIZE = new Size(32, 32);
        static readonly Size DEF_BUTTON_SIZE = new Size(90, 28);
        static readonly Padding DEF_BUTTON_PANEL_PADDING = new Padding(10);
        static readonly Size DEF_BUTTON_PANEL_SIZE = new Size(-1, DEF_BUTTON_SIZE.Height + DEF_BUTTON_PANEL_PADDING.Vertical);
        static readonly Padding DEF_BUTTON_MARGIN = new Padding(8, 0, 0, 0);
        static readonly Padding DEF_CANVAS_PADDING = new Padding(25, 20, 25, 0);
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

        /// <summary>DPI 放大倍數。</summary>
        internal static float DpiGain = 0;
        /// <summary>設定或取得按鈕大小。</summary>
        internal static Size ButtonSize { get; set; }
        internal static Padding ButtonPanelPadding { get; set; }
        internal static Size ButtonPanelSize { get; set; }
        internal static Size IconSize { get; set; }
        internal static Padding ButtonMargin { get; set; }
        internal static Padding Canvas_Padding { get; set; }
        #endregion

        #region Private Variables
        readonly string _Message = string.Empty;
        readonly string _SgrText = string.Empty;
        readonly string _PureText = string.Empty;
        readonly bool _HasCsiSgr = false;
        readonly int _MaxTextWidth = 0;
        readonly MessageBoxButtons _Buttons = MessageBoxButtons.OK;
        readonly MessageBoxDefaultButton _DefButton = MessageBoxDefaultButton.Button1;
        readonly MessageBoxIcon _MsgIcon = MessageBoxIcon.None;

        int _CanvasMaxWidth = 0;
        Size _TextSize = Size.Empty;
        Icon _Icon = null;
        Size _ButtonArea = Size.Empty;
        StringFormat _DefFormat = new StringFormat(StringFormat.GenericTypographic);
        Bitmap _Buffer = null;
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
            ResetControlsSize();
            this.DialogResult = DialogResult.None;
            this.Text = caption;
            if (TextFont != null)
                this.Font = TextFont;
            else
                this.Font = SystemFonts.MessageBoxFont;
            this.Size = Size.Empty;
            _Buttons = button;
            _DefButton = defButton;
            _DefFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

            Rectangle sr = Screen.GetWorkingArea(this);
            _Message = text;
            _MsgIcon = icon;
            _SgrText = CsiBuilder.GetOnlySGR(_Message);
            _PureText = CsiBuilder.GetPureText(_Message);
            _Icon = GetIcon(icon);
            if (DialogMaxSize.IsEmpty)
                _MaxTextWidth = (int)(sr.Width * MAX_WIDTH_PERCENTAGE);
            else
                _MaxTextWidth = DialogMaxSize.Width;
            float lineHeight = 0;
            using (Graphics g = this.CreateGraphics())
            {
                _TextSize = g.MeasureString(_PureText, this.Font, _MaxTextWidth, _DefFormat).ToSize();
                lineHeight = g.MeasureString("@", this.Font, Point.Empty, _DefFormat).Height;
            }
            _TextSize.Height += (int)(5 * DpiGain);
            _TextSize.Width += (int)(5 * DpiGain);
            _HasCsiSgr = !_PureText.Equals(_Message);

            #region Draw Text to Buffer
            Size res = Size.Empty;
            using (Bitmap bitmap = new Bitmap(_MaxTextWidth, _TextSize.Height + (int)(lineHeight * 5)))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.Clear(SystemColors.Window);
                    res = DrawAnsiCsiSgrText(g, _SgrText);
                }
                _Buffer = bitmap.Clone(new Rectangle(Point.Empty, res), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            }
            if (_MsgIcon != MessageBoxIcon.None)
            {
                res.Height = Math.Max(res.Height, _Icon.Height);
            }
            _TextSize = res;
            #endregion
        }
        #endregion

        #region Protected Override Method : void OnPaint(PaintEventArgs e)
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(SystemColors.Window);
            g.DrawImage(_Buffer, Canvas_Padding.Left, Canvas_Padding.Top);
            g.DrawLine(new Pen(SystemBrushes.ControlDark, 1), new Point(0, flpButtons.Top - 2), new Point(flpButtons.Width, flpButtons.Top - 2));
            g.DrawLine(new Pen(SystemBrushes.Window, 1), new Point(0, flpButtons.Top - 1), new Point(flpButtons.Width, flpButtons.Top - 1));

            #region For Debug - Draw Message Area Border
#if DEBUG
            g.DrawRectangle(new Pen(Color.Blue), new Rectangle(Canvas_Padding.LeftTop(), _Buffer.Size));
#endif
            #endregion
        }
        #endregion

        #region Protected Override Method : void OnResize(EventArgs e)
        protected override void OnResize(EventArgs e)
        {
            if (Owner is null)
                this.CenterToParent();
            else
                this.CenterToScreen();
            base.OnResize(e);
        }
        #endregion

        #region Protected Override Method : void OnShown(EventArgs e)
        protected override void OnShown(EventArgs e)
        {
            SetButtons(_Buttons, _DefButton);
            _ButtonArea = ButtonPanelSize;
            _ButtonArea.Width = btn1.Width + (btn2.Visible ? btn2.Width + btn2.Margin.Horizontal : 0) + (btn3.Visible ? btn3.Width + btn3.Margin.Horizontal : 0);
            _ButtonArea.Width += ButtonPanelPadding.Horizontal;
            Size sw = _TextSize;
            sw.Width += Canvas_Padding.Horizontal;
            sw.Height += Canvas_Padding.Vertical;
            this.ClientSize = new Size()
            {
                Width = (_ButtonArea.Width > sw.Width ? _ButtonArea.Width : sw.Width) + SystemInformation.Border3DSize.Width * 2,
                Height = flpButtons.Height + sw.Height + SystemInformation.Border3DSize.Height + SystemInformation.CaptionHeight
            };
            // 計算按鍵區寬度
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
            base.OnShown(e);
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

        #region Private Method : void ResetControlsSize()
        private void ResetControlsSize()
        {
            flpButtons.Height = ButtonPanelSize.Height;
            flpButtons.Padding = ButtonPanelPadding;
            foreach (Control c in flpButtons.Controls)
            {
                c.Size = ButtonSize;
                c.Margin = ButtonMargin;
            }
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

        #region Private Method : Size DrawNormalText(Graphics grp, string text) - Remarked
        //private Size DrawNormalText(Graphics grp, string text)
        //{
        //    // 純文字
        //    Point leftTop = Point.Empty;
        //    Size s = Size.Empty;
        //    int lineHeight = grp.MeasureString("@", this.Font, Point.Empty, _DefFormat).ToSize().Height;
        //    if (_MsgIcon != MessageBoxIcon.None)
        //    {
        //        leftTop.X = IconSize.Width + btn1.Margin.Left;
        //        s = grp.MeasureString(text, this.Font, (_MaxTextWidth - leftTop.X), _DefFormat).ToSize();
        //        grp.DrawIcon(_Icon, new Rectangle(Point.Empty, IconSize));
        //        if (s.Height < IconSize.Height)
        //            leftTop.Y = (IconSize.Height - s.Height) / 2;
        //    }
        //    else
        //    {
        //        s = grp.MeasureString(text, this.Font, _MaxTextWidth, _DefFormat).ToSize();
        //    }
        //    s.Height += lineHeight;
        //    grp.DrawString(text, this.Font, SystemBrushes.WindowText, new Rectangle(leftTop, s), _DefFormat);
        //    s = s.Addition(new Size(leftTop));
        //    s.Height -= lineHeight;
        //    return s;
        //}
        #endregion

        #region Private Method : Size DrawAnsiCsiSgrText(Graphics grp, string sgrText)
        private Size DrawAnsiCsiSgrText(Graphics grp, string sgrText)
        {
            _CanvasMaxWidth = 0;

            PointF loc = PointF.Empty;
            Brush bFore = SystemBrushes.ControlText;
            Brush bBack = SystemBrushes.Window;
            Font fText = this.Font;
            int idx = 0;
            SizeF f1 = SizeF.Empty, f2 = SizeF.Empty, f3 = SizeF.Empty;
            string txt = string.Empty, tmp = string.Empty, st = string.Empty;
            SizeF lastSize = SizeF.Empty;
            List<string> line = new List<string>();
            // 繪製圖示
            if (_MsgIcon != MessageBoxIcon.None)
            {
                grp.DrawImage(_Icon.ToBitmap(), new Rectangle(Point.Empty, IconSize));
                _CanvasMaxWidth = IconSize.Width + btn1.Margin.Left;
                loc.X += IconSize.Width + btn1.Margin.Left;
                txt = CsiBuilder.GetPureText(sgrText);
                int h = grp.MeasureString(txt, fText, _MaxTextWidth, _DefFormat).ToSize().Height;
                if (IconSize.Height > h)
                {
                    loc.Y = (IconSize.Height - h) / 2;
                }
            }

            MatchCollection mc = Regex.Matches(sgrText, "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?m", RegexOptions.Multiline);
            float lineHeight = grp.MeasureString("@", fText, Point.Empty, _DefFormat).Height;
            bool colorReverse = false;
            foreach (Match m in mc)
            {
                if (!m.Success) continue;
                if (idx != m.Index)
                {
                    txt = sgrText.Substring(idx, m.Index - idx);
                    loc = DrawWrapCsiSgrText(grp, txt, fText, bFore, bBack, loc);
                    _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
                }

                #region 設定顏色與字型
                ushort v = 0;
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    if (!m.Groups[i].Success)
                        continue;
                    v = Convert.ToUInt16(m.Groups[i].Value);
                    if (v >= 30 && v <= 37 || v >= 90 && v <= 97)
                        bFore = new SolidBrush(((SgrColors)v).ToColor());           // 3/4 位元前景色
                    else if (v >= 40 && v <= 47 || v >= 100 && v <= 107)
                        bBack = new SolidBrush(((SgrColors)(v - 10)).ToColor());    // 3/4 位元背景色
                    else
                    {
                        #region 其餘 SGR 參數
                        switch (v)
                        {
                            case 0:     // 重設/正常
                                bFore = new SolidBrush(SystemColors.ControlText);
                                bBack = new SolidBrush(SystemColors.Window);
                                fText = this.Font;
                                colorReverse = false;
                                break;
                            case 1:     // 粗體
                                fText = new Font(fText.FontFamily, fText.Size, fText.Style | FontStyle.Bold);
                                break;
                            case 4:     // 底線
                                fText = new Font(fText.FontFamily, fText.Size, fText.Style | FontStyle.Underline);
                                break;
                            case 7:     // 前景與背景色互換
                                if (!colorReverse)
                                {
                                    colorReverse = true;
                                    Brush b = bFore;
                                    bFore = bBack;
                                    bBack = b;
                                }
                                break;
                            case 9:     // 刪除線
                                fText = new Font(fText.FontFamily, fText.Size, fText.Style | FontStyle.Strikeout);
                                break;
                            case 21:    // 關閉粗體
                                fText = new Font(fText.FontFamily, fText.Size, fText.Style ^ FontStyle.Bold);
                                break;
                            case 22:    // 正常字型
                                fText = new Font(fText.FontFamily, fText.Size);
                                break;
                            case 24:    // 關閉底線
                                fText = new Font(fText.FontFamily, fText.Size, fText.Style ^ FontStyle.Underline);
                                break;
                            case 29:    // 關閉刪除線
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
                            case 38:    // 8 位元前景色
                                if (i + 1 >= m.Groups.Count)
                                    break;  // 錯誤格式，忽略
                                v = Convert.ToUInt16(m.Groups[i + 1].Value);
                                if (v == 5)
                                {
                                    // 8-bits Colors
                                    if (i + 2 >= m.Groups.Count)
                                        break;  // 錯誤格式，忽略
                                    v = Convert.ToUInt16(m.Groups[i + 2].Value);
                                    if (v < 0 || v > 255)
                                        break;  // 顏色值錯誤，忽略
                                    bFore = new SolidBrush(Get8bitsColor(v));
                                    i += 2;
                                }
                                else if (v == 2)
                                {
                                    // 24-bits Colors
                                    if (i + 4 >= m.Groups.Count)
                                        break;  // 錯誤格式，忽略
                                    bFore = new SolidBrush(Color.FromArgb(Convert.ToUInt16(m.Groups[i + 2].Value), Convert.ToUInt16(m.Groups[i + 3].Value), Convert.ToUInt16(m.Groups[i + 4].Value)));
                                    i += 4;
                                }
                                break;
                            case 39:    // 還原預設前景色
                                bFore = new SolidBrush(SystemColors.ControlText);
                                break;
                            case 48:    // 8 位元背景色
                                if (i + 1 >= m.Groups.Count)
                                    break;  // 錯誤格式，忽略
                                v = Convert.ToUInt16(m.Groups[i + 1].Value);
                                if (v == 5)
                                {
                                    // 8-bits Colors
                                    if (i + 2 >= m.Groups.Count)
                                        break;  // 錯誤格式，忽略
                                    v = Convert.ToUInt16(m.Groups[i + 2].Value);
                                    if (v < 0 || v > 255)
                                        break;  // 顏色值錯誤，忽略
                                    bBack = new SolidBrush(Get8bitsColor(v));
                                    i += 2;
                                }
                                else if (v == 2)
                                {
                                    // 24-bits Colors
                                    if (i + 4 >= m.Groups.Count)
                                        break;  // 錯誤格式，忽略
                                    bBack = new SolidBrush(Color.FromArgb(Convert.ToUInt16(m.Groups[i + 2].Value), Convert.ToUInt16(m.Groups[i + 3].Value), Convert.ToUInt16(m.Groups[i + 4].Value)));
                                    i += 4;
                                }
                                break;
                            case 49:    // 還原預設背景色
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
            if (idx < sgrText.Length)
            {
                txt = sgrText.Substring(idx, sgrText.Length - idx);
                loc = DrawWrapCsiSgrText(grp, txt, fText, bFore, bBack, loc);
                _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
            }

            if (!bFore.Equals(SystemBrushes.ControlText))
                bFore?.Dispose();
            if (!bBack.Equals(SystemBrushes.Window))
                bBack?.Dispose();
            if (!fText.Equals(this.Font))
                fText?.Dispose();
            int lastHeight = (int)Math.Ceiling(loc.Y + lineHeight);
            if (_MsgIcon != MessageBoxIcon.None)
            {
                lastHeight = Math.Max(lastHeight, IconSize.Height);
            }
            return new Size(_CanvasMaxWidth, lastHeight);
        }
        #endregion

        #region Private Method : PointF DrawWrapCsiSgrText(Graphics g, string txt, Font font, Brush bFore, Brush bBack, PointF loc)
        private PointF DrawWrapCsiSgrText(Graphics g, string txt, Font font, Brush bFore, Brush bBack, PointF loc)
        {
            int idx = 0;
            SizeF f1 = Size.Empty, f2 = Size.Empty;
            string tmp, st = string.Empty;
            MatchCollection mc;
            RectangleF tRect;
            PointF backOffset = new PointF(0, 0);
            string[] lines = null;
            float lineHeight = g.MeasureString("@", font, Point.Empty, _DefFormat).Height;
            txt = txt.Replace("\r\n", "\n").Replace("\r", "\n");
            float left = 0;
            if (_MsgIcon != MessageBoxIcon.None)
                left = IconSize.Width + btn1.Margin.Left;

            #region 處理行首的換行符號
            while (txt.StartsWith("\n"))
            {
                loc.X = left;
                loc.Y += lineHeight;
                txt = txt.Substring(1);
            }
            #endregion

            if (txt.IndexOf("\n") != -1)
            {
                #region 處理多行文字
                lines = txt.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrEmpty(lines[i]))
                    {
                        loc.X = left;
                        loc.Y += lineHeight;
                    }
                    else
                    {
                        loc = DrawWrapCsiSgrText(g, lines[i], font, bFore, bBack, loc);
                        _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
                        if (i < lines.Length - 1)
                        {
                            loc.X = left;
                            loc.Y += lineHeight;
                        }
                    }
                }
                #endregion
            }
            else
            {
                #region 處理單行文字
                // 右邊剩下的空間 : _MaxTextWidth - loc.X
                f1 = g.MeasureString(txt, font, loc, _DefFormat);
                if (_MaxTextWidth - loc.X > f1.Width)
                {
                    #region 文字需要的大小，小於等於剩餘空間
                    tRect = new RectangleF(loc, f1);
                    tRect.Offset(backOffset);
                    g.FillRectangle(bBack, tRect);
                    g.DrawString(txt, font, bFore, loc, _DefFormat);
                    loc.X += f1.Width;
                    _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
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
                                if (f2.Width + loc.X > _MaxTextWidth)
                                {
                                    // 加上字串會超過右限，先繪製之前的字串
                                    f2 = g.MeasureString(tmp, font, PointF.Empty, _DefFormat);
                                    tRect = new RectangleF(loc, f2);
                                    tRect.Offset(backOffset);
                                    g.FillRectangle(bBack, tRect);
                                    g.DrawString(tmp, font, bFore, loc, _DefFormat);
                                    _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X + f2.Width), _CanvasMaxWidth);
                                    loc.X = left;
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
                            if (f2.Width + loc.X > _MaxTextWidth)
                            {
                                // 加上字串會超過右限，先繪製之前的字串
                                f2 = g.MeasureString(tmp, font, PointF.Empty, _DefFormat);
                                tRect = new RectangleF(loc, f2);
                                tRect.Offset(backOffset);
                                g.FillRectangle(bBack, tRect);
                                g.DrawString(tmp, font, bFore, loc, _DefFormat);
                                _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X + f2.Width), _CanvasMaxWidth);
                                loc.X = left;
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
                        {
                            loc = DrawWrapCsiSgrText(g, tmp, font, bFore, bBack, loc);
                            _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
                        }
                        #region 剩下的字串
                        if (idx < txt.Length)
                        {
                            st = txt.Substring(idx, txt.Length - idx);
                            loc = DrawWrapCsiSgrText(g, st, font, bFore, bBack, loc);
                            _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
                        }
                        #endregion
                        #endregion
                    }
                    else
                    {
                        #region 處理不含符號的字串，即單純的文字
                        f1 = g.MeasureString(txt, font, loc, _DefFormat);
                        if ((Regex.IsMatch(txt, "^[A-Za-z]+$") || Regex.IsMatch(txt, "^[0-9]+$")) && (f1.Width + loc.X < _MaxTextWidth))
                        {
                            // 純英文字串或純數字字串且長度小於剩餘空間
                            f1 = g.MeasureString(txt, font, loc, _DefFormat);
                            tRect = new RectangleF(loc, f1);
                            tRect.Offset(backOffset);
                            g.FillRectangle(bBack, tRect);
                            g.DrawString(txt, font, bFore, loc, _DefFormat);
                            _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X + f1.Width), _CanvasMaxWidth);
                            loc.X += f1.Width;
                        }
                        else
                        {
                            // 非純英數字或超長純英數字
                            for (int i = 0; i < txt.Length; i++)
                            {
                                f1 = g.MeasureString(txt.Substring(idx, i + 1 - idx), font, loc, _DefFormat);
                                if (f1.Width + loc.X > _MaxTextWidth)
                                {
                                    f1 = g.MeasureString(txt.Substring(idx, i - idx), font, loc, _DefFormat);
                                    tRect = new RectangleF(loc, f1);
                                    tRect.Offset(backOffset);
                                    g.FillRectangle(bBack, tRect);
                                    g.DrawString(txt.Substring(idx, i - idx), font, bFore, loc, _DefFormat);
                                    _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X + f1.Width), _CanvasMaxWidth);
                                    loc.X = left;
                                    loc.Y += lineHeight;
                                    idx = i;
                                }
                            }
                            if (idx <= txt.Length - 1)
                            {
                                f1 = g.MeasureString(txt.Substring(idx), font, loc, _DefFormat);
                                tRect = new RectangleF(loc, f1);
                                tRect.Offset(backOffset);
                                g.FillRectangle(bBack, tRect);
                                g.DrawString(txt.Substring(idx), font, bFore, loc, _DefFormat);
                                loc.X += f1.Width;
                                _CanvasMaxWidth = Math.Max((int)Math.Ceiling(loc.X), _CanvasMaxWidth);
                            }
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
            if (DpiGain == 0)
                SetDpiGain();
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
                    if (!Application.RenderWithVisualStyles)
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                    }
                    Application.Run(dg);
                    res = dg.DialogResult;
                }
                return res;
            }
        }
        #endregion

        #region Private Static Method :void SetDpiGain()
        private static void SetDpiGain()
        {
            DpiGain = PrimaryScreen.ScaleX;
            ButtonSize = DEF_BUTTON_SIZE.Gain(DpiGain).ToSize();
            ButtonPanelPadding = DEF_BUTTON_PANEL_PADDING.Gain(DpiGain);
            ButtonPanelSize = new Size(0, ButtonSize.Height + ButtonPanelPadding.Vertical);
            IconSize = DEF_ICON_SIZE.Gain(DpiGain).ToSize();
            ButtonMargin = DEF_BUTTON_MARGIN.Gain(DpiGain);
            Canvas_Padding = DEF_CANVAS_PADDING.Gain(DpiGain);
        }
        #endregion

        #region Protected Override Method : void Dispose(bool disposing)
        /// <summary>Clean up any resources being used.</summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                _Buffer?.Dispose();
                _Icon?.Dispose();
                _DefFormat?.Dispose();
                btn1?.Dispose();
                btn2?.Dispose();
                btn3?.Dispose();
                flpButtons?.Dispose();
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion

        #region Private Method : void InitializeComponent()
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
            this.btn3.Location = new System.Drawing.Point(211, 10);
            this.btn3.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btn3.Name = "btn3";
            this.btn3.Size = new System.Drawing.Size(90, 30);
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
            this.flpButtons.Location = new System.Drawing.Point(0, 110);
            this.flpButtons.Margin = new System.Windows.Forms.Padding(1);
            this.flpButtons.Name = "flpButtons";
            this.flpButtons.Padding = new System.Windows.Forms.Padding(10, 10, 0, 10);
            this.flpButtons.Size = new System.Drawing.Size(311, 50);
            this.flpButtons.TabIndex = 0;
            this.flpButtons.WrapContents = false;
            // 
            // btn2
            // 
            this.btn2.Location = new System.Drawing.Point(113, 10);
            this.btn2.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(90, 30);
            this.btn2.TabIndex = 1;
            this.btn2.Text = "否(&N)";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Click += new System.EventHandler(this.Buttons_Click);
            // 
            // btn1
            // 
            this.btn1.Location = new System.Drawing.Point(15, 10);
            this.btn1.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(90, 30);
            this.btn1.TabIndex = 0;
            this.btn1.Text = "是(&Y)";
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Click += new System.EventHandler(this.Buttons_Click);
            // 
            // MsgDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(311, 160);
            this.Controls.Add(this.flpButtons);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MsgDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
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

        #region Public Static Property : Size DialogMaximumSize(R/W)
        /// <summary>設定或取得視窗最大範圍</summary>
        public static Size DialogMaximumSize
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

