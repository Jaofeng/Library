using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace CJF.Utility.Ansi
{
    #region Public Enum : SgrColors(ushort)
    /// <summary>文字前景與背景顏色的列舉。</summary>
    [Serializable]
    public enum SgrColors : ushort
    {
        /// <summary>無、未定義或清除顏色。</summary>
        None = 0,
        /// <summary>Color Code = 30, 黑色。</summary>
        Black = 30,
        /// <summary>Color Code = 31, 深紅色。</summary>
        DarkRed = 31,
        /// <summary>Color Code = 32, 深綠色。</summary>
        DarkGreen = 32,
        /// <summary>Color Code = 33, 深黃色。</summary>
        DarkYellow = 33,
        /// <summary>Color Code = 34, 深藍色。</summary>
        DarkBlue = 34,
        /// <summary>Color Code = 35, 深洋紅色 (深紫紅色)。</summary>
        DarkMagenta = 35,
        /// <summary>Color Code = 36, 深青色 (深藍綠色)。</summary>
        DarkCyan = 36,
        /// <summary>Color Code = 37, 灰色。</summary>
        Gray = 37,
        /// <summary>Color Code = 90, 亮黑（深灰色）。</summary>
        DarkGray = 90,
        /// <summary>Color Code = 91, 亮紅色。</summary>
        Red = 91,
        /// <summary>Color Code = 92, 亮綠色。</summary>
        Green = 92,
        /// <summary>Color Code = 93, 亮黃色。</summary>
        Yellow = 93,
        /// <summary>Color Code = 94, 亮藍色。</summary>
        Blue = 94,
        /// <summary>Color Code = 95, 亮洋紅色 (亮紫紅色)。</summary>
        Magenta = 95,
        /// <summary>Color Code = 96, 亮青色 (亮藍綠色)。</summary>
        Cyan = 96,
        /// <summary>Color Code = 97, 白色。</summary>
        White = 97
    }
    #endregion

    #region Public Enum : CsiCommands
    /// <summary>控制指令列舉</summary>
    [Serializable]
    public enum CsiCommands
    {
        /// <summary>非 CSI 指令，或不支援該指令。</summary>
        None,
        /// <summary>CSI n A(CUU)：游標上移（Cursor Up）。</summary>
        CursorUp,
        /// <summary>CSI n B(CUD)：游標下移（Cursor Down）。</summary>
        CursorDown,
        /// <summary>CSI n C(CUF)：游標前移（Cursor Forward）。</summary>
        CursorForward,
        /// <summary>CSI n D(CUB)：游標後移（Cursor Back）。</summary>
        CursorBack,
        /// <summary>CSI n E(CNL)：游標移到下一行（Cursor Next Line）；游標移動到下面第 {n}（預設1）行的開頭。</summary>
        CursorNextLine,
        /// <summary>CSI n F(CPL)：游標移到上一行（Cursor Previous Line）；游標移動到上面第 {n}（預設1）行的開頭。</summary>
        CursorPrevLine,
        /// <summary>CSI n G(CHA)：游標水平絕對（Cursor Horizontal Absolute）；游標移動到第 {n} （預設1）列。</summary>
        CursorHorizontalAbsolute,
        /// <summary>CSI n;m H(CUP)：游標位置（Cursor Position）。游標移動到第 n 行、第 m 列。值從1開始，且預設為 1（左上角）。</summary>
        CursorPosition,
        /// <summary>CSI n J(ED)：擦除顯示（Erase in Display）。清除螢幕的部分割域。n=2 使用的是清除整個螢幕。</summary>
        Cls,
        /// <summary>CSI n J(ED)：擦除顯示（Erase in Display）。清除螢幕的部分割域。n=3，則清除整個螢幕，並刪除回滾快取區中的所有行。</summary>
        ClsHistory,
        /// <summary>CSI n m(SGR)：清除 SGR 設定，n=0 等同於清除顏色設定。</summary>
        ResetSGR,
        /// <summary>CSI n[;m] m(SGR)：選擇圖形再現（Select Graphic Rendition）。CSI 後可以是 0 或者更多參數，用分號分隔。如果沒有參數，則視為CSI 0 m（等於 ResetColor）。</summary>
        SGR,
        /// <summary>CSI n K(EL)：擦除行（Erase in Line）清除行內的部分割域。n=0（或缺失），清除從游標位置到該行末尾的部分。</summary>
        EraseToStart,
        /// <summary>CSI n K(EL)：擦除行（Erase in Line）清除行內的部分割域。n=1，清除從游標位置到該行開頭的部分。</summary>
        EraseToEnd,
        /// <summary>CSI n K(EL)：擦除行（Erase in Line）清除行內的部分割域。n=2，清除整行。游標位置不變。</summary>
        EraseLine,
        /// <summary>CSI s(SCP)：儲存游標當前位置（Save Cursor Position）。</summary>
        SaveCursorPosition,
        /// <summary>CSI u(RCP)：恢復游標位置（Restore Cursor Position）</summary>
        RestoreCursorPosition,
        /// <summary>CSI n S(SU)：向上捲動（Scroll Up）。整頁向上捲動 n（預設1）行。新行添加到底部。</summary>
        ScrollUp,
        /// <summary>CSI n T(SD)：向下捲動（Scroll Down）。整頁向下捲動 n（預設1）行。新行添加到頂部。</summary>
        ScrollDown,
        /// <summary>錯誤指令參數。</summary>
        ErrorCode
    }
    #endregion

    #region Public Static Class : class Extensions
    /// <summary>擴充函示</summary>
    public static class Extensions
    {
        #region Public Static Method : Color ToColor(this SgrColors sgr)
        /// <summary>將 SGR 顏色轉為 System.Drawing.Color 結構類別。</summary>
        /// <param name="sgr">SGR 顏色值。</param>
        /// <returns>System.Drawing.Color 結構類別。</returns>
        /// <exception cref="System.NotSupportedException">未支援該 SGR 顏色值。</exception>
        public static Color ToColor(this SgrColors sgr)
        {
            switch (sgr)
            {
                case SgrColors.Black: return Color.Black;
                case SgrColors.DarkRed: return Color.DarkRed;
                case SgrColors.DarkGreen: return Color.DarkGreen;
                case SgrColors.DarkYellow: return Color.Olive;
                case SgrColors.DarkBlue: return Color.DarkBlue;
                case SgrColors.DarkMagenta: return Color.DarkMagenta;
                case SgrColors.DarkCyan: return Color.DarkCyan;
                case SgrColors.Gray: return Color.DarkGray;
                case SgrColors.DarkGray: return Color.Gray;
                case SgrColors.Red: return Color.Red;
                case SgrColors.Green: return Color.Green;
                case SgrColors.Yellow: return Color.Yellow;
                case SgrColors.Blue: return Color.Blue;
                case SgrColors.Magenta: return Color.Magenta;
                case SgrColors.Cyan: return Color.Cyan;
                case SgrColors.White: return Color.White;
                case SgrColors.None: return Color.Transparent;
                default:
                    throw new NotSupportedException();
            }
        }
        #endregion
    }
    #endregion

    /// <summary>表示可變動的 ANSI CSI 字串。這個類別 (Class) 無法被繼承。</summary>
    [Serializable]
    [ComVisible(true)]
    public sealed class CsiBuilder : IEnumerator
    {
        #region Public Consts
        /// <summary>用於判斷 ASNI CSI 的規則運算式字串。</summary>
        public const string PATTERN = "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?([ABCDEFGHJKSTfmsu])";
        /// <summary>用於判斷非 ANSI CSI SGR 的規則運算式字串。</summary>
        public const string ONLY_SGR = "\x1B\\[(\\d+)*;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?;?(\\d+)?([ABCDEFGHJKSTfsu])";
        /// <summary>ANSI CSI 前置字串。</summary>
        public const string CSI_PREFIX = "\x1B[";
        #endregion

        private List<string> _Items = null;

        #region Public Construct Method : CsiBuilder()
        /// <summary>建立一個新的 CJF.Utility.Ansi.CsiBuilder 執行個體</summary>
        public CsiBuilder() : this(string.Empty) { }
        #endregion

        #region Public Construct Method : CsiBuilder(string text)
        /// <summary>以 ANSI CSI 字串，建立一個新的 CJF.Utility.Ansi.CsiBuilder 執行個體</summary>
        /// <param name="text">欲解析的 ANSI CSI 字串。</param>
        public CsiBuilder(string text)
        {
            _Items = new List<string>();
            if (!string.IsNullOrEmpty(text))
            {
                _Items.AddRange(GetCsiArray(text));
                SetCharLength();
            }
        }
        #endregion

        #region Public Construct Method : CsiBuilder(CsiBuilder source)
        /// <summary>利用現有的 CJF.Utility.Ansi.CsiBuilder 執行個體複製一個新的執行個體。</summary>
        /// <param name="source">欲複製的執行個體。</param>
        public CsiBuilder(CsiBuilder source)
        {
            _Items = new List<string>();
            _Items.AddRange(source._Items);
            SetCharLength();
        }
        #endregion

        #region Public Property : string this[int index](R/W)
        /// <summary>取得或設定這個執行個體中指定字串位置的字串。</summary>
        /// <param name="index">分割字串的索引位置。</param>
        /// <returns>位置 index 上的字串。</returns>
        /// <exception cref="IndexOutOfRangeException">取得字串時，index 在這個執行個體的界限外。</exception>
        /// <exception cref="ArgumentOutOfRangeException">設定字串時，index 在這個執行個體的界限外。</exception>
        public string this[int index]
        {
            get
            {
                if (index < 0 || index >= _Items.Count)
                    throw new IndexOutOfRangeException();
                return _Items[index];
            }
            set
            {
                if (index < 0 || index >= _Items.Count)
                    throw new ArgumentOutOfRangeException();
                _Items[index] = value;
                SetCharLength();
            }
        }
        #endregion

        #region Public Properties
        /// <summary>取得這個執行個體的字串長度。一個 CSI 指令為 1。</summary>
        public int CharLength { get; private set; } = 0;
        /// <summary>取得這個執行個體的字串長度。包含 CSI 指令完整長度。</summary>
        public int Length { get { return this.ToString().Length; } }
        /// <summary>取得這個執行個體的字串個數。</summary>
        public int Count { get => _Items.Count; }
        object IEnumerator.Current => null;
        #endregion

        #region Public Method : void Append(string text)
        /// <summary>新增文字到字串後面。</summary>
        /// <param name="text">欲新增的文字</param>
        public void Append(string text)
        {
            string[] arr = GetCsiArray(text);
            if (arr.Length == 1)
            {
                _Items.Add(arr[0]);
                CharLength += arr[0].Length;
            }
            else
            {
                _Items.AddRange(arr);
                SetCharLength();
            }
        }
        #endregion

        #region Public Method : void Append(SgrColors fore)
        /// <summary>將文字前景顏色(SGR)指令放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        public void Append(SgrColors fore)
        {
            _Items.Add($"\x1B[{(ushort)fore}m");
            CharLength++;
        }
        #endregion

        #region Public Method : void Append(SgrColors fore, string text, bool resetColor = false)
        /// <summary>將文字前景顏色(SGR)指令與文字放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="text">欲新增的文字。</param>
        /// <param name="resetColor">文字後方是否加上清除顏色的指令。</param>
        public void Append(SgrColors fore, string text, bool resetColor = false)
        {
            Append(fore);
            Append(text);
            if (resetColor)
                Append(SgrColors.None);
        }
        #endregion

        #region Public Method : void Append(SgrColors fore, SgrColors back)
        /// <summary>將文字前景顏色、背景顏色(SGR)指令放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="back">背景顏色值。</param>
        public void Append(SgrColors fore, SgrColors back)
        {
            _Items.Add($"\x1B[{(ushort)fore};{((ushort)back + 10)}m");
            CharLength++;
        }
        #endregion

        #region Public Method : void Append(SgrColors fore, SgrColors back, string text, bool resetColor = false)
        /// <summary>將文字前景顏色、背景顏色(SGR)指令與文字放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="back">背景顏色值。</param>
        /// <param name="text">欲新增的文字。</param>
        /// <param name="resetColor">文字後方是否加上清除顏色的指令。</param>
        public void Append(SgrColors fore, SgrColors back, string text, bool resetColor = false)
        {
            Append(fore, back);
            Append(text);
            if (resetColor)
                Append(SgrColors.None);
        }
        #endregion

        #region Public Method : void AppendFormat(string format, params object[] args)
        /// <summary>以指定之陣列中對應物件的字串表示，取代指定之字串中的格式項目，並新增到字串後面。</summary>
        /// <param name="format">複合格式字串。</param>
        /// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
        public void AppendFormat(string format, params object[] args)
        {
            Append(string.Format(format, args));
        }
        #endregion

        #region Public Method : void AppendFormat(SgrColors fore, string format, params object[] args)
        /// <summary>將文字前景顏色(SGR)指令與格式化文字放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="format">複合格式字串。</param>
        /// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
        public void AppendFormat(SgrColors fore, string format, params object[] args)
        {
            Append(fore);
            AppendFormat(format, args);
        }
        #endregion

        #region Public Method : void AppendFormat(SgrColors fore, SgrColors back, string format, params object[] args)
        /// <summary>將文字前景顏色、背景顏色(SGR)指令與格式化文字放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="back">背景顏色值。</param>
        /// <param name="format">複合格式字串。</param>
        /// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
        public void AppendFormat(SgrColors fore, SgrColors back, string format, params object[] args)
        {
            Append(fore, back);
            AppendFormat(format, args);
        }
        #endregion

        #region Public Method : void AppendLine()
        /// <summary>將換行符號放入字串後面。</summary>
        public void AppendLine()
        {
            Append(Environment.NewLine);
        }
        #endregion

        #region Public Method : void AppendLine(string text)
        /// <summary>將文字與換行符號放入字串後面。</summary>
        /// <param name="text">欲新增的文字。</param>
        public void AppendLine(string text)
        {
            Append(text + Environment.NewLine);
        }
        #endregion

        #region Public Method : void AppendLine(SgrColors fore, string text, bool resetColor = false)
        /// <summary>將文字前景顏色(SGR)指令、文字與換行符號放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="text">欲新增的文字。</param>
        /// <param name="resetColor">文字後方是否加上清除顏色的指令。</param>
        public void AppendLine(SgrColors fore, string text, bool resetColor = false)
        {
            Append(fore, text + Environment.NewLine);
            if (resetColor)
                Append(SgrColors.None);
        }
        #endregion

        #region Public Method : void AppendLine(SgrColors fore, SgrColors back, string text, bool resetColor = false)
        /// <summary>將文字前景顏色、背景顏色(SGR)指令與文字放入字串後面。</summary>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="back">背景顏色值。</param>
        /// <param name="text">欲新增的文字。</param>
        /// <param name="resetColor">文字後方是否加上清除顏色的指令。</param>
        public void AppendLine(SgrColors fore, SgrColors back, string text, bool resetColor = false)
        {
            Append(fore, back, text + Environment.NewLine);
            if (resetColor)
                Append(SgrColors.None);
        }
        #endregion

        #region Public Method : void AppendCommand(Commands cmd, params object[] args)
        /// <summary>將控制指令放入字串後方。</summary>
        /// <param name="cmd">控制指令列舉值。</param>
        /// <param name="args">附屬的參數值。</param>
        /// <exception cref="System.ArgumentNullException">所用的指令必須一併傳入附屬參數值。</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">傳入附屬參數數量不符合所用的指令。</exception>
        public void AppendCommand(CsiCommands cmd, params object[] args)
        {
            _Items.Add(GetCsiString(cmd, args));
            CharLength++;
        }
        #endregion

        #region Public Method : void Insert(int charIndex, string text)
        /// <summary>在指定的字元位置，將所指定的文字字串插入這個執行個體中。</summary>
        /// <param name="charIndex">這個執行個體中開始插入的字元位置。</param>
        /// <param name="text">要插入的文字字串。</param>
        /// <exception cref="ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
        public void Insert(int charIndex, string text)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            else if (charIndex == CharLength)
            {
                Append(text);
                return;
            }
            string[] list = null;
            bool isCsi = IsCsiString(text);
            if (isCsi)
                list = GetCsiArray(text);
            int idx = 0;
            for (int i = 0; i < _Items.Count; i++)
            {
                if (idx == charIndex)
                {
                    if (isCsi)
                        _Items.InsertRange(i, list);
                    else
                    {
                        if (_Items[i].StartsWith(CSI_PREFIX))
                            _Items.Insert(i, text);
                        else
                            _Items[i] = _Items[i].Insert(0, text);
                    }
                    break;
                }
                else
                {
                    if (_Items[i].StartsWith(CSI_PREFIX))
                    {
                        idx++;
                    }
                    else
                    {
                        if (charIndex < idx + _Items[i].Length)
                        {
                            if (isCsi)
                            {
                                _Items.Insert(i, _Items[i]);
                                _Items[i] = _Items[i].Substring(0, charIndex - idx);
                                _Items[i + 1] = _Items[i + 1].Substring(charIndex - idx, _Items[i + 1].Length - (charIndex - idx));
                                _Items.InsertRange(i + 1, list);
                            }
                            else
                            {
                                _Items[i] = _Items[i].Insert(charIndex - idx, text);
                            }
                            break;
                        }
                        else
                            idx += _Items[i].Length;
                    }
                }
            }
            SetCharLength();
        }
        #endregion

        #region Public Method : void Insert(int charIndex, SgrColors fore)
        /// <summary>在指定的字元位置，將文字前景顏色(SGR)指令插入這個執行個體中。</summary>
        /// <param name="charIndex">這個執行個體中開始插入的位置。</param>
        /// <param name="fore">前景顏色值。</param>
        /// <exception cref="ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
        public void Insert(int charIndex, SgrColors fore)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            InsertCommand(charIndex, CsiCommands.SGR, (ushort)fore);
        }
        #endregion

        #region Public Method : void Insert(int charIndex, SgrColors fore, string text)
        /// <summary>在指定的字元位置，將文字前景顏色(SGR)指令、文字字串插入這個執行個體中。</summary>
        /// <param name="charIndex">這個執行個體中開始插入的位置。</param>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="text">要插入的文字字串。</param>
        /// <exception cref="ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
        public void Insert(int charIndex, SgrColors fore, string text)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            Insert(charIndex, GetCsiString(CsiCommands.SGR, (ushort)fore) + text);
        }
        #endregion

        #region Public Method : void Insert(int charIndex, SgrColors fore, SgrColors back)
        /// <summary>在指定的字元位置，將文字前景顏色、背景顏色(SGR)指令插入這個執行個體中。</summary>
        /// <param name="charIndex">這個執行個體中開始插入的位置。</param>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="back">背景顏色值。</param>
        /// <exception cref="ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
        public void Insert(int charIndex, SgrColors fore, SgrColors back)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            Insert(charIndex, GetCsiString(CsiCommands.SGR, (ushort)fore, (ushort)back + 10));
        }
        #endregion

        #region Public Method : void Insert(int charIndex, SgrColors fore, SgrColors back, string text)
        /// <summary>在指定的字元位置，將文字前景顏色、背景顏色(SGR)指令、文字字串插入這個執行個體中。</summary>
        /// <param name="charIndex">這個執行個體中開始插入的位置。</param>
        /// <param name="fore">前景顏色值。</param>
        /// <param name="back">背景顏色值。</param>
        /// <param name="text">要插入的文字字串。</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
        public void Insert(int charIndex, SgrColors fore, SgrColors back, string text)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            Insert(charIndex, GetCsiString(CsiCommands.SGR, (ushort)fore, (ushort)back + 10) + text);
        }
        #endregion

        #region Public Method : void InsertCommand(int charIndex, CsiCommands cmd, params object[] args)
        /// <summary>在指定的字元位置，將控制指令插入這個執行個體中。</summary>
        /// <param name="charIndex">這個執行個體中開始插入的位置。</param>
        /// <param name="cmd">ANSI CSI 控制指令列舉值。</param>
        /// <param name="args">附屬的參數值。</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。- 或 -傳入附屬參數數量不符合所用的指令。</exception>
        /// <exception cref="System.ArgumentNullException">所用的指令必須一併傳入附屬參數值。</exception>
        public void InsertCommand(int charIndex, CsiCommands cmd, params object[] args)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            Insert(charIndex, GetCsiString(cmd, args));
        }
        #endregion

        #region Public Method : int[] IndexesOf()
        /// <summary>取得這個執行個體中所有 ANSI CSI 指令的元素字元位置。</summary>
        /// <returns>所有 ANSI CSI 指令的元素字元位置清單。</returns>
        public int[] IndexesOf()
        {
            List<int> idxs = new List<int>();
            int idx = 0;
            foreach (string s in _Items)
            {
                if (s.StartsWith(CSI_PREFIX))
                {
                    idxs.Add(idx);
                    idx++;
                }
                else
                {
                    idx += s.Length;
                }
            }
            return idxs.ToArray();
        }
        #endregion

        #region Public Method : int IndexOf(CsiCommands cmd, params object[] args)
        /// <summary>自字元位置 0 開始搜尋指定的 ANSI CSI 指令，並傳回整個執行個體中第一個相符項目之以零起始的字元位置索引。</summary>
        /// <param name="cmd">欲搜尋的 ANSI CSI 指令。</param>
        /// <param name="args">附屬的參數值。</param>
        /// <returns>如果有找到，則是在整個執行個體內，第一個相符項目的以零起始的字元位置索引，否則為 -1。</returns>
        public int IndexOf(CsiCommands cmd, params object[] args)
        {
            return this.IndexOf(0, cmd, args);
        }
        #endregion

        #region Public Method : int IndexOf(int charIndex, CsiCommands cmd, params object[] args)
        /// <summary>在這個執行個體中從指定的字元位置索引開始到最後一個項目這段範圍內，搜尋指定的 ANSI CSI 指令第一次出現的位置，並傳回其索引值(索引以零起始)。</summary>
        /// <param name="charIndex">搜尋之以零起始的起始索引。0 (零) 在空白清單中有效。</param>
        /// <param name="cmd">欲搜尋的 ANSI CSI 指令。</param>
        /// <param name="args">附屬的參數值。</param>
        /// <returns>如果有找到，則是在整個執行個體內，第一個相符項目的以零起始的索引，否則為 -1。</returns>
        /// <exception cref="ArgumentOutOfRangeException">index 在這個執行個體的有效索引範圍之外。 - 或 - 傳入附屬參數數量不符合所用的指令。</exception>
        /// <exception cref="ArgumentNullException">所用的指令必須一併傳入附屬參數值。</exception>
        public int IndexOf(int charIndex, CsiCommands cmd, params object[] args)
        {
            if (charIndex < 0 || charIndex > CharLength)
                throw new ArgumentOutOfRangeException();
            string sk = string.Empty;
            try { sk = GetCsiString(cmd, args); }
            catch (Exception e) { throw e; }
            string text = ToString();
            int idx = text.IndexOf(sk, charIndex);
            if (idx == -1)
                return -1;
            else
            {
                string[] arr = GetCsiArray(text.Substring(0, idx));
                idx = 0;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (arr[i].StartsWith(CSI_PREFIX))
                        idx++;
                    else
                        idx += arr[i].Length;
                }
                return idx;
            }
        }
        #endregion

        #region Public Method : bool IsCsiString(int index)
        /// <summary>取得這個執行個體的元素字串是否為 ANSI CSI 字串。</summary>
        /// <returns>true:該元素為 ANSI CSI 字串；false:不為 ANSI CSI 字串。</returns>
        public bool IsCsiString()
        {
            return (_Items.Count) != 0 && _Items.Exists(s => s.StartsWith(CSI_PREFIX));
        }
        #endregion

        #region Public Method : void CopyTo(string[] array) - Remarked
        ///// <summary>將整個執行個體複製到相容的一維陣列，從目標陣列的開頭開始。</summary>
        ///// <param name="array">一維字串陣列，是從這個執行個體複製過來之元素的目的端。array 必須有以零起始的索引。</param>
        ///// <exception cref="System.ArgumentNullException">array 為 null。</exception>
        ///// <exception cref="System.ArgumentException">這個執行個體中的元素數目大於目的 array 可包含的元素數目。</exception>
        //public void CopyTo(string[] array)
        //{
        //    if (array == null)
        //        throw new ArgumentNullException();
        //    else if (_Items.Count > array.Length)
        //        throw new ArgumentException();
        //    _Items.CopyTo(array);
        //}
        #endregion

        #region Public Method : void CopyTo(string[] array, int arrayIndex) - Remarked
        ///// <summary>從目標陣列的指定索引處開始，將整個執行個體複製到相容的一維陣列中。</summary>
        ///// <param name="array">一維字串陣列，是從這個執行個體複製過來之元素的目的端。array 必須有以零起始的索引。</param>
        ///// <param name="arrayIndex">array 中以零起始的索引，是開始複製的位置。</param>
        ///// <exception cref="System.ArgumentNullException">array 為 null。</exception>
        ///// <exception cref="System.ArgumentOutOfRangeException">arrayIndex 小於 0。</exception>
        ///// <exception cref="System.ArgumentException">這個執行個體的元素數目，超過從 arrayIndex 到目的 array 末尾之間的可用空間。</exception>
        //public void CopyTo(string[] array, int arrayIndex)
        //{
        //    if (array == null)
        //        throw new ArgumentNullException();
        //    else if (arrayIndex < 0)
        //        throw new ArgumentOutOfRangeException();
        //    else if (_Items.Count > array.Length - arrayIndex)
        //        throw new ArgumentException();
        //    _Items.CopyTo(array, arrayIndex);
        //}
        #endregion

        #region Public Method : void CopyTo(int index, string[] array, int arrayIndex, int count) - Remarked
        ///// <summary>從目標陣列的指定索引處開始，將元素範圍從這個執行個體複製到相容的一維陣列。</summary>
        ///// <param name="index">來源這個執行個體中以零起始的索引，是開始複製的位置。</param>
        ///// <param name="array">一維字串陣列，必須有以零起始的索引。</param>
        ///// <param name="arrayIndex">array 中以零起始的索引，是開始複製的位置。</param>
        ///// <param name="count">要複製的字串數目。</param>
        ///// <exception cref="System.ArgumentNullException">array 為 null。</exception>
        ///// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -arrayIndex 小於 0。- 或 -count 小於等於 0。</exception>
        ///// <exception cref="System.ArgumentException">
        ///// <para>index 等於或大於這個執行個體的數量。</para>
        ///// <para>- 或 -從 index 到這個執行個體末尾的元素數目，超過從 arrayIndex 到目的 array 末尾之間的可用空間。</para>
        ///// </exception>
        //public void CopyTo(int index, string[] array, int arrayIndex, int count)
        //{
        //    if (array == null)
        //        throw new ArgumentNullException();
        //    else if (index < 0 || arrayIndex < 0 || count <= 0)
        //        throw new ArgumentOutOfRangeException();
        //    else if (index >= _Items.Count || index + count > array.Length - arrayIndex)
        //        throw new ArgumentException();
        //    _Items.CopyTo(index, array, arrayIndex, count);
        //}
        #endregion

        #region Public Method : void RemoveAt(int index) - Remarked
        ///// <summary> 移除這個執行個體中指定之索引處的項目。</summary>
        ///// <param name="index">要移除元素之以零起始的索引。</param>
        ///// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -index 等於或大於這個執行個體元素數量。</exception>
        //public void RemoveAt(int index)
        //{
        //    if (index < 0 || index >= _Items.Count)
        //        throw new ArgumentOutOfRangeException();
        //    RemoveRange(index, 1);
        //}
        #endregion

        #region Public Method : void RemoveRange(int startIndex, int length) - Remarked
        ///// <summary>從這個執行個體中移除的元素範圍。</summary>
        ///// <param name="index">要移除之元素範圍內之以零起始的起始索引。</param>
        ///// <param name="count">要移除的元素數目。</param>
        ///// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -count 小於 0。</exception>
        ///// <exception cref=" System.ArgumentException">index 和 count 並不代表這個執行個體中元素的有效範圍。</exception>
        //public void RemoveRange(int index, int count)
        //{
        //    if (index < 0 || count < 0)
        //        throw new ArgumentOutOfRangeException();
        //    else if (index >= _Items.Count || count > _Items.Count || _Items.Count - index < count)
        //        throw new ArgumentException();
        //    _Items.RemoveRange(index, count);
        //}
        #endregion

        #region Public Method : CsiCommands GetCommandType(int index) - Remarked
        ///// <summary>取得這個執行個體中指定字元元素的 ANSI CSI 指令型態。</summary>
        ///// <param name="charIndex">元素字串的位置。</param>
        ///// <returns>元素的 ANSI CSI 指令型態。</returns>
        ///// <exception cref="System.IndexOutOfRangeException">index 小於 0。- 或 -index 大於等於元素數量。</exception>
        //public CsiCommands GetCommandType(int charIndex)
        //{
        //    if (charIndex < 0 || charIndex >= CharLength)
        //        throw new IndexOutOfRangeException();
        //    return GetCommandType(_Items[charIndex]);
        //}
        #endregion

        #region Public Method : void Clear()
        /// <summary>清除這個執行個體中所有字串內容。</summary>
        public void Clear()
        {
            _Items.Clear();
        }
        #endregion

        #region Public Method : string ToPureText()
        /// <summary>將這個執行個體的值轉換為不包含 ANSI CSI 的純文字字串。</summary>
        /// <returns>不包含 ANSI CSI 的純文字字串。</returns>
        public string ToPureText()
        {
            return string.Join<string>("", _Items.FindAll(s => !s.StartsWith(CSI_PREFIX)));
        }
        #endregion

        #region Public Override Method : string ToString()
        /// <summary>將這個執行個體的值轉換為 System.String。</summary>
        /// <returns>其值和這個執行個體相同的字串。</returns>
        [SecuritySafeCritical]
        public override string ToString()
        {
            return string.Join<string>("", _Items);
        }
        #endregion

        #region Internal Method : string[] ToArray()
        /// <summary>將這個執行個體的字串元素複製到新的陣列。</summary>
        /// <returns>陣列，包含這個執行個體的項目複本。</returns>
        internal string[] ToArray()
        {
            return _Items.ToArray();
        }
        #endregion

        #region Internal Method : string[] ToPureTextArray()
        /// <summary>將這個執行個體中未包含 ANSI CSI 指令碼的字串元素複製到新的陣列。</summary>
        /// <returns>陣列，包含這個執行個體的項目複本。</returns>
        internal string[] ToPureTextArray()
        {
            return _Items.FindAll(s => !s.StartsWith(CSI_PREFIX)).ToArray();
        }
        #endregion

        #region IEnumerable 成員
        /// <summary>傳回會逐一查看集合的列舉程式。</summary>
        /// <returns>System.Collections.IEnumerator 物件，可用於逐一查看集合。</returns>
        public IEnumerator GetEnumerator()
        {
            return _Items.GetEnumerator();
        }
        #endregion

        #region Public Static Method : string CreateString(SgrColors fore, string text, bool resetColor = false)
        /// <summary>產生 ANSI SGR 字串。</summary>
        /// <param name="fore">前景顏色。</param>
        /// <param name="text">文字內容。</param>
        /// <param name="resetColor">文字後方是否加上清除顏色的指令。</param>
        /// <returns>ANSI SGR 字串。</returns>
        public static string CreateString(SgrColors fore, string text, bool resetColor = false)
        {
            CsiBuilder cb = new CsiBuilder(text);
            cb.InsertCommand(0, CsiCommands.SGR, (ushort)fore);
            if (resetColor)
                cb.AppendCommand(CsiCommands.ResetSGR);
            return cb.ToString();
        }
        #endregion

        #region Public Static Method : string CreateString(SgrColors fore, SgrColors back, string text, bool resetColor = false)
        /// <summary>產生 ANSI SGR 字串。</summary>
        /// <param name="fore">前景顏色。</param>
        /// <param name="back">背景顏色。</param>
        /// <param name="text">文字內容。</param>
        /// <param name="resetColor">文字後方是否加上清除顏色的指令。</param>
        /// <returns>ANSI SGR 字串。</returns>
        public static string CreateString(SgrColors fore, SgrColors back, string text, bool resetColor = false)
        {
            CsiBuilder cb = new CsiBuilder(text);
            cb.InsertCommand(0, CsiCommands.SGR, (ushort)fore, (ushort)back);
            if (resetColor)
                cb.AppendCommand(CsiCommands.ResetSGR);
            return cb.ToString();
        }
        #endregion

        #region Public Static Method : string GetCsiString(Commands cmd, params object[] args)
        /// <summary>取得控制指令的 ANSI CSI 字串。</summary>
        /// <param name="cmd">控制指令列舉值。</param>
        /// <param name="args">附屬的參數值。</param>
        /// <returns>控制指令的 ANSI CSI 字串</returns>
        /// <exception cref="System.ArgumentNullException">所用的指令必須一併傳入附屬參數值。</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">傳入附屬參數數量不符合所用的指令。</exception>
        public static string GetCsiString(CsiCommands cmd, params object[] args)
        {
            string res = CSI_PREFIX;
            switch (cmd)
            {
                case CsiCommands.Cls: res += "2J"; break;
                case CsiCommands.ClsHistory: res += "3J"; break;
                case CsiCommands.ResetSGR: res += "0m"; break;
                case CsiCommands.EraseToStart: res += "0K"; break;
                case CsiCommands.EraseToEnd: res += "1K"; break;
                case CsiCommands.EraseLine: res += "2K"; break;
                case CsiCommands.SaveCursorPosition: res += "s"; break;
                case CsiCommands.RestoreCursorPosition: res += "u"; break;
                case CsiCommands.CursorUp: res += "{0}A"; break;
                case CsiCommands.CursorDown: res += "{0}B"; break;
                case CsiCommands.CursorForward: res += "{0}C"; break;
                case CsiCommands.CursorBack: res += "{0}D"; break;
                case CsiCommands.CursorNextLine: res += "{0}E"; break;
                case CsiCommands.CursorPrevLine: res += "{0}F"; break;
                case CsiCommands.CursorHorizontalAbsolute: res += "{0}G"; break;
                case CsiCommands.ScrollUp: res += "{0}S"; break;
                case CsiCommands.ScrollDown: res += "{0}T"; break;
                case CsiCommands.CursorPosition: res += "{0};{1}H"; break;
                case CsiCommands.SGR:
                    if (args == null || args.Length == 0)
                        res += "m";
                    else
                    {
                        for (int i = 0; i < args.Length; i++)
                            res += $"{{{i}}};";
                        res = res.TrimEnd(';');
                        res += "m";
                    }
                    break;
                default: res = null; break;
            }
            int count = Regex.Matches(res, @"\{\d\}").Count;
            if (count != 0)
            {
                if (args == null)
                    throw new ArgumentNullException();
                else if (count > args.Length)
                    throw new ArgumentOutOfRangeException();
                else
                    res = string.Format(res, args);
            }
            return res;
        }
        #endregion

        #region Public Static Method : bool IsCsiString(string text)
        /// <summary>檢查 text 參數是否為 ANSI CSI 字串。</summary>
        /// <param name="text">欲檢查的字串。</param>
        /// <returns>true:該字串為 ANSI CSI 字串; false:不是 ANSI CSI 格式字串。</returns>
        public static bool IsCsiString(string text)
        {
            return Regex.IsMatch(text, PATTERN, RegexOptions.Multiline);
        }
        #endregion

        #region Public Static Method : string GetPureText(string csiText)
        /// <summary>將傳入的字串轉換為不包含 ANSI CSI 的純文字字串。</summary>
        /// <param name="csiText">欲移除 ANSI CSI 的字串。</param>
        /// <returns>不含 ANSI CSI 的字串。</returns>
        public static string GetPureText(string csiText)
        {
            return Regex.Replace(csiText, PATTERN, "");
        }
        #endregion

        #region Public Static Method : string GetOnlySGR(string csiText)
        /// <summary>將傳入的字串去除不是 ANSI CSI SGR 的字串。</summary>
        /// <param name="csiText">欲移除非 ANSI CSI SGR 的字串。</param>
        /// <returns>僅包含 ANSI CSI SGR 的字串。</returns>
        public static string GetOnlySGR(string csiText)
        {
            return Regex.Replace(csiText, ONLY_SGR, "");
        }
        #endregion

        #region Public Static Method : CsiCommands GetCommandType(string csi)
        /// <summary>取得字串的 ANSI CSI 指令型態。</summary>
        /// <param name="csi">字串內容。</param>
        /// <returns>元素的 ANSI CSI 指令型態。如不為 ANSI CSI 指令，則回傳 CsiCommands.None。</returns>
        /// <exception cref="ArgumentNullException">csi 不可為 null。</exception>
        public static CsiCommands GetCommandType(string csi)
        {
            if (csi == null)
                throw new ArgumentNullException();
            Match m = Regex.Match(csi, "^" + PATTERN);
            if (!m.Success)
                return CsiCommands.None;
            else
            {
                switch (m.Groups[6].Value)
                {
                    case "A": return CsiCommands.CursorUp;
                    case "B": return CsiCommands.CursorDown;
                    case "C": return CsiCommands.CursorForward;
                    case "D": return CsiCommands.CursorBack;
                    case "E": return CsiCommands.CursorNextLine;
                    case "F": return CsiCommands.CursorPrevLine;
                    case "G": return CsiCommands.CursorHorizontalAbsolute;
                    case "H": return CsiCommands.CursorPosition;
                    case "J":
                        if (m.Groups[1].Success)
                        {
                            switch (m.Groups[1].Value)
                            {
                                case "0":
                                case "1": return CsiCommands.None;
                                case "2": return CsiCommands.Cls;
                                case "3": return CsiCommands.ClsHistory;
                                default: return CsiCommands.ErrorCode;
                            }
                        }
                        else
                            return CsiCommands.None;
                    case "K":
                        if (m.Groups[1].Success)
                        {
                            switch (m.Groups[1].Value)
                            {
                                case "0": return CsiCommands.EraseToEnd;
                                case "1": return CsiCommands.EraseToStart;
                                case "2": return CsiCommands.EraseLine;
                                default: return CsiCommands.ErrorCode;
                            }
                        }
                        else
                            return CsiCommands.EraseToEnd;
                    case "S": return CsiCommands.ScrollUp;
                    case "T": return CsiCommands.ScrollDown;
                    case "m":
                        if (!m.Groups[1].Success || m.Groups[1].Value.Equals("0"))
                            return CsiCommands.ResetSGR;
                        else
                            return CsiCommands.SGR;
                    case "s": return CsiCommands.SaveCursorPosition;
                    case "u": return CsiCommands.RestoreCursorPosition;
                    default: return CsiCommands.None;
                }
            }
        }
        #endregion

        #region Private Method : void SetCharLength()
        /// <summary>重新計算並設定字元長度。</summary>
        private void SetCharLength()
        {
            int len = 0;
            foreach (string s in _Items)
            {
                if (s.StartsWith(CSI_PREFIX))
                    len++;
                else
                    len += s.Length;
            }
            CharLength = len;
        }
        #endregion

        #region Private Method : int GetCharLength(int toIndex)
        /// <summary>取得自 _Items 索引位置 0 開始到 toIndex 的字元長度。</summary>
        /// <param name="toIndex">最後一個 _Items 索引位置。</param>
        /// <returns>字元長度。</returns>
        private int GetCharLength(int toIndex)
        {
            int len = 0;
            for (int i = 0; i < toIndex; i++)
            {
                if (_Items[i].StartsWith(CSI_PREFIX))
                    len++;
                else
                    len += _Items[i].Length;
            }
            return len;
        }
        #endregion

        #region Private Method : string[] GetCsiArray(string text)
        private string[] GetCsiArray(string text)
        {
            List<string> list = new List<string>();
            MatchCollection mc = Regex.Matches(text, PATTERN, RegexOptions.Multiline);
            if (mc.Count == 0)
            {
                list.Add(text);
            }
            else
            {
                int idx = 0;
                foreach (Match m in mc)
                {
                    if (idx != m.Index)
                    {
                        list.Add(text.Substring(idx, m.Index - idx));
                    }
                    list.Add(m.Value);
                    idx = m.Index + m.Length;
                }
                if (idx < text.Length)
                {
                    list.Add(text.Substring(idx, text.Length - idx));
                }
            }
            return list.ToArray();
        }

        bool IEnumerator.MoveNext()
        {
            throw new NotImplementedException();
        }

        void IEnumerator.Reset()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
