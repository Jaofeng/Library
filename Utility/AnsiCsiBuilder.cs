using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace CJF.Utility.Ansi
{
	#region Public Enum : SgrColors(ushort)
	/// <summary>指定顏色的常數。</summary>
	[Serializable]
	public enum SgrColors : ushort
	{
		/// <summary>黑色。</summary>
		Black = 30,
		/// <summary>深紅色。</summary>
		DarkRed = 31,
		/// <summary>深綠色。</summary>
		DarkGreen = 32,
		/// <summary>深黃色。</summary>
		DarkYellow = 33,
		/// <summary>深藍色。</summary>
		DarkBlue = 34,
		/// <summary>深洋紅色 (深紫紅色)。</summary>
		DarkMagenta = 35,
		/// <summary>深青色 (深藍綠色)。</summary>
		DarkCyan = 36,
		/// <summary>灰色。</summary>
		Gray = 37,
		/// <summary>亮黑（深灰色）。</summary>
		DarkGray = 90,
		/// <summary>亮紅色。</summary>
		Red = 91,
		/// <summary>亮綠色。</summary>
		Green = 92,
		/// <summary>亮黃色。</summary>
		Yellow = 93,
		/// <summary>亮藍色。</summary>
		Blue = 94,
		/// <summary>亮洋紅色 (亮紫紅色)。</summary>
		Magenta = 95,
		/// <summary>亮青色 (亮藍綠色)。</summary>
		Cyan = 96,
		/// <summary>白色。</summary>
		White = 37
	}
	#endregion

	#region Public Enum : CsiCommands
	/// <summary>控制指令列舉</summary>
	[Serializable]
	public enum CsiCommands
	{
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
		/// <summary>CSI n J(ED)：擦除顯示（Erase in Display）。清除螢幕的部分割域。使用的是清除整個螢幕 n=2。</summary>
		Cls,
		/// <summary>CSI n m(SGR)：重新設定顏色。</summary>
		ResetColor,
		/// <summary>CSI n[;m] m(SGR)：設定顏色。</summary>
		Color,
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
		ScrollDown
	}
	#endregion

	/// <summary>表示可變動的 ANSI CSI 字串。這個類別 (Class) 無法被繼承。</summary>
	[Serializable]
	[ComVisible(true)]
	public sealed class CsiBuilder : IEnumerable
	{
		#region Public Consts
		/// <summary>用於判斷 ASNI CSI 的規則運算式字串。</summary>
		public const string PATTERN = "\x1B\\[(\\d+)*(;\\d+)?([ABCDEFGHJKSTfmsu])";
		#endregion

		private List<string> _Items = null;

		#region Construct Method : CsiBuilder()
		/// <summary>建立一個新的 CJF.Utility.Ansi.CsiBuilder 執行個體</summary>
		public CsiBuilder() : this(string.Empty) { }
		#endregion

		#region Construct Method : CsiBuilder(string ansi)
		/// <summary>以 ANSI CSI 字串，建立一個新的 CJF.Utility.Ansi.CsiBuilder 執行個體</summary>
		/// <param name="ansi">欲解析的 ANSI CSI 字串。</param>
		public CsiBuilder(string ansi)
		{
			_Items = new List<string>();
			if (string.IsNullOrEmpty(ansi))
				return;
			MatchCollection mc = Regex.Matches(ansi, PATTERN, RegexOptions.Multiline);
			if (mc.Count == 0)
				_Items.Add(ansi);
			else
			{
				int idx = 0;
				foreach (Match m in mc)
				{
					if (idx != m.Index)
						_Items.Add(ansi.Substring(idx, m.Index - idx));
					_Items.Add(m.Value);
					idx = m.Index + m.Length;
				}
				if (idx < ansi.Length)
					_Items.Add(ansi.Substring(idx, ansi.Length - idx));
			}
		}
		#endregion

		#region Public Property : string this[int index](R/W)
		/// <summary>取得或設定這個執行個體中指定字串位置的字串。</summary>
		/// <param name="index">字串的位置。</param>
		/// <returns>位置 index 上的字串。</returns>
		/// <exception cref="System.IndexOutOfRangeException">取得字串時，index 在這個執行個體的界限外。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">設定字串時，index 在這個執行個體的界限外。</exception>
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
			}
		}
		#endregion

		#region Public Property : int Length(R) + int Count(R)
		/// <summary>取得這個執行個體的字串長度。</summary>
		public int Length { get { return this.ToString().Length; } }
		/// <summary>取得這個執行個體的字串個數。</summary>
		public int Count { get { return _Items.Count; } }
		#endregion

		#region Public Method : void Append(string text)
		/// <summary>新增文字到字串後面。</summary>
		/// <param name="text">欲新增的文字</param>
		public void Append(string text)
		{
			_Items.Add(text);
		}
		#endregion

		#region Public Method : void Append(SgrColors fore)
		/// <summary>將文字前景顏色(SGR)指令放入字串後面。</summary>
		/// <param name="fore">前景顏色值。</param>
		[SecuritySafeCritical]
		public void Append(SgrColors fore)
		{
			_Items.Add(string.Format("\x1B[{0}m", (ushort)fore));
		}
		#endregion

		#region Public Method : void Append(SgrColors fore, string text)
		/// <summary>將文字前景顏色(SGR)指令與文字放入字串後面。</summary>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="text">欲新增的文字。</param>
		[SecuritySafeCritical]
		public void Append(SgrColors fore, string text)
		{
			Append(fore);
			Append(text);
		}
		#endregion

		#region Public Method : void Append(SgrColors fore, SgrColors back)
		/// <summary>將文字前景顏色、背景顏色(SGR)指令放入字串後面。</summary>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="back">背景顏色值。</param>
		public void Append(SgrColors fore, SgrColors back)
		{
			_Items.Add(string.Format("\x1B[{0};{1}m", (ushort)fore, (ushort)back + 10));
		}
		#endregion

		#region Public Method : void Append(SgrColors fore, SgrColors back, string text)
		/// <summary>將文字前景顏色、背景顏色(SGR)指令與文字放入字串後面。</summary>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="back">背景顏色值。</param>
		/// <param name="text">欲新增的文字。</param>
		public void Append(SgrColors fore, SgrColors back, string text)
		{
			Append(fore, back);
			Append(text);
		}
		#endregion

		#region Public Method : void AppendFormat(string format, params object[] args)
		/// <summary>以指定之陣列中對應物件的字串表示，取代指定之字串中的格式項目，並新增到字串後面。</summary>
		/// <param name="format">複合格式字串。</param>
		/// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
		public void AppendFormat(string format, params object[] args)
		{
			_Items.Add(string.Format(format, args));
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

		#region Public Method : void AppendLine(SgrColors fore, string text)
		/// <summary>將文字前景顏色(SGR)指令、文字與換行符號放入字串後面。</summary>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="text">欲新增的文字。</param>
		public void AppendLine(SgrColors fore, string text)
		{
			Append(fore, text + Environment.NewLine);
		}
		#endregion

		#region Public Method : void AppendLine(SgrColors fore, SgrColors back, string text)
		/// <summary>將文字前景顏色、背景顏色(SGR)指令與文字放入字串後面。</summary>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="back">背景顏色值。</param>
		/// <param name="text">欲新增的文字。</param>
		public void AppendLine(SgrColors fore, SgrColors back, string text)
		{
			Append(fore, back, text + Environment.NewLine);
		}
		#endregion

		#region Public Method : void AppendCommand(Commands cmd, params string[] args)
		/// <summary>將控制指令放入字串後方。</summary>
		/// <param name="cmd">控制指令列舉值。</param>
		/// <param name="args">附屬的參數值。</param>
		/// <exception cref="System.ArgumentNullException">所用的指令必須一併傳入附屬參數值。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">傳入附屬參數數量不符合所用的指令。</exception>
		public void AppendCommand(CsiCommands cmd, params string[] args)
		{
			_Items.Add(GetCsiString(cmd, args));
		}
		#endregion

		#region Public Method : void Insert(int index, string text)
		/// <summary>將文字字串插入這個執行個體中指定的索引處。</summary>
		/// <param name="index">這個執行個體中開始插入的位置。</param>
		/// <param name="text">要插入的文字字串。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
		public void Insert(int index, string text)
		{
			if (index < 0 || index > _Items.Count)
				throw new ArgumentOutOfRangeException();
			_Items.Insert(index, text);
		}
		#endregion

		#region Public Method : void Insert(int index, SgrColors fore, string text)
		/// <summary>將文字前景顏色(SGR)指令、文字字串插入這個執行個體中指定的索引處。</summary>
		/// <param name="index">這個執行個體中開始插入的位置。</param>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="text">要插入的文字字串。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
		public void Insert(int index, SgrColors fore, string text)
		{
			if (index < 0 || index > _Items.Count)
				throw new ArgumentOutOfRangeException();
			Insert(index, text);
			InsertCommand(index, CsiCommands.Color, (int)fore);
		}
		#endregion

		#region Public Method : void Insert(int index, SgrColors fore, SgrColors back, string text)
		/// <summary>將文字前景顏色、背景顏色(SGR)指令、文字字串插入這個執行個體中指定的索引處。</summary>
		/// <param name="index">這個執行個體中開始插入的位置。</param>
		/// <param name="fore">前景顏色值。</param>
		/// <param name="back">背景顏色值。</param>
		/// <param name="text">要插入的文字字串。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。</exception>
		public void Insert(int index, SgrColors fore, SgrColors back, string text)
		{
			if (index < 0 || index > _Items.Count)
				throw new ArgumentOutOfRangeException();
			Insert(index, text);
			InsertCommand(index, CsiCommands.Color, (int)fore, (int)back + 10);
		}
		#endregion

		#region Public Method : void InsertCommand(int index, CsiCommands cmd, params object[] args)
		/// <summary>將控制指令插入這個執行個體中指定的索引處。</summary>
		/// <param name="index">這個執行個體中開始插入的位置。</param>
		/// <param name="cmd">控制指令列舉值。</param>
		/// <param name="args">附屬的參數值。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於零或大於這個執行個體目前的長度。- 或 -傳入附屬參數數量不符合所用的指令。</exception>
		/// <exception cref="System.ArgumentNullException">所用的指令必須一併傳入附屬參數值。</exception>
		public void InsertCommand(int index, CsiCommands cmd, params object[] args)
		{
			if (index < 0 || index > _Items.Count)
				throw new ArgumentOutOfRangeException();
			_Items.Insert(index, GetCsiString(cmd, args));
		}
		#endregion

		#region Public Method : int[] IndexesOf()
		/// <summary>取得這個執行個體中所有 ANSI CSI 指令的元素位置。</summary>
		/// <returns>所有 ANSI CSI 指令的元素位置。</returns>
		public int[] IndexesOf()
		{
			List<int> idxs = new List<int>();
			int idx = _Items.FindIndex(s => s.StartsWith("\x1B["));
			do
			{
				if (idx != -1)
					idxs.Add(idx);
				idx = _Items.FindIndex(idx + 1, s => s.StartsWith("\x1B["));
			}
			while (idx != -1);
			return idxs.ToArray();
		}
		#endregion

		#region Public Method : int IndexOf(CsiCommands cmd)
		/// <summary>搜尋指定的 ANSI CSI 指令，並傳回整個執行個體中第一個相符項目之以零起始的索引。</summary>
		/// <param name="cmd">欲搜尋的 ANSI CSI 指令。</param>
		/// <returns>如果有找到，則是在整個執行個體內，第一個相符項目的以零起始的索引，否則為 -1。</returns>
		public int IndexOf(CsiCommands cmd)
		{
			return this.IndexOf(cmd, 0);
		}
		#endregion

		#region Public Method : int IndexOf(CsiCommands cmd, int index)
		/// <summary>在這個執行個體中從指定的索引開始到最後一個項目這段範圍內，搜尋指定的 ANSI CSI 指令第一次出現的位置，並傳回其索引值(索引以零起始)。</summary>
		/// <param name="cmd">欲搜尋的 ANSI CSI 指令。</param>
		/// <param name="index">搜尋之以零起始的起始索引。0 (零) 在空白清單中有效。</param>
		/// <returns>如果有找到，則是在整個執行個體內，第一個相符項目的以零起始的索引，否則為 -1。</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">index 在這個執行個體的有效索引範圍之外。</exception>
		public int IndexOf(CsiCommands cmd, int index)
		{
			if (index < 0 || index >= _Items.Count)
				throw new ArgumentOutOfRangeException();
			string sk = GetCsiString(cmd, 0, 0);
			sk = sk.Substring(sk.Length - 1);
			return _Items.FindIndex(index, s => s.StartsWith("\x1B[") && s.EndsWith(sk));
		}
		#endregion

		#region Public Method : void CopyTo(string[] array)
		/// <summary>將整個執行個體複製到相容的一維陣列，從目標陣列的開頭開始。</summary>
		/// <param name="array">一維字串陣列，是從這個執行個體複製過來之元素的目的端。array 必須有以零起始的索引。</param>
		/// <exception cref="System.ArgumentNullException">array 為 null。</exception>
		/// <exception cref="System.ArgumentException">這個執行個體中的元素數目大於目的 array 可包含的元素數目。</exception>
		public void CopyTo(string[] array)
		{
			if (array == null)
				throw new ArgumentNullException();
			else if (_Items.Count > array.Length)
				throw new ArgumentException();
			_Items.CopyTo(array);
		}
		#endregion

		#region Public Method : void CopyTo(string[] array, int arrayIndex)
		/// <summary>從目標陣列的指定索引處開始，將整個執行個體複製到相容的一維陣列中。</summary>
		/// <param name="array">一維字串陣列，是從這個執行個體複製過來之元素的目的端。array 必須有以零起始的索引。</param>
		/// <param name="arrayIndex">array 中以零起始的索引，是開始複製的位置。</param>
		/// <exception cref="System.ArgumentNullException">array 為 null。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex 小於 0。</exception>
		/// <exception cref="System.ArgumentException">這個執行個體的元素數目，超過從 arrayIndex 到目的 array 末尾之間的可用空間。</exception>
		public void CopyTo(string[] array, int arrayIndex)
		{
			if (array == null)
				throw new ArgumentNullException();
			else if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException();
			else if (_Items.Count > array.Length - arrayIndex)
				throw new ArgumentException();
			_Items.CopyTo(array, arrayIndex);
		}
		#endregion

		#region Public Method : void CopyTo(int index, string[] array, int arrayIndex, int count)
		/// <summary>從目標陣列的指定索引處開始，將元素範圍從這個執行個體複製到相容的一維陣列。</summary>
		/// <param name="index">來源這個執行個體中以零起始的索引，是開始複製的位置。</param>
		/// <param name="array">一維字串陣列，必須有以零起始的索引。</param>
		/// <param name="arrayIndex">array 中以零起始的索引，是開始複製的位置。</param>
		/// <param name="count">要複製的字串數目。</param>
		/// <exception cref="System.ArgumentNullException">array 為 null。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -arrayIndex 小於 0。- 或 -count 小於等於 0。</exception>
		/// <exception cref="System.ArgumentException">
		/// <para>index 等於或大於這個執行個體的數量。</para>
		/// <para>- 或 -從 index 到這個執行個體末尾的元素數目，超過從 arrayIndex 到目的 array 末尾之間的可用空間。</para>
		/// </exception>
		public void CopyTo(int index, string[] array, int arrayIndex, int count)
		{
			if (array == null)
				throw new ArgumentNullException();
			else if (index < 0 || arrayIndex < 0 || count <= 0)
				throw new ArgumentOutOfRangeException();
			else if (index >= _Items.Count || index + count > array.Length - arrayIndex)
				throw new ArgumentException();
			_Items.CopyTo(index, array, arrayIndex, count);
		}
		#endregion

		#region Public Method : void RemoveAt(int index)
		/// <summary> 移除這個執行個體中指定之索引處的項目。</summary>
		/// <param name="index">要移除元素之以零起始的索引。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -index 等於或大於這個執行個體元素數量。</exception>
		public void RemoveAt(int index)
		{
			if (index < 0 || index >= _Items.Count)
				throw new ArgumentOutOfRangeException();
			RemoveRange(index, 1);
		}
		#endregion

		#region Public Method : void RemoveRange(int startIndex, int length)
		/// <summary>從這個執行個體中移除的元素範圍。</summary>
		/// <param name="index">要移除之元素範圍內之以零起始的起始索引。</param>
		/// <param name="count">要移除的元素數目。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -count 小於 0。</exception>
		/// <exception cref=" System.ArgumentException">index 和 count 並不代表這個執行個體中元素的有效範圍。</exception>
		public void RemoveRange(int index, int count)
		{
			if (index < 0 || count < 0)
				throw new ArgumentOutOfRangeException();
			else if (index >= _Items.Count || count > _Items.Count || _Items.Count - index < count)
				throw new ArgumentException();
			_Items.RemoveRange(index, count);
		}
		#endregion

		#region Public Method : void Clear()
		/// <summary>清除這個執行個體中所有字串內容。</summary>
		public void Clear()
		{
			_Items.Clear();
		}
		#endregion

		#region Public Method : string[] ToArray()
		/// <summary>將這個執行個體的字串元素複製到新的陣列。</summary>
		/// <returns>陣列，包含這個執行個體的項目複本。</returns>
		public string[] ToArray()
		{
			return _Items.ToArray();
		}
		#endregion

		#region Public Method : string ToPureText()
		/// <summary>將這個執行個體的值轉換為不包含 ANSI CSI 的純文字字串。</summary>
		/// <returns>不包含 ANSI CSI 的純文字字串。</returns>
		public string ToPureText()
		{
			return Regex.Replace(ToString(), PATTERN, "");
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

		#region IEnumerable 成員
		/// <summary>傳回會逐一查看集合的列舉程式。</summary>
		/// <returns>System.Collections.IEnumerator 物件，可用於逐一查看集合。</returns>
		public IEnumerator GetEnumerator()
		{
			return _Items.GetEnumerator();
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
			string res = "\x1B[";
			switch (cmd)
			{
				case CsiCommands.Cls: res += "2J"; break;
				case CsiCommands.ResetColor: res += "0m"; break;
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
				case CsiCommands.Color:
					if (args == null || args.Length == 0)
						throw new ArgumentNullException();
					else if (args.Length == 1)
						res += "{0}m";
					else if (args.Length >= 2)
						res += "{0};{1}m";
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

	}
}
