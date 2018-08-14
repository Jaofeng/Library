using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Net;
using CJF.Net;
using CJF.Net.Telnet;
using CJF.Utility;

namespace STE.TGL.TCCU
{
	#region Telnet Functions - Telnet Remarked
    /*
	#region Private Enum : ArrowKeys
	enum ArrowKeys
	{
		/// <summary>上鍵</summary>
		UpArrow = 1,
		/// <summary>下鍵</summary>
		DownArrow = 2,
		/// <summary>左鍵</summary>
		LeftArrow = 3,
		/// <summary>右鍵</summary>
		RightArrow = 4,
		/// <summary>空白鍵，模擬 Check 用</summary>
		Space = 5,
		/// <summary>輸入鍵，模擬確定(OK)用</summary>
		Enter = 6,
		/// <summary>Escape鍵，模擬取消(Cancel)用</summary>
		Escape = 7
	}
	#endregion

	#region Private Class : MenuCommandAttribute
	class MenuCommandAttribute : Attribute
	{
		string[] _Commands = null;
		public MenuCommandAttribute(params string[] cmds)
		{
			_Commands = new string[cmds.Length];
			Array.Copy(cmds, _Commands, cmds.Length);
		}
		public string[] Commands { get { return _Commands; } }
	}
	#endregion

	#region Private Class : ExecuteCommandAttribute
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	class ExecuteCommandAttribute : Attribute
	{
		/// <summary>建立 ExecuteCommandAttribute 類別，供 Telnet 選單用</summary>
		/// <param name="cmd">按鍵指令</param>
		/// <param name="funcName">欲執行的函示名稱</param>
		/// <param name="goNext">是否接續執行</param>
		/// <param name="variables">傳入的參數資料</param>
		public ExecuteCommandAttribute(string cmd, string funcName, bool goNext, params object[] variables)
		{
			this.Command = cmd;
			Type type = typeof(SvcWorker);
			this.Method = type.GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (variables != null)
			{
				this.Variables = new object[variables.Length];
				Array.Copy(variables, this.Variables, this.Variables.Length);
			}
			this.ContinueNextStep = goNext;
		}

		/// <summary>建立 ExecuteCommandAttribute 類別，供 Telnet 選單用</summary>
		/// <param name="cmd">按鍵指令</param>
		/// <param name="funcName">欲執行的函示名稱</param>
		/// <param name="variables">傳入的參數資料</param>
		public ExecuteCommandAttribute(string cmd, string funcName, params object[] variables) : this(cmd, funcName, true, variables) { }
		/// <summary>取得按鍵指令</summary>
		public string Command { get; private set; }
		/// <summary>取得函示呼叫點</summary>
		public MethodInfo Method { get; private set; }
		/// <summary>取得傳入的參數資料</summary>
		public object[] Variables { get; private set; }
		/// <summary>取得是否接續執行</summary>
		public bool ContinueNextStep { get; private set; }
	}
	#endregion

	#region Private Class : InputCommandAttribute
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	class InputCommandAttribute : Attribute
	{
		MethodInfo _Method = null;
		Dictionary<string, string[]> _Prompts = null;
		/// <summary>將選單函示定義成可輸入的自訂屬性類別</summary>
		/// <param name="funcName">函示名稱</param>
		/// <param name="prompt">輸入提示字串</param>
		/// <param name="keys">可輸入的文字字串，如無限制，則輸入null或空陣列</param>
		public InputCommandAttribute(string funcName, string prompt, string[] keys)
		{
			Type type = typeof(SvcWorker);
			_Method = type.GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			_Prompts = new Dictionary<string, string[]>();
			if (keys != null && keys.Length != 0)
			{
				string[] pks = new string[keys.Length];
				Array.Copy(keys, pks, pks.Length);
				_Prompts.Add(prompt, pks);
			}
			else
				_Prompts.Add(prompt, new string[] { });
		}
		/// <summary>將選單函示定義成可輸入的自訂屬性類別</summary>
		/// <param name="funcName">函示名稱</param>
		/// <param name="prompt1">第一組輸入提示字串</param>
		/// <param name="keys1">第一組可輸入的文字字串，如無限制，則輸入null或空陣列</param>
		/// <param name="prompt2">第二組輸入提示字串</param>
		/// <param name="keys2">第二組可輸入的文字字串，如無限制，則輸入null或空陣列</param>
		public InputCommandAttribute(string funcName, string prompt1, string[] keys1, string prompt2, string[] keys2)
		{
			Type type = typeof(SvcWorker);
			_Method = type.GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			_Prompts = new Dictionary<string, string[]>();
			if (keys1 != null && keys1.Length != 0)
			{
				string[] pks = new string[keys1.Length];
				Array.Copy(keys1, pks, pks.Length);
				_Prompts.Add(prompt1, pks);
			}
			else
				_Prompts.Add(prompt1, new string[] { });
			if (keys2 != null && keys2.Length != 0)
			{
				string[] pks = new string[keys2.Length];
				Array.Copy(keys2, pks, pks.Length);
				_Prompts.Add(prompt2, pks);
			}
			else
				_Prompts.Add(prompt2, new string[] { });
		}
		public MethodInfo Method { get { return _Method; } }
		public Dictionary<string, string[]> Prompts { get { return _Prompts; } }
	}
	#endregion

	#region Private Class : CursorControlAttribute
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
	class CursorControlAttribute : Attribute
	{
		/// <summary>建立 CursorControlAttribute 類別，供 Telnet 選單用，此建立方式適用於核選類(Check)的功能</summary>
		/// <param name="ck">控制按鍵</param>
		/// <param name="funcName">欲執行的函示名稱</param>
		/// <param name="dev">設備代碼</param>
		/// <param name="mode">設備模組(如果有)</param>
		/// <param name="minY">第一選擇項的所在 Y 位置</param>
		public CursorControlAttribute(ArrowKeys ck, string funcName, string dev, string mode, int minY) : this(ck, funcName, new object[] { dev, mode, minY, null, null, null }) { }
		/// <summary>建立 CursorControlAttribute 類別，供 Telnet 選單用</summary>
		/// <param name="ck">控制按鍵</param>
		/// <param name="funcName">欲執行的函示名稱</param>
		/// <param name="variables">傳入的參數資料, 0:設備代碼, 1:設備模組, 2:第一選擇項的所在 Y 位置, 3:選擇項數量/核選類項目文字 X 位置, 4:選擇項可調整的最小 X 位置, 5:選擇項可調整的格數</param>
		public CursorControlAttribute(ArrowKeys ck, string funcName, params object[] variables)
		{
			switch (ck)
			{
				case ArrowKeys.UpArrow: this.KeyCode = "\x1B[A"; break;
				case ArrowKeys.DownArrow: this.KeyCode = "\x1B[B"; break;
				case ArrowKeys.LeftArrow: this.KeyCode = "\x1B[D"; break;
				case ArrowKeys.RightArrow: this.KeyCode = "\x1B[C"; break;
				case ArrowKeys.Space: this.KeyCode = "\x20"; break;
				case ArrowKeys.Enter: this.KeyCode = "\x0A"; break;
				case ArrowKeys.Escape: this.KeyCode = "\x1B"; break;
				default: break;
			}
			Type type = typeof(SvcWorker);
			this.Method = type.GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			if (variables != null)
			{
				this.Variables = new object[variables.Length];
				Array.Copy(variables, this.Variables, this.Variables.Length);
			}
		}
		/// <summary>取得控制鍵</summary>
		public string KeyCode { get; private set; }
		/// <summary>取得函示呼叫點</summary>
		public MethodInfo Method { get; private set; }
		/// <summary>取得傳入的參數資料</summary>
		public object[] Variables { get; private set; }
	}
	#endregion

	public partial class SvcWorker
	{
		/// <summary>Telnet 指令執行後的暫停時間，單位豪秒</summary>
		const int TELNET_CMD_EXEC_SLEEP = 2000;
		/// <summary>語音設備最小增益值</summary>
		const short DB_LEVEL_MIN = -60;
		/// <summary>語音設備最大增益值</summary>
		const short DB_LEVEL_MAX = 20;
		/// <summary>顯示在 Telnet 用戶端的調整條的寬度</summary>
		const int DB_LEVEL_BAR_SIZE = 40;

		#region Private Struct : CursorLocation
		struct CursorLocation : IEquatable<CursorLocation>, ICloneable
		{
			public int Row;
			public int Col;
			public CursorLocation(int r, int c)
			{
				this.Row = r;
				this.Col = c;
			}
			public CursorLocation Clone() { return new CursorLocation(this.Row, this.Col); }
			public bool Equals(CursorLocation other) { return this.Row == other.Row && this.Col == other.Col; }
			object ICloneable.Clone() { return this.Clone(); }
		}
		#endregion

		/// <summary>處理 Telnet 每條連線之選單當前執行路徑</summary>
		ConcurrentDictionary<EndPoint, string> _MenuPath = null;
		/// <summary>處理 Telnet 每條連線之選單輸入的內容</summary>
		ConcurrentDictionary<EndPoint, string[]> _TelnetInput = null;
		/// <summary>紀錄 Telnet 使用者是否正在監控設備狀態資料</summary>
		ConcurrentDictionary<EndPoint, TelnetMonitorType> _TelnetOnMonitor = null;
		/// <summary>紀錄 Telnet 使用者選單游標位置</summary>
		ConcurrentDictionary<EndPoint, CursorLocation> _TelnetCursor = null;

		#region Private Method : MethodInfo GetMenuMethod(string cp)
		private MethodInfo GetMenuMethod(string cp)
		{
			Type type = this.GetType();
			MethodInfo[] mis = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach (MethodInfo mi in mis)
			{
				if (!mi.Name.StartsWith("Menu_")) continue;
				if (mi.Name.Equals("Menu_" + cp.Replace("\\", "_")))
					return mi;
			}
			return null;
		}
		#endregion

		#region Private Method : MethodInfo GetMethod(string methodName)
		private MethodInfo GetMethod(string methodName)
		{
			return this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
		}
		#endregion

		#region Private Method : string GetMenuHeader()
		private string GetMenuHeader(bool cleanScreen = true)
		{
			StringBuilder sb = new StringBuilder();
			if (cleanScreen)
				sb.Append("{cls}{reset}\x1B[1;1H");
			sb.AppendFormat("{0} v.{1}", System.Windows.Forms.Application.ProductName, System.Windows.Forms.Application.ProductVersion);
			sb.AppendLine();
			sb.AppendLine(AssemblyCopyright);
			sb.AppendFormat("Location : {0}, This TCCU is {1}", _ConfigDB.SelfCarCode, (IsMaster) ? "Master" : "Slave");
			sb.AppendLine();
			return AnsiString.ToANSI(sb.ToString());
		}
		#endregion

		#region Private Method : string GetCommandPrompt(string prompt = "Choose")
		private string GetCommandPrompt(string prompt = "Choose")
		{
			StringBuilder sb = new StringBuilder();
			if (string.IsNullOrEmpty(prompt) || prompt.Equals("Choose", StringComparison.OrdinalIgnoreCase))
				sb.Append(AnsiString.ToANSI(string.Format("{{yellow}}Choose{{reset}}({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}}):", _Telnet.Connections, _Telnet.MaxConnections)));
			else
				sb.Append(AnsiString.ToANSI(string.Format("{{yellow}}{2}{{reset}}", _Telnet.Connections, _Telnet.MaxConnections, prompt)));
			return AnsiString.ToANSI(sb.ToString());
		}
		#endregion

		#region Private Method : string GetMenuFooter(string prompt = "Choose")
		private string GetMenuFooter(string prompt = "Choose")
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("=".PadRight(30, '='));
			sb.Append(GetCommandPrompt(prompt));
			return AnsiString.ToANSI(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M(AsyncClient ac, string parent, string cmd)
		/// <summary>主選單</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("1", "2", "3", "S", "V", "Q")]
		private void Menu_M(AsyncClient ac, string parent, string cmd)
		{
			// 主選單
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] Device Test");
			sb.AppendLine(" [2] Device Status");
			sb.AppendFormat(" [3] Change to {0}", IsMaster ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine(" [S] System Management");
			sb.AppendLine(" [V] Module Version");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, "M", (k, v) => v = "M");
		}
		#endregion

		#region Private Method : void Menu_M_1(AsyncClient ac, string parent, string cmd)
		/// <summary>第一層選單：Device Test</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("1", "2", "3", "4", "5", "6", "7", "M", "B", "Q")]
		private void Menu_M_1(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] PIDS(Car485/TDDU)");
			sb.AppendLine(" [2] TMS SDR");
			sb.AppendLine(" [3] Motorola Radio");
			sb.AppendLine(" [4] Service Intercom");
			sb.AppendLine(" [5] PICU");
			sb.AppendLine(" [6] AECU");
			sb.AppendLine(" [7] PACU");
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_2(AsyncClient ac, string parent, string cmd)
		/// <summary>第一層選單：Device Status Check</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		private void Menu_M_2(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Status Check");
			sb.AppendLine("=".PadRight(30, '='));
			sb.Append(string.Format(" [{0}][{1}] Moxa NPort            [{2}][{3}] TMS                  ", _NPort.Connected ? "o" : " ", _NPort.Alive ? "v" : " ", _NPort.TMS_Connected ? "o" : " ", _NPort.TMS_Alive ? "v" : " "));
			sb.AppendLine(string.Format(" [{0}][{1}] Motorola Radio        [{2}][{3}] Service Intercom      ", _NPort.TETRA_Connected ? "o" : " ", _NPort.TETRA_Alive ? "v" : " ", _SIC.Connected ? "o" : " ", _SIC.Alive ? "v" : " "));
			sb.AppendLine(string.Format(" [{0}][{1}] Voice Encoder(AECU)   [{2}][{3}] PACU                  ", _AECU.Connected ? "o" : " ", _AECU.Alive ? "v" : " ", _PACU.Connected ? "o" : " ", _PACU.Alive ? "v" : " "));
			for (int i = 0; i < PICU_AMOUNT; i++)
			{
				sb.AppendFormat(" [{0}][{1}] {2,-3} PICU-{3,-2}          ", _PICUs[i].Connected ? "o" : " ", _PICUs[i].Alive ? "v" : " ", _PICUs[i].Location, _PICUs[i].LocationCode);
				if (i % 4 == 3)
					sb.AppendLine();
			}
			bool[] pids = _NPort.PIDS_GetAlive();
			sb.AppendLine(string.Format(" [{0}][{1}] PIDS(Car485/TDDU)                                 ", _NPort.PIDS_Connected ? "o" : " ", _NPort.PIDS_Alive ? "v" : " "));
			if (_NPort.PIDS_Connected && _NPort.PIDS_Alive && pids != null)
			{
				sb.AppendLine(string.Format(" [{0}][{1}] > A Side PIDS 1       [{2}][{3}] > B Side PIDS 1       ", "o", pids[0] ? "v" : " ", "o", pids[5] ? "v" : " "));
				sb.AppendLine(string.Format(" [{0}][{1}] > A Side PIDS 2       [{2}][{3}] > B Side PIDS 2       ", "o", pids[1] ? "v" : " ", "o", pids[6] ? "v" : " "));
				sb.AppendLine(string.Format(" [{0}][{1}] > A Side PIDS 3       [{2}][{3}] > B Side PIDS 3       ", "o", pids[2] ? "v" : " ", "o", pids[7] ? "v" : " "));
				sb.AppendLine(string.Format(" [{0}][{1}] > A Side PIDS 4       [{2}][{3}] > B Side PIDS 4       ", "o", pids[3] ? "v" : " ", "o", pids[8] ? "v" : " "));
				sb.AppendLine(string.Format(" [{0}][{1}] > A Side PIDS 5       [{2}][{3}] > B Side PIDS 5       ", "o", pids[4] ? "v" : " ", "o", pids[9] ? "v" : " "));
			}
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_3(AsyncClient ac, string parent, string cmd)
		/// <summary>第一層選單：Change Master</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("1", "2", "M", "B", "Q")]
		private void Menu_M_3(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendFormat(" Main Menu>Change to {0}", IsMaster ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendFormat(" [1] Change to {0} by Manual", IsMaster ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendFormat(" [2] Change to {0} by Communication Control", IsMaster ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" {red}Warning!!!");
			sb.AppendLine(" Switching the master / slave control may cause system instability!!!{reset}");
			sb.Append(GetCommandPrompt());
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_V(AsyncClient ac, string parent, string cmd)
		/// <summary>第一層選單：Module Version</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		private void Menu_M_V(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.Append(SvcWorker.GetModuleVersion());
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_S(AsyncClient ac, string parent, string cmd)
		/// <summary>第一層選單：System Management</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("1", "2", "3", "4", "M", "B", "Q")]
		private void Menu_M_S(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>System Management");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] Shutdown TCCU");
			sb.AppendLine(" [2] Restart TCCU");
			sb.AppendLine(" [3] Shutdown Machine");
			sb.AppendLine(" [4] Restart Machine");
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_1(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\PIDS(Car485/TDDU)</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q", "6")]
		[ExecuteCommand("1", "Exec_PIDS_SendCommand", 1)]
		[ExecuteCommand("2", "Exec_PIDS_SendCommand", 2)]
		[ExecuteCommand("3", "Exec_PIDS_SendCommand", 3)]
		[ExecuteCommand("4", "Exec_PIDS_SendCommand", 4)]
		[ExecuteCommand("5", "Exec_PIDS_SendCommand", 0)]
		private void Menu_M_1_1(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PIDS(Car485/TDDU)");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] Turn On");
			sb.AppendLine(" [2] Turn Off");
			sb.AppendLine(" [3] Show ID/FW");
			sb.AppendLine(" [4] Show Test Mode");
			sb.AppendLine(" [5] Show Arrival/Departure");
			sb.AppendLine(" [6] Samples Demo");
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_1_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\PIDS(Car485/TDDU)\Show Sample</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("1", "Exec_PIDS_SendSample", 0)]
		[ExecuteCommand("2", "Exec_PIDS_SendSample", 1)]
		[ExecuteCommand("3", "Exec_PIDS_SendSample", 2)]
		[ExecuteCommand("4", "Exec_PIDS_SendSample", 3)]
		[ExecuteCommand("5", "Exec_PIDS_SendSample", 4)]
		[ExecuteCommand("6", "Exec_PIDS_SendSample", 5)]
		private void Menu_M_1_1_6(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PIDS(Car485/TDDU)>Show Smaple");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] 到站");
			sb.AppendLine(" [2] 離站");
			sb.AppendLine(" [3] 目的地資訊");
			sb.AppendLine(" [4] 過站不停(市政府)");
			sb.AppendLine(" [5] 木班車");
			sb.AppendLine(" [6] 出門故障");
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_2(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\TMS SDR</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("1", "Exec_TMS_ShowRealTimeSDR", null)]
		[ExecuteCommand("2", "Exec_TMS_CleanSDR", null)]
		private void Menu_M_1_2(AsyncClient ac, string parent, string cmd)
		{
			StringBuilder sb = new StringBuilder();
			TelnetMonitorType tmt = TelnetMonitorType.None;
			bool onMonitor = (_TelnetOnMonitor != null && _TelnetOnMonitor.TryGetValue(ac.RemoteEndPoint, out tmt) && tmt == TelnetMonitorType.SDR);
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			if (onMonitor)
			{
				sb.Append("{reset}{cls}\x1B[1;1H");
				//                      1         2         3         4         5         6         7         8         9         0
				//             12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
				sb.AppendLine("Host Time   :                    |");
				sb.AppendLine("From        :                    | Is Test     :                    | Car Position:                    |");
				sb.AppendLine("OCS Valid   :                    | Car No      :                    | Head Car    :                    |");
				sb.AppendLine("Current Time:                    | Arrival Time:                    |                                  |");
				sb.AppendLine("Loc. Info   :                    | Skipped PID :                    | Trip No.    :                    |");
				sb.AppendLine("Current Stop:                    | Next Stop   :                    | Dest No.    :                    |");
				sb.AppendLine("End Service :                    | Last Train  :                    | No Open Door:                    |");
				sb.AppendLine("Orig Station:                    | Turn Off    :                    | Oper. Mode  :                    |");
				sb.AppendLine("--- DM1 ------------------------------------------------------------------------------------------------");
				sb.AppendLine("Emer Switch :                    | Detection(A):                    | Detection(B):                    |");
				sb.AppendLine("Smoke Dete. :                    | Front       :                    | Rear        :                    |");
				sb.AppendLine("Door Command:                    | Open Cmd.   :                    | Close Cmd.  :                    |");
				sb.AppendLine("Door Status :                    | Opened      :                    | Closed      :                    |");
				sb.AppendLine("Open Side   :                    | Obstacle(A) :                    | Obstacle(B) :                    |");
				sb.AppendLine("                                 | Handler(A)  :                    | Handler(B)  :                    |");
				sb.AppendLine("                                 | Failure(A)  :                    | Failure(B)  :                    |");
				sb.AppendLine("--- DM2 ------------------------------------------------------------------------------------------------");
				sb.AppendLine("Emer Switch :                    | Detection(A):                    | Detection(B):                    |");
				sb.AppendLine("Smoke Dete. :                    | Front       :                    | Rear        :                    |");
				sb.AppendLine("Door Command:                    | Open        :                    | Close       :                    |");
				sb.AppendLine("Door Status :                    | Opened      :                    | Closed      :                    |");
				sb.AppendLine("Open Side   :                    | Obstacle(A) :                    | Obstacle(B) :                    |");
				sb.AppendLine("                                 | Handler(A)  :                    | Handler(B)  :                    |");
				sb.AppendLine("                                 | Failure(A)  :                    | Failure(B)  :                    |");
				sb.AppendLine("--- Raw Data -------------------------------------------------------------------------------------------");
				for (int i = 0; i <= 31; i++)
					sb.AppendFormat("{0:D2} ", i);
				sb.AppendLine("");
				sb.AppendLine("");
				for (int i = 32; i <= 63; i++)
					sb.AppendFormat("{0:D2} ", i);
				sb.AppendLine("");
				sb.AppendLine("");
				sb.AppendLine("--------------------------------------------------------------------------------------------------------");
				sb.AppendFormat("Choose : [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?\x1B[31;56H{{reset}}", _Telnet.Connections, _Telnet.MaxConnections);
			}
			else
			{
				sb.Append(GetMenuHeader(!onMonitor));
				sb.AppendLine(" Main Menu>Device Test>TMS SDR");
				sb.AppendLine("=".PadRight(30, '='));
				sb.AppendLine(" [1] Show Realtime SDR Fields Status");
				sb.AppendLine(" [2] Clean Received SDR");
				sb.AppendLine();
				sb.AppendLine(" [M] Back to Main Menu");
				sb.AppendLine(" [B] Back to Parent Menu");
				sb.AppendLine(" [Q] Quit");
				sb.Append(GetMenuFooter());
			}
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_3(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\Motorola Radio</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("3", "4", "5", "6", "A", "M", "B", "Q")]
		[ExecuteCommand("1", "Exec_TETRA_ShowRadioInfo")]
		[ExecuteCommand("2", "Exec_TETRA_RegistrationCMFT")]
		[ExecuteCommand("7", "Exec_TETRA_GroupCall")]
		[ExecuteCommand("8", "Exec_TETRA_TerminateCall")]
		private void Menu_M_1_3(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>Motorola Radio");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] Radio Device Information");
			sb.AppendLine(" [2] Registration CMFT");
			sb.AppendLine(" [3] Operating Mode");
			sb.AppendLine(" [4] Talk Group");
			sb.AppendLine(" [5] Send SDS");
			sb.AppendLine(" [6] Initiate Call");
			sb.AppendLine(" [7] Group Call(Push PTT)");
			sb.AppendLine(" [8] Terminate Call");
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_4(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\Service Intercom</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("6", "M", "B", "Q")]
		[ExecuteCommand("1", "Exec_SIC_ButtonLED", "ON")]
		[ExecuteCommand("2", "Exec_SIC_ButtonLED", "OFF")]
		[ExecuteCommand("3", "Exec_SIC_ShowStatus", null)]
		private void Menu_M_1_4(AsyncClient ac, string parent, string cmd)
		{
			StringBuilder sb = new StringBuilder();
			TelnetMonitorType tmt = TelnetMonitorType.None;
			bool onMonitor = (_TelnetOnMonitor != null && _TelnetOnMonitor.TryGetValue(ac.RemoteEndPoint, out tmt) && tmt == TelnetMonitorType.SIC);
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			if (onMonitor)
			{
				sb.Append("{reset}{cls}\x1B[1;1H");
				//                      1         2         3         4         5         6         7         8         9         0
				//             12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
				sb.AppendLine("Button 1  :         | LED :     | Button 2  :         | LED :     |");
				sb.AppendLine("Button 3  :         | LED :     | Button 4  :         | LED :     |");
				sb.AppendLine("Button 5  :         | LED :     | Button 6  :         | LED :     |");
				sb.AppendLine("Button 7  :         | LED :     | Button 8  :         | LED :     |");
				sb.AppendLine("Phone PTT :         |           | Phone Hook:         |           |");
				sb.AppendLine("-------------------------------------------------------------------");
				sb.AppendFormat("Choose : [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
				string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
				int row = lines.Length, col = lines[lines.Length - 1].Length + 1;
				sb.AppendFormat("\x1B[{0};{1}H{{reset}}", row, col);
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(row, col), (k, v) => v = new CursorLocation(row, col));
			}
			else
			{
				sb.Append(GetMenuHeader());
				sb.AppendLine(" Main Menu>Device Test>Service Intercom");
				sb.AppendLine("=".PadRight(30, '='));
				sb.AppendLine(" [1] All LED Turn On");
				sb.AppendLine(" [2] All LED Turn Off");
				sb.AppendLine(" [3] Button Status");
				sb.AppendLine(" [6] Restart Controller");
				sb.AppendLine();
				sb.AppendLine(" [M] Back to Main Menu");
				sb.AppendLine(" [B] Back to Parent Menu");
				sb.AppendLine(" [Q] Quit");
				sb.Append(GetMenuFooter());
			}
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_4_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\Service Intercom\Restart Controller</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_SIC_Restart")]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\1\\4")]
		private void Menu_M_1_4_6(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>Service Intercom>Restart Controller");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Restart Service Intercom Controller(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_5(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\PICU</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("4", "5", "6", "M", "B", "Q")]
		[ExecuteCommand("1", "Exec_PICU_ShowStatus", null)]
		[ExecuteCommand("2", "Exec_PICU_SetButtonLED", "ON")]
		[ExecuteCommand("3", "Exec_PICU_SetButtonLED", "OFF")]
		private void Menu_M_1_5(AsyncClient ac, string parent, string cmd)
		{
			StringBuilder sb = new StringBuilder();
			TelnetMonitorType tmt = TelnetMonitorType.None;
			bool onMonitor = (_TelnetOnMonitor != null && _TelnetOnMonitor.TryGetValue(ac.RemoteEndPoint, out tmt) && tmt == TelnetMonitorType.PICU);
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			if (onMonitor)
			{
				sb.Append("{reset}{cls}\x1B[1;1H");
				//                      1         2         3         4         5         6         7         8         9         0
				//             12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
				sb.AppendLine("--- DM1 ---------------------------------------------------------------------------------------------");
				sb.AppendLine("[A1]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("[A3]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("[B2]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("[B5]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("--- DM2 ---------------------------------------------------------------------------------------------");
				sb.AppendLine("[A1]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("[A3]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("[B2]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("[B5]Button    :          | LED      :          | Flash-Off     :          | Flash-On     :          |");
				sb.AppendLine("    MIC Level :          | MIC Mute :          | Speaker Level :          | Speaker Mute :          |");
				sb.AppendLine("    In/Mute   :          | Out/Mute :          | Speaker Source:          | Out Source   :          |");
				sb.AppendLine("-----------------------------------------------------------------------------------------------------");
				sb.AppendFormat("Choose : [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
				string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
				int row = lines.Length, col = lines[lines.Length - 1].Length + 1;
				sb.AppendFormat("\x1B[{0};{1}H{{reset}}", row, col);
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(row, col), (k, v) => v = new CursorLocation(row, col));
			}
			else
			{
				sb.Append(GetMenuHeader(!onMonitor));
				sb.AppendLine(" Main Menu>Device Test>PICU");
				sb.AppendLine("=".PadRight(30, '='));
				sb.AppendLine(" [1] Show Realtime PICU Status");
				sb.AppendLine(" [2] Set All PICU Button LED ON");
				sb.AppendLine(" [3] Set All PICU Button LED OFF");
				sb.AppendLine(" [4] Adjust MIC Volume");
				sb.AppendLine(" [5] Adjust Speaker Volume");
				sb.AppendLine(" [6] Restart Controller");
				sb.AppendLine();
				sb.AppendLine(" [M] Back to Main Menu");
				sb.AppendLine(" [B] Back to Parent Menu");
				sb.AppendLine(" [Q] Quit");
				sb.Append(GetMenuFooter());
			}
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_5_4(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\PICU\Adjust MIC Volume</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[CursorControl(ArrowKeys.UpArrow, "Menu_CursorMoveUp", "PICU", "MIC", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.DownArrow, "Menu_CursorMoveDn", "PICU", "MIC", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.LeftArrow, "Menu_CursorMoveLf", "PICU", "MIC", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.RightArrow, "Menu_CursorMoveRt", "PICU", "MIC", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.Space, "Menu_CursorCheck", "PICU", "MIC", 6)]
		private void Menu_M_1_5_4(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PICU>Adjust MIC Volume");
			sb.AppendLine("=".PadRight(74, '='));
			CursorLocation cl;
			if (!_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl))
			{
				int c = 18;
				int step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / DB_LEVEL_BAR_SIZE;
				c += (_PICUs[0].Register[PICUDevice.RegisterAddresses.MIC_InputLevel] - DB_LEVEL_MIN) / step;
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(6, c), (k, v) => v = new CursorLocation(6, c));
			}
			string tmp = string.Empty;
			for (int i = 0; i < PICU_AMOUNT; i++)
				sb.AppendLine(GetPICU_LevelBar(_PICUs[i], DB_LEVEL_MIN, DB_LEVEL_MAX, cl.Row == i, true));
			//sb.AppendLine("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
			//sb.AppendLine("         1         2         3         4         5         6         7         8         9");
			sb.AppendLine("-".PadRight(74, '-'));
			sb.AppendFormat("Choose : [Space] Set Mute On/OFF, [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
			string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
			sb.AppendFormat("\x1B[{0};{1}H{{reset}}", lines.Length, lines[lines.Length - 1].Length + 1);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_5_5(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\PICU\Adjust Speaker Volume</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[CursorControl(ArrowKeys.UpArrow, "Menu_CursorMoveUp", "PICU", "SPK", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.DownArrow, "Menu_CursorMoveDn", "PICU", "SPK", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.LeftArrow, "Menu_CursorMoveLf", "PICU", "SPK", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.RightArrow, "Menu_CursorMoveRt", "PICU", "SPK", 6, PICU_AMOUNT, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.Space, "Menu_CursorCheck", "PICU", "SPK", 6)]
		private void Menu_M_1_5_5(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PICU>Adjust Speaker Volume");
			sb.AppendLine("=".PadRight(74, '='));
			CursorLocation cl;
			if (!_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl))
			{
				int c = 18;
				int step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / DB_LEVEL_BAR_SIZE;
				c += (_PICUs[0].Register[PICUDevice.RegisterAddresses.SpeakerOutputLevel] - DB_LEVEL_MIN) / step;
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(6, c), (k, v) => v = new CursorLocation(6, c));
			}
			string tmp = string.Empty;
			for (int i = 0; i < PICU_AMOUNT; i++)
				sb.AppendLine(GetPICU_LevelBar(_PICUs[i], DB_LEVEL_MIN, DB_LEVEL_MAX, cl.Row == i, false));
			//sb.AppendLine("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
			//sb.AppendLine("         1         2         3         4         5         6         7         8         9");
			sb.AppendLine("-".PadRight(74, '-'));
			sb.AppendFormat("Choose : [Space] Set Mute On/OFF, [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
			string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
			sb.AppendFormat("\x1B[{0};{1}H{{reset}}", lines.Length, lines[lines.Length - 1].Length + 1);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_5_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\PICU\Restart Controller</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_PICU_Restart")]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\1\\5")]
		private void Menu_M_1_5_6(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PICU>Restart Controller");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Restart All PICU Controller(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\AECU</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("2", "3", "6", "M", "B", "Q")]
		[ExecuteCommand("1", "Exec_AECU_ShowStatus", null)]
		private void Menu_M_1_6(AsyncClient ac, string parent, string cmd)
		{
			StringBuilder sb = new StringBuilder();
			TelnetMonitorType tmt = TelnetMonitorType.None;
			bool onMonitor = (_TelnetOnMonitor != null && _TelnetOnMonitor.TryGetValue(ac.RemoteEndPoint, out tmt) && tmt == TelnetMonitorType.AECU);
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			if (onMonitor)
			{
				sb.Append("{reset}{cls}\x1B[1;1H");
				//                      1         2         3         4         5         6         7         8         9         0
				//             12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
				sb.AppendFormat("--- {0} ---------------------------------------------------------------------------------------------", _ConfigDB.SelfCarCode);
				sb.AppendLine();
				sb.AppendLine("AMP Out Level :         | AMP Out Mute :         |");
				sb.AppendLine("In/Ch1 Level  :         | In/Ch1 Mute  :         | In/Ch1 Power  :         | In/Ch1 Sen.  :         |");
				sb.AppendLine("In/Ch2 Level  :         | In/Ch2 Mute  :         | In/Ch2 Power  :         | In/Ch2 Sen.  :         |");
				sb.AppendLine("Out/Ch1 Level :         | Out/Ch1 Mute :         | Out/Ch2 Level :         | Out/Ch2 Mute :         |");
				sb.AppendLine("Audio Input Mixer Mute  :                                                                           |");
				sb.AppendLine("Audio Output Mixer Mute :                                                                           |");
				sb.AppendLine("Output Ch1 Source Select:                        | Output Ch2 Source Select:                        |");
				sb.AppendLine("AMP Output Source Select:                        |");
				sb.AppendLine("To PICU Source Select   :                        | To PACU Source Select   :                        |");
				sb.AppendLine("To Record Source Select :                                                                           |");
				sb.AppendLine("-----------------------------------------------------------------------------------------------------");
				sb.AppendFormat("Choose : [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
				string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
				int row = lines.Length, col = lines[lines.Length - 1].Length + 1;
				sb.AppendFormat("\x1B[{0};{1}H{{reset}}", row, col);
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(row, col), (k, v) => v = new CursorLocation(row, col));
			}
			else
			{
				sb.Append(GetMenuHeader(!onMonitor));
				sb.AppendLine(" Main Menu>Device Test>AECU");
				sb.AppendLine("=".PadRight(30, '='));
				sb.AppendLine(" [1] Show Realtime AECU Status");
				sb.AppendLine(" [2] Adjust Volume");
				sb.AppendLine(" [3] Output Source Select");
				sb.AppendLine(" [6] Restart Controller");
				sb.AppendLine();
				sb.AppendLine(" [M] Back to Main Menu");
				sb.AppendLine(" [B] Back to Parent Menu");
				sb.AppendLine(" [Q] Quit");
				sb.Append(GetMenuFooter());
			}
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_6_3(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\AECU\Output Source Select</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[CursorControl(ArrowKeys.UpArrow, "Menu_CursorMoveUp", "AECU", "OutSrc", 6, 6, 0)]
		[CursorControl(ArrowKeys.DownArrow, "Menu_CursorMoveDn", "AECU", "OutSrc", 6, 6, 0)]
		[CursorControl(ArrowKeys.Space, "Menu_CursorCheck", "AECU", "OutSrc", 6)]
		private void Menu_M_1_6_3(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>AECU>Output Source Select");
			sb.AppendLine("=".PadRight(81, '='));
			CursorLocation cl;
			if (!_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl))
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(6, 0), (k, v) => v = new CursorLocation(6, 0));
			//                      1         2         3         4         5         6         7         8         9         0
			//             12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
			for (int i = 0; i <= 5; i++)
				sb.AppendLine(GetAECU_OutSelectBar(GetAECU_OutSourceRegister(i), cl.Row == i));
			sb.AppendLine("-".PadRight(81, '-'));
			sb.AppendLine("[Up/Down] Move Light Bar, [Space] Select, [Enter] Setting, Or.");
			sb.AppendFormat("[M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
			string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
			sb.AppendFormat("\x1B[{0};{1}H{{reset}}", lines.Length, lines[lines.Length - 1].Length + 1);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_6_2(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\AECU\Adjust Volume</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[CursorControl(ArrowKeys.UpArrow, "Menu_CursorMoveUp", "AECU", null, 6, 5, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.DownArrow, "Menu_CursorMoveDn", "AECU", null, 6, 5, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.LeftArrow, "Menu_CursorMoveLf", "AECU", null, 6, 5, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.RightArrow, "Menu_CursorMoveRt", "AECU", null, 6, 5, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.Space, "Menu_CursorCheck", "AECU", null, 6)]
		private void Menu_M_1_6_2(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>AECU>Adjust Volume");
			sb.AppendLine("=".PadRight(79, '='));
			CursorLocation cl;
			if (!_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl))
			{
				int c = 18;
				int step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / DB_LEVEL_BAR_SIZE;
				c += (_AECU.Register[AECUDevice.RegisterAddresses.AMP_OutLevel] - DB_LEVEL_MIN) / step;
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(6, c), (k, v) => v = new CursorLocation(6, c));
			}
			for (int i = 0; i <= 4; i++)
				sb.AppendLine(GetAECU_LevelBar(GetAECU_LevelRegister(i), DB_LEVEL_MIN, DB_LEVEL_MAX, cl.Row == i));
			//sb.AppendLine("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
			//sb.AppendLine("         1         2         3         4         5         6         7         8         9");
			sb.AppendLine("-".PadRight(79, '-'));
			sb.AppendFormat("Choose : [Space] Set Mute On/OFF, [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
			string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
			sb.AppendFormat("\x1B[{0};{1}H{{reset}}", lines.Length, lines[lines.Length - 1].Length + 1);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_6_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\AECU\Restart Controller</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_AECU_Restart")]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\1\\6")]
		private void Menu_M_1_6_6(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>AECU>Restart Controller");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Restart AECU Controller(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_7(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Device Test\PACU</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("2", "6", "M", "B", "Q")]
		[ExecuteCommand("1", "Exec_PACU_ShowStatus", null)]
		private void Menu_M_1_7(AsyncClient ac, string parent, string cmd)
		{
			StringBuilder sb = new StringBuilder();
			TelnetMonitorType tmt = TelnetMonitorType.None;
			bool onMonitor = (_TelnetOnMonitor != null && _TelnetOnMonitor.TryGetValue(ac.RemoteEndPoint, out tmt) && tmt == TelnetMonitorType.PACU);
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			if (onMonitor)
			{
				sb.Append("{reset}{cls}\x1B[1;1H");
				//                      1         2         3         4         5         6         7         8         9         0
				//             12345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234
				sb.AppendFormat("--- {0} ---------------------------------------------------------------------------------------------", _ConfigDB.SelfCarCode);
				sb.AppendLine();
				sb.AppendLine("AMP Out/Ch1 Lv :       | AMP Out/Ch1 Mute :       | AMP Out/Ch2 Lv :       | AMP Out/Ch2 Mute :       |");
				sb.AppendLine("Audio In/Ch1 Lv:       | Audio In/Ch1 Mute:       | Audio In/Ch2 Lv:       | Audio In/Ch2 Mute:       |");
				sb.AppendLine("Audio In/Ch3 Lv:       | Audio In/Ch3 Mute:       | Audio In/Ch4 Lv:       | Audio In/Ch4 Mute:       |");
				sb.AppendLine("Input Ch1 Level:       | Input Ch1 Mute   :       | Input Ch1 Power:       | Input Ch1 Sen.   :       |");
				sb.AppendLine("Input Ch2 Level:       | Input Ch2 Mute   :       | Input Ch2 Power:       | Input Ch2 Sen.   :       |");
				sb.AppendLine("Input Ch3 Level:       | Input Ch3 Mute   :       | Input Ch3 Power:       | Input Ch3 Sen.   :       |");
				sb.AppendLine("Input Ch4 Level:       | Input Ch4 Mute   :       | Input Ch4 Power:       | Input Ch4 Sen.   :       |");
				sb.AppendLine("Message Out Lv :       | Message Out Mute :       |");
				sb.AppendLine("Audio Input Mixer Mute :                                                                              |");
				sb.AppendLine("Audio Output Mixer Mute:                                                                              |");
				sb.AppendLine("Out/Ch1 Source Select  :                          | Out/Ch2 Source Select  :                          |");
				sb.AppendLine("Out/Ch3 Source Select  :                          | Out/Ch4 Source Select  :                          |");
				sb.AppendLine("AMP Out-1 Source Select:                          | AMP Out-2 Source Select:                          |");
				sb.AppendLine("USB Rec. Source Select :                          |");
				sb.AppendLine("-----------------------------------------------------------------------------------------------------");
				sb.AppendFormat("Choose : [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
				string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
				int row = lines.Length, col = lines[lines.Length - 1].Length + 1;
				sb.AppendFormat("\x1B[{0};{1}H{{reset}}", row, col);
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(row, col), (k, v) => v = new CursorLocation(row, col));
			}
			else
			{
				sb.Append(GetMenuHeader(!onMonitor));
				sb.AppendLine(" Main Menu>Device Test>PACU");
				sb.AppendLine("=".PadRight(30, '='));
				sb.AppendLine(" [1] Show Realtime PACU Status");
				sb.AppendLine(" [2] Adjust Volume");
				sb.AppendLine(" [6] Restart Controller");
				sb.AppendLine();
				sb.AppendLine(" [M] Back to Main Menu");
				sb.AppendLine(" [B] Back to Parent Menu");
				sb.AppendLine(" [Q] Quit");
				sb.Append(GetMenuFooter());
			}
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_7_2(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\PACU\Adjust Volume</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[CursorControl(ArrowKeys.UpArrow, "Menu_CursorMoveUp", "PACU", null, 6, 10, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.DownArrow, "Menu_CursorMoveDn", "PACU", null, 6, 10, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.LeftArrow, "Menu_CursorMoveLf", "PACU", null, 6, 10, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.RightArrow, "Menu_CursorMoveRt", "PACU", null, 6, 10, 18, DB_LEVEL_BAR_SIZE)]
		[CursorControl(ArrowKeys.Space, "Menu_CursorCheck", "PACU", null, 6)]
		private void Menu_M_1_7_2(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PACU>Adjust Volume");
			sb.AppendLine("=".PadRight(74, '='));
			CursorLocation cl;
			if (!_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl))
			{
				int c = 18;
				int step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / DB_LEVEL_BAR_SIZE;
				c += (_PACU.Register[PACUDevice.RegisterAddresses.AMP_Ch1_Level] - DB_LEVEL_MIN) / step;
				_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, new CursorLocation(6, c), (k, v) => v = new CursorLocation(6, c));
			}
			string tmp = string.Empty;
			for (int i = 0; i <= 9; i++)
				sb.AppendLine(GetPACU_LevelBar(GetPACU_LevelRegister(i), DB_LEVEL_MIN, DB_LEVEL_MAX, cl.Row == i));
			//sb.AppendLine("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
			//sb.AppendLine("         1         2         3         4         5         6         7         8         9");
			sb.AppendLine("-".PadRight(74, '-'));
			sb.AppendFormat("Choose : [Space] Set Mute On/OFF, [M]ain Menu, [B]ack to Parent, [Q]uit - ({{cyan}}{0}{{reset}}/{{cyan}}{1}{{reset}})?", _Telnet.Connections, _Telnet.MaxConnections);
			string[] lines = AnsiString.RemoveANSI(sb.ToString()).Split('\n');
			sb.AppendFormat("\x1B[{0};{1}H{{reset}}", lines.Length, lines[lines.Length - 1].Length + 1);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_1_7_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\PACU\Restart Controller</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_PACU_Restart")]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\1\\7")]
		private void Menu_M_1_7_6(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>PACU>Restart Controller");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Restart PACU Controller(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_3_3(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\Motorola Radio\Operating Mode</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("1", "Exec_TETRA_ChangeOperatingMode", 0)]	// TMO
		[ExecuteCommand("2", "Exec_TETRA_ChangeOperatingMode", 1)]	// DMO
		[ExecuteCommand("3", "Exec_TETRA_ChangeOperatingMode", 5)]	// DM-Gateway
		[ExecuteCommand("4", "Exec_TETRA_ChangeOperatingMode", 6)]	// DM-Repeater
		private void Menu_M_1_3_3(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>Motorola Radio>Operating Mode");
			sb.AppendLine(" Current Operating Mode:" + _NPort.TETRA_OperatingMode);
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [1] Trunked Mode Operations");
			sb.AppendLine(" [2] Direct Mode Operations");
			sb.AppendLine(" [3] Direct Mode Gateway");
			sb.AppendLine(" [4] Direct Mode Repeater");
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_3_4(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\Motorola Radio\Change TalkGroup</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("1", "Exec_TETRA_ChangeRadioTalkGroup", "TalkGroup_1")]
		[ExecuteCommand("2", "Exec_TETRA_ChangeRadioTalkGroup", "TalkGroup_2")]
		[ExecuteCommand("3", "Exec_TETRA_ChangeRadioTalkGroup", "TalkGroup_3")]
		[ExecuteCommand("4", "Exec_TETRA_ChangeRadioTalkGroup", "TalkGroup_4")]
		[ExecuteCommand("5", "Exec_TETRA_ChangeRadioTalkGroup", "TalkGroup_5")]
		private void Menu_M_1_3_4(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>Motorola Radio>Change TalkGroup");
			sb.AppendLine(" Current TalkGroup:" + _NPort.TETRA_TalkGroup);
			sb.AppendLine("=".PadRight(30, '='));
			for (int i = 1; i <= _NPort.TETRA_TalkGroupList.Length; i++)
			{
				sb.AppendFormat(" [{0}] {1,-12} : GSSI={2}", i, _NPort.TETRA_TalkGroupList[i - 1].Split(':'));
				sb.AppendLine();
			}
			sb.AppendLine();
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter());
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_1_3_5(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\Motorola Radio\Send SDS</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[InputCommand("Exec_TETRA_SendMessage", "Target SSID:", null, "Messages:", null)]
		private void Menu_M_1_3_5(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>Motorola Radio>Send SDS");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			MethodInfo mi = GetMenuMethod(currentPath);
			InputCommandAttribute ica = (InputCommandAttribute)mi.GetCustomAttributes(typeof(InputCommandAttribute), false)[0];
			string[] prompts = new string[ica.Prompts.Keys.Count];
			ica.Prompts.Keys.CopyTo(prompts, 0);
			sb.Append(GetMenuFooter(prompts[0]));
			ac.SendData(sb.ToString());
			_Telnet.SetCommandEndChar(ac.RemoteEndPoint, TelnetServer.CommandEndCharType.CrLf);
		}
		#endregion

		#region Private Method : void Menu_M_1_3_6(AsyncClient ac, string parent, string cmd)
		/// <summary>第三層選單：Device Test\Motorola Radio\Initiate Call</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[InputCommand("Exec_TETRA_InitiateCall", "Duplex call(Y/N)?", new string[] { "Y", "N" }, "SSID:", null)]
		private void Menu_M_1_3_6(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>Device Test>Motorola Radio>Initiate Call");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			MethodInfo mi = GetMenuMethod(currentPath);
			InputCommandAttribute ica = (InputCommandAttribute)mi.GetCustomAttributes(typeof(InputCommandAttribute), false)[0];
			string[] prompts = new string[ica.Prompts.Keys.Count];
			ica.Prompts.Keys.CopyTo(prompts, 0);
			sb.Append(GetMenuFooter(prompts[0]));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_3_1(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Change Master</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_ChangeMasterByManual", false, null)]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M")]
		private void Menu_M_3_1(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendFormat(" Main Menu>Change to {0} by Manual", IsMaster ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" {red}Warning!!!");
			sb.AppendLine(" Switching the master / slave control may cause system instability!!!{reset}");
			sb.Append(string.Format("Are you sure change {{yellow}}{0}{{reset}} to {{yellow}}{1}{{reset}}(Y/N)?", _ConfigDB.SelfCarCode, IsMaster ? "Slave" : "Master"));
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_3_2(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：Change Master by Communication Control</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_ChangeMasterByCommunication", false, null)]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M")]
		private void Menu_M_3_2(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendFormat(" Main Menu>Change to {0} by Communication Control", IsMaster ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" {red}Warning!!!");
			sb.AppendLine(" Switching the master / slave control may cause system instability!!!{reset}");
			sb.Append(string.Format("Are you sure change {{yellow}}{0}{{reset}} to {{yellow}}{1}{{reset}}(Y/N)?", _ConfigDB.SelfCarCode, IsMaster ? "Slave" : "Master"));
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
		}
		#endregion

		#region Private Method : void Menu_M_S_1(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：System Management\Shutdown TCCU</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_Shutdown", false, CommandType.Shutdown_TCCU)]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\S")]
		private void Menu_M_S_1(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>System Management>Shutdown TCCU");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Shutdown TCCU(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_S_2(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：System Management\Restart TCCU</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_Shutdown", false, CommandType.Restart_TCCU)]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\S")]
		private void Menu_M_S_2(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>System Management>Restart TCCU");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Restart TCCU(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_S_3(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：System Management\Shutdown Machine</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_Shutdown", false, CommandType.Shutdown_Machine)]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\S")]
		private void Menu_M_S_3(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>System Management>Shutdown Machine");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Shutdown Machine(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Menu_M_S_4(AsyncClient ac, string parent, string cmd)
		/// <summary>第二層選單：System Management\Shutdown Machine</summary>
		/// <param name="ac">使用者端點連線類別</param>
		/// <param name="parent">上層路徑</param>
		/// <param name="cmd">指令</param>
		[MenuCommand("M", "B", "Q")]
		[ExecuteCommand("Y", "Exec_Shutdown", false, CommandType.Restart_Machine)]
		[ExecuteCommand("N", "Exec_BackMenu", false, "M\\S")]
		private void Menu_M_S_4(AsyncClient ac, string parent, string cmd)
		{
			string currentPath = parent;
			if (!string.IsNullOrEmpty(cmd)) currentPath += "\\" + cmd;
			_MenuPath.AddOrUpdate(ac.RemoteEndPoint, currentPath, (k, v) => v = currentPath);
			StringBuilder sb = new StringBuilder();
			sb.Append(GetMenuHeader());
			sb.AppendLine(" Main Menu>System Management>Restart Machine");
			sb.AppendLine("=".PadRight(30, '='));
			sb.AppendLine(" [M] Back to Main Menu");
			sb.AppendLine(" [B] Back to Parent Menu");
			sb.AppendLine(" [Q] Quit");
			sb.Append(GetMenuFooter("Restart Machine(Y/N)?"));
			ac.SendData(sb.ToString());
		}
		#endregion

		#region Private Method : void Exec_BackMenu(AsyncClient ac, object[] data)
		private void Exec_BackMenu(AsyncClient ac, object[] data)
		{
			string[] arr = data[0].ToString().Split('\\');
			string goPath = string.Join("\\", arr, 0, arr.Length - 1);
			//MethodInfo mi = GetMenuMethod(goPath);
			MethodInfo mi = GetMenuMethod(data[0].ToString());
			mi.Invoke(this, new object[] { ac, goPath, arr[arr.Length - 1] });
			//_MenuPath.AddOrUpdate(ac.RemoteEndPoint, data[0].ToString(), (k, v) => v = data[0].ToString());
		}
		#endregion

		#region Private Method : void Exec_ChangeMasterByManual(AsyncClient ac, object[] data)
		private void Exec_ChangeMasterByManual(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Master Change", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Change to \"{1}\" by Manual", tcs.HasValue ? tcs.Value.UserID : "Unknow", IsMaster ? "Slave" : "Master");
			bool orig = IsMaster;
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{green}}{0}{{reset}} changing to {{green}}{1}{{reset}}, please wait...", _ConfigDB.SelfCarCode, IsMaster ? "Master" : "Slave");
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			//SwitchToMaster(!IsMaster);
			SwitchExecuteMode((_ExecMode == ExecuteModes.Master) ? ExecuteModes.Slave : ExecuteModes.Master, SwitchReason.Manual);
			sb.Clear();
			sb.AppendLine();
			sb.AppendFormat("{{green}}{0}{{reset}} has changed to {{green}}{1}{{reset}}..", _ConfigDB.SelfCarCode, IsMaster ? "Master" : "Slave");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
			Exec_BackMenu(ac, new object[] { "M\\3" });
		}
		#endregion

		#region Private Method : void Exec_ChangeMasterByCommunication(AsyncClient ac, object[] data)
		private void Exec_ChangeMasterByCommunication(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Change to {0} by Communication", _isMaster ? "Slave" : "Master");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Change to \"{1}\" by Communication", tcs.HasValue ? tcs.Value.UserID : "Unknow", IsMaster ? "Slave" : "Master");
			bool orig = IsMaster;
			bool res = MasterSwitch(!IsMaster, SwitchReason.Manual);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			if (res)
				sb.AppendFormat("{{green}}{0}{{reset}} has Changed to {{green}}{1}{{reset}}..", _ConfigDB.SelfCarCode, IsMaster ? "Master" : "Slave");
			else
				sb.AppendFormat("{{red}}{0} change to {1}{{reset}} failure..", _ConfigDB.SelfCarCode, orig ? "Slave" : "Master");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
			Exec_BackMenu(ac, new object[] { "M\\3" });
		}
		#endregion

		#region Private Method : void Exec_PIDS_SendCommand(AsyncClient ac, object[] data)
		private void Exec_PIDS_SendCommand(AsyncClient ac, object[] data)
		{
			PIDS_Commands cmd = (PIDS_Commands)Convert.ToInt32(data[0]);
			//Console.WriteLine("Telnet:Send PIDS Command:{0}", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Send PIDS Command:{1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", cmd);
			_NPort.PIDS_SendCommand(cmd);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Command \"{{cyan}}{0}{{reset}}\" sended, please check all PIDS.", cmd.ToString());
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_PIDS_SendSample(AsyncClient ac, object[] data)
		private void Exec_PIDS_SendSample(AsyncClient ac, object[] data)
		{
			PIDS_Samples cmd = (PIDS_Samples)Convert.ToUInt16(data[0]);
			//Console.WriteLine("Telnet:Send PIDS Sample:{0}", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Send PIDS Sample:{1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", cmd);
			_NPort.PIDS_SendSample(cmd);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Command \"{{cyan}}{0}{{reset}}\" sended, please check all PIDS.", cmd.ToString());
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TMS_ShowRealTimeSDR(AsyncClient ac, object[] data)
		private void Exec_TMS_ShowRealTimeSDR(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Show Real Time SDR", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Show Realtime SDR", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			_TelnetOnMonitor.AddOrUpdate(ac.RemoteEndPoint, TelnetMonitorType.SDR, (k, v) => v = TelnetMonitorType.SDR);
			ac.SendData(AnsiString.ToANSI("{{reset}}{{cls}}"));
			_NPort.TMS_ShowRealTimeSDR(ac);
		}
		#endregion

		#region Private Method : void Exec_TMS_CleanSDR(AsyncClient ac, object[] data)
		private void Exec_TMS_CleanSDR(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Clean Received SDR", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Clean Received SDR", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			_NPort.TMS_CleanSDR(ac);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Received SDR Clean Done.");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_PICU_ShowStatus(AsyncClient ac, object[] data)
		private void Exec_PICU_ShowStatus(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Show PICU Status", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Show PICU Status", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			_TelnetOnMonitor.AddOrUpdate(ac.RemoteEndPoint, TelnetMonitorType.PICU, (k, v) => v = TelnetMonitorType.PICU);
			ac.SendData(AnsiString.ToANSI("{{reset}}{{cls}}"));
			System.Threading.Tasks.Task.Factory.StartNew(() => PICU_ShowStatus(ac));
		}
		#endregion

		#region Private Method : void Exec_PICU_SetButtonLED(AsyncClient ac, object[] data)
		private void Exec_PICU_SetButtonLED(AsyncClient ac, object[] data)
		{
			string cmd = data[0].ToString();
			//Console.WriteLine("Telnet:Set All PICU Button LED : {0}", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Set All PICU Button LED to {1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", cmd);
			for (int i = 0; i < PICU_AMOUNT; i++)
				_PICUs[i].SetButtonLED(cmd.Equals("ON", StringComparison.OrdinalIgnoreCase));
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Set All PICU Button LED {{green}}{0}{{reset}} Done.", cmd.ToUpper());
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_AECU_ShowStatus(AsyncClient ac, object[] data)
		private void Exec_AECU_ShowStatus(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Show AECU Status", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Show AECU Status", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			_TelnetOnMonitor.AddOrUpdate(ac.RemoteEndPoint, TelnetMonitorType.AECU, (k, v) => v = TelnetMonitorType.AECU);
			ac.SendData(AnsiString.ToANSI("{{reset}}{{cls}}"));
			System.Threading.Tasks.Task.Factory.StartNew(() => AECU_ShowStatus(ac));
		}
		#endregion

		#region Private Method : void Exec_PACU_ShowStatus(AsyncClient ac, object[] data)
		private void Exec_PACU_ShowStatus(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Show PACU Status", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Show PACU Status", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			_TelnetOnMonitor.AddOrUpdate(ac.RemoteEndPoint, TelnetMonitorType.PACU, (k, v) => v = TelnetMonitorType.PACU);
			ac.SendData(AnsiString.ToANSI("{{reset}}{{cls}}"));
			System.Threading.Tasks.Task.Factory.StartNew(() => PACU_ShowStatus(ac));
		}
		#endregion

		#region Private Method : void Exec_TETRA_RegistrationCMFT(AsyncClient ac, object[] data)
		private void Exec_TETRA_RegistrationCMFT(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:TETRA Registration CMFT");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Registration CMFT", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			TETRA_RegistrationProcess();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendLine("{green}Registered to CMFT {reset}.");
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_ChangeRadioTalkGroup(AsyncClient ac, object[] data)
		private void Exec_TETRA_ChangeRadioTalkGroup(AsyncClient ac, object[] data)
		{
			int gid = _ConfigDB.IntValue(VarGroups.TETRA, data[0].ToString());
			//Console.WriteLine("Telnet:ChangeTalkGroup:{0}", gid);
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Change TalkGroup to {1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", gid);
			bool res = _NPort.TETRA_ChangeGroup(gid);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}TalkGroup has changed to {{cyan}}{0}{{reset}} {1}{{reset}}, please check Radio.", gid, res ? "{green}Success" : "{red}Failure");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_ChangeOperatingMode(AsyncClient ac, object[] data)
		private void Exec_TETRA_ChangeOperatingMode(AsyncClient ac, object[] data)
		{
			TETRA_RedioOperatingMode mode = (TETRA_RedioOperatingMode)Convert.ToInt32(data[0]);
			//Console.WriteLine("Telnet:ChangeOperatingMode:{0}", mode);
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Change Operating Mode:{1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", mode);
			bool res = _NPort.TETRA_ChangeOperatingMode(mode);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Operating Mode has changed to {{cyan}}{0}{{reset}} {1}{{reset}}, please check Radio.", mode, res ? "{green}Success" : "{red}Failure");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_ReadRadioTalkGroup(AsyncClient ac, object[] data)
		private void Exec_TETRA_ReadRadioTalkGroup(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:ReadTalkGroup");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Read Radio Current TalkGroup", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			string gid = _NPort.TETRA_ReadTalkGroup();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			if (!string.IsNullOrEmpty(gid))
				sb.AppendFormat("{{reset}}Radio Current TalkGroup GSSI is {{cyan}}{0}{{reset}}. ", gid);
			else
				sb.AppendFormat("{{reset}}Can't read Radio Current TalkGroup GSSI {{reset}}. ");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_ReadRadioTalkGroupList(AsyncClient ac, object[] data)
		private void Exec_TETRA_ReadRadioTalkGroupList(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:ReadTalkGroupList");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Read Radio TalkGroup List", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			string[] gids = _NPort.TETRA_ReadTalkGroupList();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			if (gids != null)
			{
				sb.AppendLine("{reset}Radio TalkGroup List is :");
				foreach (string s in gids)
				{
					sb.AppendFormat("{{cyan}}{0}{{reset}}. ", s);
					sb.AppendLine();
				}
			}
			else
				sb.AppendFormat("{{reset}}Can't read Radio TalkGroup List {{reset}}. ");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_ReadRadioOperatingMode(AsyncClient ac, object[] data)
		private void Exec_TETRA_ReadRadioOperatingMode(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:ReadRadioOperatingMode");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Read Radio Current Operating Mode", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			TETRA_RedioOperatingMode? mode = _NPort.TETRA_ReadOperatingMode();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Radio Current Operating Mode is {{cyan}}{0}{{reset}}.", mode.HasValue ? mode.ToString() : "{red}Unknow");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_SendMessage(AsyncClient ac, object[] data)
		private void Exec_TETRA_SendMessage(AsyncClient ac, object[] data)
		{
			string ssid = data[0].ToString();
			string text = data[1].ToString();
			//Console.WriteLine("Telnet:Send SDS {0} to {1}", text, ssid);
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Send SDS {1} to {2}", tcs.HasValue ? tcs.Value.UserID : "Unknow", text, ssid);
			string res = _NPort.TETRA_SendMessage(ssid, text);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Send SDS {{cyan}}{0}{{reset}} to {{cyan}}{1}{{reset}} {2}{{reset}}.", text, ssid, (res == "Success" ? "{green}Success" : "{red}Failure"));
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()), _Encoding);
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_ShowRadioInfo(AsyncClient ac, object[] data)
		private void Exec_TETRA_ShowRadioInfo(AsyncClient ac, object[] data)
		{
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Show Info", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			string txt = _NPort.TETRA_ShowRadioInfo();
			ac.SendData("\r\n" + AnsiString.ToANSI(txt));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_InitiateCall(AsyncClient ac, object[] data)
		private void Exec_TETRA_InitiateCall(AsyncClient ac, object[] data)
		{
			bool isFull = data[0].ToString().Equals("Y", StringComparison.OrdinalIgnoreCase);
			string ssid = data[1].ToString();
			//Console.WriteLine("Telnet:Initiate Call({0}) to {1}", (isFull ? "Duplex" : "Simplex"), ssid);
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Initiate({1}) Call to {2}", tcs.HasValue ? tcs.Value.UserID : "Unknow", (isFull ? "Duplex" : "Simplex"), ssid);

			bool res = _NPort.TETRA_InitiateCall(isFull, ssid);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Initiate Call({{cyan}}{0}{{reset}}) to {{cyan}}{1}{{reset}} {2}{{reset}}.", (isFull ? "Duplex" : "Simplex"), ssid, res ? "{green}Success" : "{red}Failure");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_TerminateCall(AsyncClient ac, object[] data)
		private void Exec_TETRA_TerminateCall(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:TETRA TerminateCall");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Terminate Call", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			bool res = _NPort.TETRA_TerminateCall();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Terminate Call {0}{{reset}}. ", res ? "{green}Success" : "{red}Failure");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_TETRA_GroupCall(AsyncClient ac, object[] data)
		private void Exec_TETRA_GroupCall(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:TETRA GroupCall");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:TETRA Group Call", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			bool res = _NPort.TETRA_GroupCall();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			if (res)
				sb.AppendLine("{reset}Group Call is {green}ready{reset}. ");
			else
				sb.AppendLine("{red}Can't use Group Call!!!{reset}. ");
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_Shutdown(AsyncClient ac, object[] data)
		private void Exec_Shutdown(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Shutdown TCCU");
			CommandType pct = (CommandType)Convert.ToInt32(data[0]);
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Warn, "[TELNET][{0}]MG:{1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", pct);
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("{{cls}}{{reset}}TCCU Will be {{green}}{0}{{reset}} after {{red}}5{{reset}} second... ", pct.ToString().Replace('_', ' '));
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			ac.Close();
			if (this.OnCommand != null)
				this.OnCommand.BeginInvoke(this, pct, 5000, null, null);
		}
		#endregion

		#region Private Method : string GetPICU_LevelBar(PICUDevice picu, int min, int max, bool focus, bool isMic)
		private string GetPICU_LevelBar(PICUDevice picu, int min, int max, bool focus, bool isMic)
		{
			short val = 0;
			PICUDevice.RegisterAddresses ra = isMic ? PICUDevice.RegisterAddresses.MIC_InputMute : PICUDevice.RegisterAddresses.SpeakerOutputMute;
			picu.Register.TryGetValue(ra, out val);
			bool mute = (val == 1);
			ra = isMic ? PICUDevice.RegisterAddresses.MIC_InputLevel : PICUDevice.RegisterAddresses.SpeakerOutputLevel;
			picu.Register.TryGetValue(ra, out val);
			string res = "#".PadRight(DB_LEVEL_BAR_SIZE, '#') + "{reset}";
			int step = (max - min) / DB_LEVEL_BAR_SIZE;
			int addr = (val - min) / step;
			res = res.Insert(addr, "{d_gray}").Insert(0, "{green}");
			res = string.Format("{5}{6} {0} PICU{1}/{2}  {4}{5}  [{{green}}{3,3}{{reset}}{5}] [{{reset}}{7}Mute{{reset}}{5}] {{reset}}",
				picu.Location, (picu.Index - 1) % 4 + 1, picu.LocationCode, val, res, focus ? "{!d_blue}{white}" : "{gray}", focus ? " >" : "  ", mute ? (focus ? "{!d_blue}{red}" : "{red}") : (focus ? "{!d_blue}{black}" : "{d_gray}"));
			return res;
		}
		#endregion

		#region Private Method : string GetAECU_LevelBar(VoiceEncoder.RegisterAddresses reg, int min, int max, bool focus)
		private string GetAECU_LevelBar(AECUDevice.RegisterAddresses reg, int min, int max, bool focus)
		{
			short val = 0;
			_AECU.Register.TryGetValue(reg + 1, out val);
			bool mute = (val == 1);
			_AECU.Register.TryGetValue(reg, out val);
			string res = "#".PadRight(DB_LEVEL_BAR_SIZE, '#') + "{reset}";
			int step = (max - min) / DB_LEVEL_BAR_SIZE;
			int addr = (val - min) / step;
			res = res.Insert(addr, "{d_gray}").Insert(0, "{green}");
			res = string.Format("{3}{4} [{0,-15}]  {2}{3}  [{{green}}{1,3}{{reset}}{3}] [{{reset}}{5}Mute{{reset}}{3}] {{reset}}",
				reg.ToString().Replace('_', ' '), val, res, focus ? "{!d_blue}{white}" : "{gray}", focus ? " >" : "  ", mute ? (focus ? "{!d_blue}{red}" : "{red}") : (focus ? "{!d_blue}{black}" : "{d_gray}"));
			return res;
		}
		#endregion

		#region Private Method : string GetAECU_OutSelectBar(VoiceEncoder.RegisterAddresses reg, bool focus)
		private string GetAECU_OutSelectBar(AECUDevice.RegisterAddresses reg, bool focus)
		{
			short val = 0;
			_AECU.Register.TryGetValue(reg, out val);
			string res = string.Empty;
			string[] vs = new string[3] { " ", " ", " " };
			if (reg != AECUDevice.RegisterAddresses.ToRecorderSourceMixer && val != 0)
				vs[val - 1] = "v";
			switch (reg)
			{
				case AECUDevice.RegisterAddresses.Ch1_OutputSourceSelect:
					res = string.Format(" Output Ch1 Source Select   : [{0}] Input Ch1, [{1}] Input Ch2, [{2}] PICU Net In |", vs);
					break;
				case AECUDevice.RegisterAddresses.Ch2_OutputSourceSelect:
					res = string.Format(" Output Ch2 Source Select   : [{0}] Input Ch1, [{1}] Input Ch2, [{2}] PICU Net In |", vs);
					break;
				case AECUDevice.RegisterAddresses.AMP_OutputSourceSelect:
					res = string.Format(" AMP Output Source Select   : [{0}] Input Ch1, [{1}] Input Ch2, [{2}] PICU Net In |", vs);
					break;
				case AECUDevice.RegisterAddresses.ToPICUSourceSelect:
					res = string.Format(" Net To PICU Source Select  : [{0}] Input Ch1, [{1}] Input Ch2                  |", vs);
					break;
				case AECUDevice.RegisterAddresses.ToPACUSourceSelect:
					res = string.Format(" Net To PACU Source Select  : [{0}] Input Ch1, [{1}] Input Ch2                  |", vs);
					break;
				case AECUDevice.RegisterAddresses.ToRecorderSourceMixer:
					if (((AECUDevice.SourceMixer)val).HasFlag(AECUDevice.SourceMixer.InputCh1))
						vs[0] = "v";
					if (((AECUDevice.SourceMixer)val).HasFlag(AECUDevice.SourceMixer.InputCh2))
						vs[1] = "v";
					if (((AECUDevice.SourceMixer)val).HasFlag(AECUDevice.SourceMixer.PICU_NetIn))
						vs[2] = "v";
					res = string.Format(" Net To Record Source Mixer : [{0}] Input Ch1, [{1}] Input Ch2, [{2}] PICU Net In |", vs);
					break;
			}
			if (!string.IsNullOrEmpty(res))
				res = (focus ? "{!d_blue}{white} >" : "{gray}  ") + res + "{reset}";
			return res;
		}
		#endregion

		#region Private Method : string GetPACU_LevelBar(PACUDevice.RegisterAddresses reg, int min, int max, bool focus)
		private string GetPACU_LevelBar(PACUDevice.RegisterAddresses reg, int min, int max, bool focus)
		{
			short val = 0;
			_PACU.Register.TryGetValue(reg + 1, out val);
			bool mute = (val == 1);
			_PACU.Register.TryGetValue(reg, out val);
			string res = "#".PadRight(DB_LEVEL_BAR_SIZE, '#') + "{reset}";
			int step = (max - min) / DB_LEVEL_BAR_SIZE;
			int addr = (val - min) / step;
			res = res.Insert(addr, "{d_gray}").Insert(0, "{green}");
			res = string.Format("{3}{4} [{0,-16}]  {2}{3}  [{{green}}{1,3}{{reset}}{3}] [{{reset}}{5}Mute{{reset}}{3}] {{reset}}",
				reg.ToString().Replace('_', ' '), val, res, focus ? "{!d_blue}{white}" : "{gray}", focus ? " >" : "  ", mute ? (focus ? "{!d_blue}{red}" : "{red}") : (focus ? "{!d_blue}{black}" : "{d_gray}"));
			return res;
		}
		#endregion

		#region Private Method : VoiceEncoder.RegisterAddresses GetAECU_LevelRegister(int row)
		private AECUDevice.RegisterAddresses GetAECU_LevelRegister(int row)
		{
			switch (row)
			{
				case 0: return AECUDevice.RegisterAddresses.AMP_OutLevel;
				case 1: return AECUDevice.RegisterAddresses.Ch1_InputLevel;
				case 2: return AECUDevice.RegisterAddresses.Ch1_OutputLevel;
				case 3: return AECUDevice.RegisterAddresses.Ch2_InputLevel;
				case 4: return AECUDevice.RegisterAddresses.Ch2_OutputLevel;
				default: return AECUDevice.RegisterAddresses.AMP_OutLevel;
			}
		}
		#endregion

		#region Private Method : VoiceEncoder.RegisterAddresses GetAECU_OutSourceRegister(int row)
		private AECUDevice.RegisterAddresses GetAECU_OutSourceRegister(int row)
		{
			switch (row)
			{
				case 0: return AECUDevice.RegisterAddresses.Ch1_OutputSourceSelect;
				case 1: return AECUDevice.RegisterAddresses.Ch2_OutputSourceSelect;
				case 2: return AECUDevice.RegisterAddresses.AMP_OutputSourceSelect;
				case 3: return AECUDevice.RegisterAddresses.ToPICUSourceSelect;
				case 4: return AECUDevice.RegisterAddresses.ToPACUSourceSelect;
				case 5: return AECUDevice.RegisterAddresses.ToRecorderSourceMixer;
				default: return AECUDevice.RegisterAddresses.Ch1_OutputSourceSelect;
			}
		}
		#endregion

		#region Private Method : PACUDevice.RegisterAddresses GetPACU_LevelRegister(int row)
		private PACUDevice.RegisterAddresses GetPACU_LevelRegister(int row)
		{
			switch (row)
			{
				case 0: return PACUDevice.RegisterAddresses.AMP_Ch1_Level;
				case 1: return PACUDevice.RegisterAddresses.AMP_Ch2_Level;
				case 2: return PACUDevice.RegisterAddresses.Output_Ch1_Level;
				case 3: return PACUDevice.RegisterAddresses.Output_Ch2_Level;
				case 4: return PACUDevice.RegisterAddresses.Output_Ch3_Level;
				case 5: return PACUDevice.RegisterAddresses.Output_Ch4_Level;
				case 6: return PACUDevice.RegisterAddresses.Input_Ch1_Level;
				case 7: return PACUDevice.RegisterAddresses.Input_Ch2_Level;
				case 8: return PACUDevice.RegisterAddresses.Input_Ch3_Level;
				case 9: return PACUDevice.RegisterAddresses.Input_Ch4_Level;
				default: return PACUDevice.RegisterAddresses.AMP_Ch1_Level;
			}
		}
		#endregion

		#region Menu Cursor Control
		#region Private Method : void Menu_CursorMoveUp(AsyncClient ac, object[] data)
		private void Menu_CursorMoveUp(AsyncClient ac, object[] data)
		{
			 //* data Array:
			 //* 0 : Device Name
			 //* 1 : Device Mode(for PICU)
			 //* 2 : Cursor Min Y
			 //* 3 : Rows
			 //* 4 : Cursor Min X
			 //* 5 : Steps
			string dev = data[0].ToString();
			string mode = string.Empty;
			if (data[1] != null)
				mode = data[1].ToString();
			int minY = Convert.ToInt32(data[2]);
			int minX = 0;
			if (data[4] != null)
				minX = Convert.ToInt32(data[4]);
			CursorLocation cl, orig;
			_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl);
			orig = cl.Clone();
			cl.Row--;
			if (cl.Row < minY)
				cl.Row = minY;
			StringBuilder sb = new StringBuilder();
			sb.Append(AnsiString.SaveCursor);
			if (!cl.Equals(orig))
			{
				cl.Col = minX;
				int step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / DB_LEVEL_BAR_SIZE;
				switch (dev.ToUpper())
				{
					case "PICU":
						{
							switch (mode)
							{
								case "MIC":
								case "SPK":
									cl.Col += (_PICUs[cl.Row - minY].Register[PICUDevice.RegisterAddresses.MIC_InputLevel] - DB_LEVEL_MIN) / step;
									sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetPICU_LevelBar(_PICUs[orig.Row - minY], DB_LEVEL_MIN, DB_LEVEL_MAX, false, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
									sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPICU_LevelBar(_PICUs[cl.Row - minY], DB_LEVEL_MIN, DB_LEVEL_MAX, true, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
									break;
								case "OutSrc":
									break;
							}
							break;
						}
					case "AECU":
						{
							AECUDevice.RegisterAddresses ra;
							switch (mode)
							{
								case "OutSrc":
									ra = GetAECU_OutSourceRegister(cl.Row - minY);
									sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetAECU_OutSelectBar(GetAECU_OutSourceRegister(orig.Row - minY), false));
									sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_OutSelectBar(ra, true));
									break;
								default:
									ra = GetAECU_LevelRegister(cl.Row - minY);
									cl.Col += (_AECU.Register[ra] - DB_LEVEL_MIN) / step;
									sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetAECU_LevelBar(GetAECU_LevelRegister(orig.Row - minY), DB_LEVEL_MIN, DB_LEVEL_MAX, false));
									sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
									break;
							}
							break;
						}
					case "PACU":
						{
							PACUDevice.RegisterAddresses ra = GetPACU_LevelRegister(cl.Row - minY);
							cl.Col += (_PACU.Register[ra] - DB_LEVEL_MIN) / step;
							sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetPACU_LevelBar(GetPACU_LevelRegister(orig.Row - minY), DB_LEVEL_MIN, DB_LEVEL_MAX, false));
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPACU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
							break;
						}
				}
			}
			sb.Append(AnsiString.RestoreCursor);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, cl, (k, v) => v = cl);
		}
		#endregion

		#region Private Method : void Menu_CursorMoveDn(AsyncClient ac, object[] data)
		private void Menu_CursorMoveDn(AsyncClient ac, object[] data)
		{
			string dev = data[0].ToString();
			string mode = string.Empty;
			if (data[1] != null)
				mode = data[1].ToString();
			int minY = Convert.ToInt32(data[2]);
			int maxY = minY + Convert.ToInt32(data[3]) - 1;
			int minX = 0;
			if (data[4] != null)
				minX = Convert.ToInt32(data[4]);
			CursorLocation cl, orig;
			_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl);
			orig = cl.Clone();
			cl.Row++;
			if (cl.Row >= maxY)
				cl.Row = maxY;
			StringBuilder sb = new StringBuilder();
			sb.Append(AnsiString.SaveCursor);
			if (!cl.Equals(orig))
			{
				cl.Col = minX;
				int step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / DB_LEVEL_BAR_SIZE;
				switch (dev.ToUpper())
				{
					case "PICU":
						{
							switch (mode)
							{
								case "MIC":
								case "SPK":
									cl.Col += (_PICUs[cl.Row - minY].Register[PICUDevice.RegisterAddresses.MIC_InputLevel] - DB_LEVEL_MIN) / step;
									sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetPICU_LevelBar(_PICUs[orig.Row - minY], DB_LEVEL_MIN, DB_LEVEL_MAX, false, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
									sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPICU_LevelBar(_PICUs[cl.Row - minY], DB_LEVEL_MIN, DB_LEVEL_MAX, true, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
									break;
								case "OutSrc":
									break;
							}
							break;
						}
					case "AECU":
						{
							AECUDevice.RegisterAddresses ra;
							switch (mode)
							{
								case "OutSrc":
									ra = GetAECU_OutSourceRegister(cl.Row - minY);
									sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetAECU_OutSelectBar(GetAECU_OutSourceRegister(orig.Row - minY), false));
									sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_OutSelectBar(ra, true));
									break;
								default:
									ra = GetAECU_LevelRegister(cl.Row - minY);
									cl.Col += (_AECU.Register[ra] - DB_LEVEL_MIN) / step;
									sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetAECU_LevelBar(GetAECU_LevelRegister(orig.Row - minY), DB_LEVEL_MIN, DB_LEVEL_MAX, false));
									sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
									break;
							}
							break;
						}
					case "PACU":
						{
							PACUDevice.RegisterAddresses ra = GetPACU_LevelRegister(cl.Row - minY);
							cl.Col += (_PACU.Register[ra] - DB_LEVEL_MIN) / step;
							sb.AppendFormat("\x1B[{0};1H{1}", orig.Row, GetPACU_LevelBar(GetPACU_LevelRegister(orig.Row - minY), DB_LEVEL_MIN, DB_LEVEL_MAX, false));
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPACU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
							break;
						}
				}
			}
			sb.Append(AnsiString.RestoreCursor);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, cl, (k, v) => v = cl);
		}
		#endregion

		#region Private Method : void Menu_CursorMoveLf(AsyncClient ac, object[] data)
		private void Menu_CursorMoveLf(AsyncClient ac, object[] data)
		{
			string dev = data[0].ToString();
			string mode = string.Empty;
			if (data[1] != null)
				mode = data[1].ToString();
			int minY = Convert.ToInt32(data[2]);
			int minX = Convert.ToInt32(data[4]);
			int steps = Convert.ToInt32(data[5]);
			int step = 0;
			CursorLocation cl, orig;
			_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl);
			orig = cl.Clone();
			cl.Col--;
			if (cl.Col < minX)
				cl.Col = minX;
			StringBuilder sb = new StringBuilder();
			sb.Append(AnsiString.SaveCursor);
			if (!cl.Equals(orig))
			{
				step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / steps;
				short val = (short)((cl.Col - minX) * step + DB_LEVEL_MIN);
				switch (dev.ToUpper())
				{
					case "PICU":
						{
							if (mode.Equals("MIC", StringComparison.OrdinalIgnoreCase))
								_PICUs[cl.Row - minY].SetLevel(PICUDevice.RegisterAddresses.MIC_InputLevel, val);
							else
								_PICUs[cl.Row - minY].SetLevel(PICUDevice.RegisterAddresses.SpeakerOutputLevel, val);
							Thread.Sleep(50);
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPICU_LevelBar(_PICUs[cl.Row - minY], DB_LEVEL_MIN, DB_LEVEL_MAX, true, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
							break;
						}
					case "AECU":
						{
							AECUDevice.RegisterAddresses ra = GetAECU_LevelRegister(cl.Row - minY);
							_AECU.SetRegister(ra, val);
							Thread.Sleep(50);
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
							break;
						}
					case "PACU":
						{
							PACUDevice.RegisterAddresses ra = GetPACU_LevelRegister(cl.Row - minY);
							_PACU.SetLevel(ra, val);
							Thread.Sleep(50);
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPACU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
							break;
						}
				}
			}
			sb.Append(AnsiString.RestoreCursor);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, cl, (k, v) => v = cl);
		}
		#endregion

		#region Private Method : void Menu_CursorMoveRt(AsyncClient ac, object[] data)
		private void Menu_CursorMoveRt(AsyncClient ac, object[] data)
		{
			string dev = data[0].ToString();
			string mode = string.Empty;
			if (data[1] != null)
				mode = data[1].ToString();
			int row = Convert.ToInt32(data[2]);
			int min = Convert.ToInt32(data[4]);
			int steps = Convert.ToInt32(data[5]);
			int max = min + steps;
			int step = 0;
			CursorLocation cl, orig;
			_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl);
			orig = cl.Clone();
			cl.Col++;
			if (cl.Col > max)
				cl.Col = max;
			StringBuilder sb = new StringBuilder();
			sb.Append(AnsiString.SaveCursor);
			if (!cl.Equals(orig))
			{
				step = (DB_LEVEL_MAX - DB_LEVEL_MIN) / steps;
				short val = (short)((cl.Col - min) * step + DB_LEVEL_MIN);
				switch (dev.ToUpper())
				{
					case "PICU":
						{
							if (mode.Equals("MIC", StringComparison.OrdinalIgnoreCase))
								_PICUs[cl.Row - row].SetLevel(PICUDevice.RegisterAddresses.MIC_InputLevel, val);
							else
								_PICUs[cl.Row - row].SetLevel(PICUDevice.RegisterAddresses.SpeakerOutputLevel, val);
							Thread.Sleep(50);
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPICU_LevelBar(_PICUs[cl.Row - row], DB_LEVEL_MIN, DB_LEVEL_MAX, true, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
							break;
						}
					case "AECU":
						{
							AECUDevice.RegisterAddresses ra = GetAECU_LevelRegister(cl.Row - row);
							_AECU.SetRegister(ra, val);
							Thread.Sleep(50);
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
							break;
						}
					case "PACU":
						{
							PACUDevice.RegisterAddresses ra = GetPACU_LevelRegister(cl.Row - row);
							_PACU.SetLevel(ra, val);
							Thread.Sleep(50);
							sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPACU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
							break;
						}
				}
			}
			sb.Append(AnsiString.RestoreCursor);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, cl, (k, v) => v = cl);
		}
		#endregion

		#region Private Method : void Menu_CursorCheck(AsyncClient ac, object[] data)
		private void Menu_CursorCheck(AsyncClient ac, object[] data)
		{
			string dev = data[0].ToString();
			string mode = string.Empty;
			if (data[1] != null)
				mode = data[1].ToString();
			int minY = Convert.ToInt32(data[2]);
			CursorLocation cl;
			_TelnetCursor.TryGetValue(ac.RemoteEndPoint, out cl);
			StringBuilder sb = new StringBuilder();
			sb.Append(AnsiString.SaveCursor);
			short tmp = 0;
			switch (dev.ToUpper())
			{
				case "PICU":
					{
						PICUDevice.RegisterAddresses ra = mode.Equals("MIC") ? PICUDevice.RegisterAddresses.MIC_InputMute : PICUDevice.RegisterAddresses.SpeakerOutputMute;
						_PICUs[cl.Row - minY].Register.TryGetValue(ra, out tmp);
						_PICUs[cl.Row - minY].SetMute(ra, tmp == 0);
						Thread.Sleep(50);
						sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPICU_LevelBar(_PICUs[cl.Row - minY], DB_LEVEL_MIN, DB_LEVEL_MAX, true, mode.Equals("MIC", StringComparison.OrdinalIgnoreCase)));
						break;
					}
				case "AECU":
					{
						AECUDevice.RegisterAddresses ra;
						switch (mode)
						{
							case "OutSrc":
								ra = GetAECU_OutSourceRegister(cl.Row - minY);
								_AECU.Register.TryGetValue(ra, out tmp);
								switch (ra)
								{
									case AECUDevice.RegisterAddresses.Ch1_OutputSourceSelect:
									case AECUDevice.RegisterAddresses.Ch2_OutputSourceSelect:
									case AECUDevice.RegisterAddresses.AMP_OutputSourceSelect:
										_AECU.SetRegister(ra, (short)((tmp + 1) % 4));
										break;
									case AECUDevice.RegisterAddresses.ToPICUSourceSelect:
									case AECUDevice.RegisterAddresses.ToPACUSourceSelect:
										_AECU.SetRegister(ra, (short)((tmp + 1) % 3));
										break;
									case AECUDevice.RegisterAddresses.ToRecorderSourceMixer:
										break;
								}
								sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_OutSelectBar(ra, true));
								break;
							default:
								ra = GetAECU_LevelRegister(cl.Row - minY);
								_AECU.Register.TryGetValue(ra + 1, out tmp);
								_AECU.SetRegister(ra + 1, (short)((tmp + 1) % 2));
								Thread.Sleep(50);
								sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetAECU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
								break;
						}
						break;
					}
				case "PACU":
					{
						PACUDevice.RegisterAddresses ra = GetPACU_LevelRegister(cl.Row - minY);
						_PACU.Register.TryGetValue(ra + 1, out tmp);
						_PACU.SetMute(ra + 1, tmp == 0);
						Thread.Sleep(50);
						sb.AppendFormat("\x1B[{0};1H{1}", cl.Row, GetPACU_LevelBar(ra, DB_LEVEL_MIN, DB_LEVEL_MAX, true));
						break;
					}
			}
			sb.Append(AnsiString.RestoreCursor);
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			_TelnetCursor.AddOrUpdate(ac.RemoteEndPoint, cl, (k, v) => v = cl);
		}
		#endregion
		#endregion

		#region Private Method : void Exec_SIC_ButtonLED(AsyncClient ac, object[] data)
		private void Exec_SIC_ButtonLED(AsyncClient ac, object[] data)
		{
			bool onOff = data[0].ToString().Equals("ON", StringComparison.OrdinalIgnoreCase);
			//Console.WriteLine("Telnet:Set Service Intercom Button LED {0}", onOff ? "ON" : "OFF");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:SIC All Button LED {1}", tcs.HasValue ? tcs.Value.UserID : "Unknow", onOff ? "ON" : "OFF");
			_SIC.SetButtonLED(onOff);
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Set Service Intercom Button LED to {{cyan}}{0}{{reset}} {{green}}Success{{reset}}.", onOff ? "ON" : "OFF");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_SIC_ShowStatus(AsyncClient ac, object[] data)
		private void Exec_SIC_ShowStatus(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Show SIC Status", cmd.ToString());
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Show SIC Status", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			_TelnetOnMonitor.AddOrUpdate(ac.RemoteEndPoint, TelnetMonitorType.SIC, (k, v) => v = TelnetMonitorType.SIC);
			ac.SendData(AnsiString.ToANSI("{{reset}}{{cls}}"));
			System.Threading.Tasks.Task.Factory.StartNew(() => SIC_ShowStatus(ac));
		}
		#endregion

		#region Private Method : void Exec_SIC_Restart(AsyncClient ac, object[] data)
		private void Exec_SIC_Restart(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Restart SIC");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Restart Service Intercom Controller", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			RestartSIC();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}Service Intercom Controller {{green}}Restarted{{reset}}.");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_PICU_Restart(AsyncClient ac, object[] data)
		private void Exec_PICU_Restart(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Restart SIC");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Restart ALL PICU Controller", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			RestartPICU();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}All PICU Controller {{green}}Restarted{{reset}}.");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_AECU_Restart(AsyncClient ac, object[] data)
		private void Exec_AECU_Restart(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Restart AECU Controller");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Restart AECU Controller", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			RestartAECU();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}AECU Controller {{green}}Restarted{{reset}}.");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion

		#region Private Method : void Exec_PACU_Restart(AsyncClient ac, object[] data)
		private void Exec_PACU_Restart(AsyncClient ac, object[] data)
		{
			//Console.WriteLine("Telnet:Restart PACU Controller");
			TelnetClientSetting? tcs = _Telnet.GetTelnetClientSetting(ac.RemoteEndPoint);
			_log.Write(LogManager.LogLevel.Info, "[TELNET][{0}]MG:Restart PACU Controller", tcs.HasValue ? tcs.Value.UserID : "Unknow");
			RestartPACU();
			StringBuilder sb = new StringBuilder();
			sb.AppendLine();
			sb.AppendFormat("{{reset}}PACU Controller {{green}}Restarted{{reset}}.");
			sb.AppendLine();
			sb.AppendLine();
			ac.SendData(AnsiString.ToANSI(sb.ToString()));
			Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
		}
		#endregion
	}
	*/
	#endregion
}
