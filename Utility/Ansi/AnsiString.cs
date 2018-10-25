using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CJF.Utility.Ansi
{
	/// <summary>提供協助 TelnetServer 輸出 ANSI 相關控制碼</summary>
	[Obsolete("請改用 CJF.Utility.Ansi.CsiBuilder", true)]
	public static class AnsiString
	{
		#region Private Varaibles
		private static Dictionary<string, AnsiData> _AnsiTable = new Dictionary<string, AnsiData>();
		#endregion

		#region Construct Method : AnsiString()
		/// <summary>靜態建立 CJF.Utility.AnsiString 類別</summary>
		static AnsiString()
		{
			#region 指令類
			AppendData("{cls}", CodeType.Command, "\x1B[2J", "Clears part of the screen");
			AppendData("{c2end}", CodeType.Command, "\x1B[0K", "Clear from cursor to the end of the line");
			AppendData("{c2start}", CodeType.Command, "\x1B[1K", "Clear from cursor to beginning of the line");
			AppendData("{cline}", CodeType.Command, "\x1B[2K", "Clear entire line");
			AppendData("{savecursor}", CodeType.Command, "\x1B[s", "Saves the cursor position");
			AppendData("{restorecursor}", CodeType.Command, "\x1B[u", "Restores the cursor position");
			#endregion

			#region 字型、顏色類
			// 重置字型與顏色
			AppendData("{reset}", CodeType.FontColor, "\x1B[0m", "Reset");

			// 字型樣式(開)
			AppendData("{bold}", CodeType.FontColor, "\x1B[1m", "Bold");
			AppendData("{italic}", CodeType.FontColor, "\x1B[3m", "Italic");
			AppendData("{ul}", CodeType.FontColor, "\x1B[4m", "Underline");
			AppendData("{blink}", CodeType.FontColor, "\x1B[5m", "Blink");
			AppendData("{blinkf}", CodeType.FontColor, "\x1B[6m", "Blink Fast");
			AppendData("{inverse}", CodeType.FontColor, "\x1B[7m", "Inverse");
			AppendData("{conceal}", CodeType.FontColor, "\x1B[8m", "Conceal(Not widely supported.)");
			AppendData("{strike}", CodeType.FontColor, "\x1B[9m", "Strikethrough");

			// 字型樣式(關)
			AppendData("{!bold}", CodeType.FontColor, "\x1B[22m", "Bold Off");
			AppendData("{!italic}", CodeType.FontColor, "\x1B[23m", "Italic Off");
			AppendData("{!ul}", CodeType.FontColor, "\x1B[24m", "Underline Off");
			AppendData("{!blink}", CodeType.FontColor, "\x1B[25m", "Blink Off");
			AppendData("{!inverse}", CodeType.FontColor, "\x1B[27m", "Inverse Off");
			AppendData("{!strike}", CodeType.FontColor, "\x1B[29m", "Strikethrough Off");

			// 前景顏色
			AppendData("{black}", CodeType.FontColor, "\x1B[30m", "Foreground Black");
			AppendData("{d_red}", CodeType.FontColor, "\x1B[31m", "Foreground DarkRed");
			AppendData("{d_green}", CodeType.FontColor, "\x1B[32m", "Foreground DarkGreen");
			AppendData("{d_yellow}", CodeType.FontColor, "\x1B[33m", "Foreground DarkYellow");
			AppendData("{d_blue}", CodeType.FontColor, "\x1B[34m", "Foreground DarkBlue");
			AppendData("{d_magenta}", CodeType.FontColor, "\x1B[35m", "Foreground DarkMagenta");
			AppendData("{d_cyan}", CodeType.FontColor, "\x1B[36m", "Foreground DarkCyan");
			AppendData("{gray}", CodeType.FontColor, "\x1B[37m", "Foreground Gray");
			AppendData("{d_gray}", CodeType.FontColor, "\x1B[1;30m", "Foreground DarkGary");
			AppendData("{red}", CodeType.FontColor, "\x1B[1;31m", "Foreground Red");
			AppendData("{green}", CodeType.FontColor, "\x1B[1;32m", "Foreground Green");
			AppendData("{yellow}", CodeType.FontColor, "\x1B[1;33m", "Foreground Yellow");
			AppendData("{blue}", CodeType.FontColor, "\x1B[1;34m", "Foreground Blue");
			AppendData("{magenta}", CodeType.FontColor, "\x1B[1;35m", "Foreground Magenta");
			AppendData("{cyan}", CodeType.FontColor, "\x1B[1;36m", "Foreground Cyan");
			AppendData("{white}", CodeType.FontColor, "\x1B[1;37m", "Foreground White");
			AppendData("{defcolor}", CodeType.FontColor, "\x1B[39m", "Default Foreground Color");

			// 背景顏色
			AppendData("{!black}", CodeType.FontColor, "\x1B[40m", "Background Black");
			AppendData("{!d_red}", CodeType.FontColor, "\x1B[41m", "Background DarkRed");
			AppendData("{!d_green}", CodeType.FontColor, "\x1B[42m", "Background DarjGreen");
			AppendData("{!d_yellow}", CodeType.FontColor, "\x1B[43m", "Background DarkYellow");
			AppendData("{!d_blue}", CodeType.FontColor, "\x1B[44m", "Background DarkBlue");
			AppendData("{!d_magenta}", CodeType.FontColor, "\x1B[45m", "Background DarkMagenta");
			AppendData("{!d_cyan}", CodeType.FontColor, "\x1B[46m", "Background DarkCyan");
			AppendData("{!gray}", CodeType.FontColor, "\x1B[47m", "Background Gray");
			AppendData("{!d_gray}", CodeType.FontColor, "\x1B[1;40m", "Background DarkGray");
			AppendData("{!red}", CodeType.FontColor, "\x1B[1;41m", "Background Red");
			AppendData("{!green}", CodeType.FontColor, "\x1B[1;42m", "Background Green");
			AppendData("{!yellow}", CodeType.FontColor, "\x1B[1;43m", "Background Yellow");
			AppendData("{!blue}", CodeType.FontColor, "\x1B[1;44m", "Background Blue");
			AppendData("{!magenta}", CodeType.FontColor, "\x1B[1;45m", "Background Magenta");
			AppendData("{!cyan}", CodeType.FontColor, "\x1B[1;46m", "Background Cyan");
			AppendData("{!white}", CodeType.FontColor, "\x1B[1;47m", "Background White");
			AppendData("{!defcolor}", CodeType.FontColor, "\x1B[49m", "Default Background Color");
			#endregion
		}
		#endregion

		#region Private Static Method : void AppendData(string key, CodeType ct, string code, string memo)
		private static void AppendData(string key, CodeType ct, string code, string memo)
		{
			_AnsiTable.Add(key, new AnsiData(key, ct, code, memo));
		}
		#endregion

		#region Public Static Method : string ToANSI(string source)
		/// <summary>將關鍵字替換成ANSI String</summary>
		/// <param name="source">內含關鍵字的原始字串</param>
		/// <returns>替換完畢包含ANSI String的字串</returns>
		public static string ToANSI(string source)
		{
			foreach (AnsiData ansiData in _AnsiTable.Values)
				source = source.Replace(ansiData.Key, ansiData.Code);
			return (source);
		}
		#endregion

		#region Public Static Method : string RemoveANSI(string source)
		/// <summary>移除ANSI代碼</summary>
		/// <param name="source">包含 ANSI 代碼的字串</param>
		/// <returns></returns>
		public static string RemoveANSI(string source)
		{
			return Regex.Replace(source, "\x1B\\[(\\d+)*(;\\d+)?([ABCDEFGHJKSTfmsu])", "");
		}
		#endregion

		#region Public Static Method : string CleanKeycode(string source)
		/// <summary>將關鍵字清除</summary>
		/// <param name="source">內含關鍵字的原始字串</param>
		/// <returns>清除完畢的字串</returns>
		public static string CleanKeycode(string source)
		{
			Regex reg = new Regex(@"\{\w+\}", RegexOptions.Multiline);
			return reg.Replace(source, "");
		}
		#endregion

		#region Demo Methods
		/// <summary>展示字形顏色</summary>
		/// <returns></returns>
		public static string DemoAnsiColor()
		{
			StringBuilder output = new StringBuilder();
			foreach (AnsiData ansiData in _AnsiTable.Values)
			{
				if (ansiData.CodeType == CodeType.FontColor)
					output.AppendFormat("Displaying {0,-20} {1}TEST{{reset}}\r\n", ansiData.Memo, ansiData.Key);
			}
			return (ToANSI(output.ToString() + "\r\n\r\n"));
		}
		#endregion

		#region Public Static Method : string ProgressBarDemo(int percent, int width, string colorCode)
		/// <summary>展示如何以ANSI String方式呈現ProgressBar</summary>
		/// <param name="percent">百分比</param>
		/// <param name="width">寬度</param>
		/// <param name="colorCode">顏色代碼</param>
		/// <returns></returns>
		public static string DemoProgressBar(int percent, int width, string colorCode)
		{
			if (percent < 0 || percent > 100) percent = 0;
			int blocks = (int)((percent / 100f) * width);
			string progressBar = "{" + colorCode;
			for (int i = 0; i < blocks; i++)
				progressBar += ' ';
			progressBar += "{reset}";

			for (int i = 0; i < width - blocks; i++)
				progressBar += ' ';
			progressBar += '}';

			return ("Progress Bar: " + ToANSI(progressBar) + "\r\n");
		}
		#endregion

		#region Public Static Method : string MoveCursor(ushort row, ushort col)
		/// <summary>產生定位游標位置的ANSI語法</summary>
		/// <param name="row">行號(橫)</param>
		/// <param name="col">列號(直)</param>
		/// <returns></returns>
		public static string MoveCursor(ushort row, ushort col)
		{
			return string.Format("\x1B[{0};{1}H", row, col);
		}
		#endregion

		#region Public Static Properties
		/// <summary>取得ANSI控制碼:清除畫面({cls})</summary>
		public static string Cls { get { return _AnsiTable["{cls}"].Code; } }
		/// <summary>取得ANSI控制碼:清除該行游標後面的文字({c2end})</summary>
		public static string ClearToLineEndFromCursor { get { return _AnsiTable["{c2end}"].Code; } }
		/// <summary>取得ANSI控制碼:自游標該行的行首清除至游標位置({c2start})</summary>
		public static string ClearToCursorFromLineStart { get { return _AnsiTable["{c2start}"].Code; } }
		/// <summary>取得ANSI控制碼:清除該行文字內容({cline})</summary>
		public static string ClearLine { get { return _AnsiTable["{cline}"].Code; } }
		/// <summary>取得ANSI控制碼:記錄游標位置({savecursor})</summary>
		public static string SaveCursor { get { return _AnsiTable["{savecursor}"].Code; } }
		/// <summary>取得ANSI控制碼:還原游標位置({restorecursor})</summary>
		public static string RestoreCursor { get { return _AnsiTable["{restorecursor}"].Code; } }
		/// <summary>取得ANSI控制碼:還原字型與顏色預設值({reset})</summary>
		public static string Reset { get { return _AnsiTable["{reset}"].Code; } }
		/// <summary>取得ANSI控制碼:設定粗體字({bold})</summary>
		public static string Bold { get { return _AnsiTable["{bold}"].Code; } }
		/// <summary>取得ANSI控制碼:還原粗體字({bold})</summary>
		public static string BoldOff { get { return _AnsiTable["{!bold}"].Code; } }
		/// <summary>取得ANSI控制碼:設定斜體字({italic})</summary>
		public static string Italic { get { return _AnsiTable["{italic}"].Code; } }
		/// <summary>取得ANSI控制碼:還原斜體字({italic})</summary>
		public static string ItalicOff { get { return _AnsiTable["{!italic}"].Code; } }
		/// <summary>取得ANSI控制碼:加上底線{ul}</summary>
		public static string Underline { get { return _AnsiTable["{ul}"].Code; } }
		/// <summary>取得ANSI控制碼:移除底線{ul}</summary>
		public static string UnderlineOff { get { return _AnsiTable["{!ul}"].Code; } }
		/// <summary>取得ANSI控制碼:緩慢閃爍</summary>
		public static string Blink { get { return _AnsiTable["{blink}"].Code; } }
		/// <summary>取得ANSI控制碼:快速閃爍</summary>
		public static string BlinkFast { get { return _AnsiTable["{blinkf}"].Code; } }
		/// <summary>取得ANSI控制碼:前景色與背景色交換</summary>
		public static string Inverse { get { return _AnsiTable["{inverse}"].Code; } }
		/// <summary>取得ANSI控制碼:隱藏(未廣泛支援)</summary>
		public static string Conceal { get { return _AnsiTable["{conceal}"].Code; } }
		/// <summary>取得ANSI控制碼:刪除線(未廣泛支援)</summary>
		public static string Strikethrough { get { return _AnsiTable["{strike}"].Code; } }
		/// <summary></summary>
		public static string ForeColorBlack { get { return _AnsiTable["{black}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkRed { get { return _AnsiTable["{d_red}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkGreen { get { return _AnsiTable["{d_green}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkYellow { get { return _AnsiTable["{d_yellow}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkBlue { get { return _AnsiTable["{d_blue}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkMagenta { get { return _AnsiTable["{d_magenta}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkCyan { get { return _AnsiTable["{b_cyan}"].Code; } }
		/// <summary></summary>
		public static string ForeColorGray { get { return _AnsiTable["{gray}"].Code; } }
		/// <summary></summary>
		public static string ForeColorDarkGray { get { return _AnsiTable["{d_gray}"].Code; } }
		/// <summary></summary>
		public static string ForeColorRed { get { return _AnsiTable["{red}"].Code; } }
		/// <summary></summary>
		public static string ForeColorGreen { get { return _AnsiTable["{green}"].Code; } }
		/// <summary></summary>
		public static string ForeColorYellow { get { return _AnsiTable["{yellow}"].Code; } }
		/// <summary></summary>
		public static string ForeColorBlue { get { return _AnsiTable["{blue}"].Code; } }
		/// <summary></summary>
		public static string ForeColorMagenta { get { return _AnsiTable["{magenta}"].Code; } }
		/// <summary></summary>
		public static string ForeColorCyan { get { return _AnsiTable["{cyan}"].Code; } }
		/// <summary></summary>
		public static string ForeColorWhite { get { return _AnsiTable["{white}"].Code; } }
		/// <summary></summary>
		public static string DefaultForeColor { get { return _AnsiTable["{defcolor}"].Code; } }
		/// <summary></summary>
		public static string BackColorBlack { get { return _AnsiTable["{!black}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkRed { get { return _AnsiTable["{!d_red}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkGreen { get { return _AnsiTable["{!d_green}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkYellow { get { return _AnsiTable["{!d_yellow}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkBlue { get { return _AnsiTable["{!d_blue}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkMagenta { get { return _AnsiTable["{!d_magenta}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkCyan { get { return _AnsiTable["{!b_cyan}"].Code; } }
		/// <summary></summary>
		public static string BackColorGray { get { return _AnsiTable["{!gray}"].Code; } }
		/// <summary></summary>
		public static string BackColorDarkGray { get { return _AnsiTable["{!d_gray}"].Code; } }
		/// <summary></summary>
		public static string BackColorRed { get { return _AnsiTable["{!red}"].Code; } }
		/// <summary></summary>
		public static string BackColorGreen { get { return _AnsiTable["{!green}"].Code; } }
		/// <summary></summary>
		public static string BackColorYellow { get { return _AnsiTable["{!yellow}"].Code; } }
		/// <summary></summary>
		public static string BackColorBlue { get { return _AnsiTable["{!blue}"].Code; } }
		/// <summary></summary>
		public static string BackColorMagenta { get { return _AnsiTable["{!magenta}"].Code; } }
		/// <summary></summary>
		public static string BackColorCyan { get { return _AnsiTable["{!cyan}"].Code; } }
		/// <summary></summary>
		public static string BackColorWhite { get { return _AnsiTable["{!white}"].Code; } }
		/// <summary></summary>
		public static string DefaultBackColor { get { return _AnsiTable["{!defcolor}"].Code; } }
		/// <summary>取得ANSI控制碼的所有關鍵字</summary>
		public static string[] AllKeyWords
		{
			get
			{
				string[] keys = new string[_AnsiTable.Keys.Count];
				_AnsiTable.Keys.CopyTo(keys, 0);
				return keys;
			}
		}
		#endregion
	}

	#region Enum : CodeType
	/// <summary>ANSI 控制代碼類型列舉</summary>
	public enum CodeType
	{
		/// <summary>未定義</summary>
		None = 0,
		/// <summary>命令、控制型</summary>
		Command = 1,
		/// <summary>字形顏色型</summary>
		FontColor = 2
	}
	#endregion

	#region Struct : AnsiData
	/// <summary>AnsiString 類別用的控制碼結構</summary>
	struct AnsiData
	{
        #region Public Readonly Properties
        public string Code { get; }
        public string Key { get; }
        public string Memo { get; }
        public CodeType CodeType { get; }
        #endregion

        public AnsiData(string key, CodeType ct, string code, string definition)
		{
			Key = key;
			Code = code;
			Memo = definition;
			CodeType = ct;
		}

		public static AnsiData Empty { get { return new AnsiData(); } }
		public bool IsEmpty { get { return string.IsNullOrEmpty(Key); } }
		public bool Equals(AnsiData item)
		{
			return item.Key.Equals(this.Key);
		}
	}
	#endregion
}
