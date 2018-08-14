using CJF.Net;
using CJF.Net.Telnet;
using CJF.Utility;
using CJF.Utility.Ansi;
using CJF.Utility.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

#pragma warning disable IDE1006
namespace Tester
{
    public partial class FTelnetServer : Form
    {
        enum LoginStep
        {
            Commands = 0,
            Account = 1,
            Passowrd = 2,
            Success = 3
        }

        TelnetServer _Server = null;
        bool _isExit = false;

        public FTelnetServer()
        {
            InitializeComponent();
            WriteLog("===== New Session =====");
        }

        #region WriteLog
        delegate void WriteLogCallback(string txt);
        void WriteLog(string txt)
        {
            try
            {
                if (rtbConsole.InvokeRequired)
                    this.Invoke(new WriteLogCallback(WriteLog), new object[] { txt });
                else
                {
                    if (rtbConsole.Lines.Length > 500)
                    {
                        List<string> ls = new List<string>(rtbConsole.Lines);
                        ls.RemoveRange(0, ls.Count - 400);
                        rtbConsole.Lines = ls.ToArray();
                    }
                    LogManager.WriteLog(txt);
                    rtbConsole.AppendText(string.Format("{0} - {1}\n", DateTime.Now.ToString("HH:mm:ss.fff"), txt));
                    rtbConsole.SelectionStart = rtbConsole.TextLength;
                    rtbConsole.ScrollToCaret();
                }
            }
            catch { }
        }
        void WriteLog(string format, params object[] args)
        {
            WriteLog(string.Format(format, args));
        }
        #endregion

        #region Button Events
        private void btnListen_Click(object sender, EventArgs e)
        {
            btnListen.Enabled = false;
            InitTelnetListener();
            btnStop.Enabled = true;
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            SetButtonEnabled(btnStop, false);
            _Server.Shutdown();
            _Server.Dispose();
            _Server = null;
            SetButtonEnabled(btnListen, true);
        }
        #endregion

        #region SetButtonEnabled
        delegate void SetButtonEnabledCallback(Button btn, bool enabled);
        void SetButtonEnabled(Button btn, bool enabled)
        {
            if (btn.InvokeRequired)
                this.Invoke(new SetButtonEnabledCallback(SetButtonEnabled), new object[] { btn, enabled });
            else
                btn.Enabled = enabled;
        }
        #endregion

        #region SetConteolText
        delegate void SetControlTextCallback(Control c, string text);
        void SetConteolText(Control c, string text)
        {
            if (c.InvokeRequired)
                c.Invoke(new SetControlTextCallback(SetConteolText), new object[] { c, text });
            else
                c.Text = text;
        }
        #endregion

        #region ListBox Delegate
        delegate void AppendListBoxItemCallback(ListBox lb, object o);
        void AppendListBoxItem(ListBox lb, object o)
        {
            try
            {
                if (lb.InvokeRequired)
                    lb.Invoke(new AppendListBoxItemCallback(AppendListBoxItem), new object[] { lb, o });
                else
                    lb.Items.Add(o);
            }
            catch (ObjectDisposedException) { }
        }
        delegate void RemoveListBoxItemCallback(ListBox lb, object o);
        void RemoveListBoxItem(ListBox lb, object o)
        {
            try
            {
                if (lb.InvokeRequired)
                    lb.Invoke(new RemoveListBoxItemCallback(RemoveListBoxItem), new object[] { lb, o });
                else
                    lb.Items.Remove(o);
            }
            catch (ObjectDisposedException) { }
        }
        delegate void ClearListBoxItemCallback(ListBox lb);
        void ClearListBoxItem(ListBox lb)
        {
            try
            {
                if (lb.InvokeRequired)
                    lb.Invoke(new ClearListBoxItemCallback(ClearListBoxItem), new object[] { lb });
                else
                    lb.Items.Clear();
            }
            catch (ObjectDisposedException) { }
        }
        #endregion

        private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode != Keys.Enter)
            //    return;
            //if (lbRemotes.SelectedItems == null || lbRemotes.SelectedItems.Count == 0)
            //{
            //    AsyncClient[] acs = _Server.SocketServer.Clients;
            //    for (int i = 0; i < acs.Length; i++)
            //    {
            //        if (chkHexString.Checked)
            //            acs[i].SendData(txtSendMsg.Text.ToByteArray());
            //        else
            //            acs[i].SendData(txtSendMsg.Text);
            //    }
            //}
            //else
            //{
            //    AsyncClient ac = null;
            //    foreach (object o in lbRemotes.SelectedItems)
            //    {
            //        ac = _Server.SocketServer.FindClient((System.Net.EndPoint)o);
            //        if (ac != null)
            //        {
            //            if (chkHexString.Checked)
            //                _Client.SendData(txtSendMsg.Text.ToByteArray());
            //            else
            //                _Client.SendData(txtSendMsg.Text);
            //        }
            //    }
            //}
            //txtSendMsg.SelectAll();
        }

        private void FTelnetServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_Server != null && _Server.IsStarted)
            {
                _Server.Shutdown();
                _Server.Dispose();
                _Server = null;
            }
            WriteLog("===== Session Stop =====");
        }

        private void chkPopupCommands_CheckedChanged(object sender, EventArgs e)
        {
            if (_Server != null)
                _Server.PopupCommands = chkPopupCommands.Checked;
        }

        #region Telnet Listener Segment
        ConcurrentDictionary<EndPoint, TelnetMenus> _ClientMenu = null;

        #region Private Method : InitTelnetListener()
        private bool InitTelnetListener()
        {
            _ClientMenu = new ConcurrentDictionary<EndPoint, TelnetMenus>();
            try
            {
                int bufferSize = 1024;
                int clients = 10;
                string ap = string.Empty;
                _Server = new TelnetServer(clients, Encoding.UTF8)
                {
                    Authentication = txtAuth.Text,
                    AutoCloseTime = 0,
                    DebugMode = SocketDebugType.Receive | SocketDebugType.Send,
                    DefaultEndChar = TelnetServer.CommandEndCharType.None
                };
                _Server.Started += new EventHandler(Server_OnStarted);
                _Server.Shutdowned += new EventHandler(Server_OnShutdown);
                _Server.ClientConnected += new EventHandler<SocketServerEventArgs>(Server_OnClientConnected);
                _Server.ClientClosed += new EventHandler<SocketServerEventArgs>(Server_OnClientClosed);
                _Server.ClientClosing += new EventHandler<SocketServerEventArgs>(Server_OnClientClosing);
                _Server.DataReceived += new EventHandler<SocketServerEventArgs>(Server_OnDataReceived);
                _Server.DataSended += new EventHandler<SocketServerEventArgs>(Server_OnDataSended);
                _Server.SendedFail += new EventHandler<SocketServerEventArgs>(Server_OnSendedFail);
                _Server.Exception += new EventHandler<SocketServerEventArgs>(Server_OnException);
                TelnetMenus.Server = _Server;
                IPEndPoint ipp = new IPEndPoint(IPAddress.Any, Convert.ToInt32(txtPort.Text));
                DateTime now = DateTime.Now;
                _Server.Start(ipp);
                while (!_Server.IsStarted && DateTime.Now.Subtract(now).TotalMilliseconds < 5000)
                    Thread.Sleep(100);
                string msg = $" # 於 {ipp} 初始化伺服器, 最大連線數:{clients}, 緩衝區:{bufferSize}Bytes ...";

                if (_Server.IsStarted)
                {
                    WriteLog($"{msg} Success");
                }
                else
                {
                    WriteLog($"{msg} Failure");
                }
                return _Server.IsStarted;
            }
            catch (Exception ex)
            {
                WriteLog("Initial Telnet Server Failure");
                WriteLog(ex.Message);
                return false;
            }
        }
        #endregion

        #region Private Method : void Server_OnStarted(object sender, EventArgs e)
        private void Server_OnStarted(object sender, EventArgs e)
        {
            WriteLog("M:伺服器已於 {0} 啟動", _Server.LocalEndPort);
            WriteLog("M:接線池剩餘：{0}", _Server.MaxConnections - _Server.Connections);
        }
        #endregion

        #region Private Method : void Server_OnShutdown(object sender, EventArgs e)
        private void Server_OnShutdown(object sender, EventArgs e)
        {
            WriteLog("M:伺服器已關閉");
            ClearListBoxItem(lbRemotes);
        }
        #endregion

        #region Private Method : void Server_OnClientConnected(object sender, SocketServerEventArgs e)
        private void Server_OnClientConnected(object sender, SocketServerEventArgs e)
        {
            if (e == null || e.Client == null) return;
            TelnetServer svr = (TelnetServer)sender;
            int cPort = 0;
            try
            {
                if (_isExit || IsDisposed)
                {
                    e.Client.Close();
                    return;
                }
                cPort = ((IPEndPoint)e.RemoteEndPoint).Port;
                WriteLog("用戶端 {0} 已連線", e.RemoteEndPoint);
                WriteLog("M:接線池剩餘：{0}", svr.MaxConnections - svr.Connections);
                SetConteolText(gbClients, string.Format("Clients : {0}", svr.Connections));
                AppendListBoxItem(lbRemotes, e.RemoteEndPoint);
                svr.SetCommandEndChar(e.RemoteEndPoint, TelnetServer.CommandEndCharType.None);
                TelnetMenus tm = new TelnetMenus(e.Client);
                _ClientMenu.AddOrUpdate(e.RemoteEndPoint, tm, (k, v) => v = tm);
                RemoveInvalidClient();
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(200);
                    tm.GoMainMenu();
                });
            }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                if (!_isExit)
                    WriteLog(ex.Message);
            }
        }
        #endregion

        #region Private Method : void Server_OnClientClosing(object sender, SocketServerEventArgs e)
        private void Server_OnClientClosing(object sender, SocketServerEventArgs e)
        {
            if (IsDisposed || e == null || e.RemoteEndPoint == null) return;
            try
            {
                if (e.RemoteEndPoint != null)
                {
                    WriteLog("M:用戶端 {0} 正停止連線", e.RemoteEndPoint);
                    _ClientMenu?.TryRemove(e.RemoteEndPoint, out TelnetMenus tmp);
                }
            }
            catch (Exception ex)
            {
                if (!_isExit)
                    WriteLog(ex.Message);
            }
        }
        #endregion

        #region Private Method : void Server_OnClientClosed(object sender, SocketServerEventArgs e)
        private void Server_OnClientClosed(object sender, SocketServerEventArgs e)
        {
            if (IsDisposed || e == null || e.RemoteEndPoint == null) return;
            TelnetServer svr = (TelnetServer)sender;
            if (svr == null || !svr.IsStarted)
                return;
            try
            {
                if (e.RemoteEndPoint != null)
                {
                    WriteLog($"M:用戶端 {e.RemoteEndPoint} 已關閉連線");
                    WriteLog($"M:接線池剩餘：{(svr.MaxConnections - svr.Connections)}");
                    RemoveListBoxItem(lbRemotes, e.RemoteEndPoint);
                    SetConteolText(gbClients, $"Clients : {svr.Connections}");
                    _ClientMenu?.TryRemove(e.RemoteEndPoint, out TelnetMenus tmp);
                }
            }
            catch (Exception ex)
            {
                if (!_isExit)
                    WriteLog(ex.Message);
            }
        }
        #endregion

        #region Private Method : void Server_OnDataReceived(object sender, SocketServerEventArgs e)
        private void Server_OnDataReceived(object sender, SocketServerEventArgs e)
        {
            if (_isExit || IsDisposed || e == null || e.Client == null) return;

            TelnetServer svr = (TelnetServer)sender;
            DateTime recTime = DateTime.Now;
            int cPort = ((IPEndPoint)e.RemoteEndPoint).Port;
            WriteLog($"自 {e.RemoteEndPoint} 收到 {e.Data.ToHexString()}");
            if (!_ClientMenu.ContainsKey(e.RemoteEndPoint))
                return;
            string currentPath = _ClientMenu[e.RemoteEndPoint].CurrentPath;
            string[] arr = currentPath.Split('\\');
            string previousPath = string.Join("\\", arr, 0, arr.Length - 1);
            string lastCommand = arr[arr.Length - 1];
            TelnetMenus tm = _ClientMenu[e.RemoteEndPoint];
            string cmdLine = svr.Encoding.GetString(e.Data).Replace("\r", "").Replace("\n", "");
            if (string.IsNullOrEmpty(cmdLine) && svr.GetCommandEndChar(e.RemoteEndPoint) == TelnetServer.CommandEndCharType.None)
                return;
            tm.Execute(cmdLine);
        }
        #endregion

        #region Private Method : void Server_OnDataSended(object sender, SocketServerEventArgs e)
        private void Server_OnDataSended(object sender, SocketServerEventArgs e)
        {
            if (e == null || e.Data == null || e.Data.Length == 0) return;
            try
            {
                AsyncClient ac = e.Client;
                if (ac == null || e.ExtraInfo == null) return;
                WriteLog($"成功送出 {e.Data.Length} Bytes 資料給 {e.RemoteEndPoint}");
            }
            catch (Exception ex)
            {
                if (!_isExit)
                    WriteLog(ex.Message);
            }
        }
        #endregion

        #region Private Method : void Server_OnSendedFail(object sender, SocketServerEventArgs e)
        private void Server_OnSendedFail(object sender, SocketServerEventArgs e)
        {
            if (e == null || e.Data == null || e.Data.Length == 0) return;
            try
            {
                AsyncClient ac = e.Client;
                if (ac == null || ac.ExtraInfo == null) return;
                WriteLog($"送出資料給 {e.RemoteEndPoint} 失敗 {e.Data.Length} Bytes");
            }
            catch (Exception ex)
            {
                if (!_isExit)
                    WriteLog(ex.Message);
            }
        }
        #endregion

        #region Private Method : void Server_OnException(object sender, SocketServerEventArgs e)
        private void Server_OnException(object sender, SocketServerEventArgs e)
        {
            int cPort = 0;
            try
            {
                cPort = ((IPEndPoint)e.RemoteEndPoint).Port;
                AsyncClient ac = e.Client;
                if (e.Exception.GetType().Equals(typeof(System.Net.Sockets.SocketException)))
                {
                    System.Net.Sockets.SocketException ex = (System.Net.Sockets.SocketException)e.Exception;
                    if (ex.ErrorCode == 10024 && e.Client != null)
                    {
                        e.Client.SendData(CsiBuilder.CreateString(SgrColors.Red, "Session Full!!!", true));
                        Thread.Sleep(100);
                        e.Client.Close();
                    }
                }
                else
                {
                    WriteLog($"MG:Telnet Exception:{e.Exception.Message}");
                }
            }
            catch (Exception ex)
            {
                if (!_isExit)
                    WriteLog(ex.Message);
            }
        }
        #endregion
        #endregion

        #region Private Method : void RemoveInvalidClient()
        private void RemoveInvalidClient()
        {
            EndPoint[] eps = new EndPoint[_ClientMenu.Keys.Count];
            _ClientMenu.Keys.CopyTo(eps, 0);
            foreach (EndPoint ep in eps)
            {
                if (_Server.SocketServer.FindClient(ep) == null)
                    _ClientMenu.TryRemove(ep, out TelnetMenus tmp);
            }
        }
        #endregion
    }

    #region Telnet Functions
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

    #region Private Enum : TelnetMonitorType
    enum TelnetMonitorType : byte
    {
        None = 0,
        Action1 = 1,
        Action2 = 2,
        AECU = 3,
        PACU = 4,
        SIC = 5
    }
    #endregion

    public class TelnetMenus
    {
        #region Enum : MenuKeys
        enum MenuKeys
        {
            None = 0,
            MainMenu = 1,
            BackToParent = 2,
            Quit = 3
        }
        #endregion

        #region Private Class : MenuCommandAttribute
        class MenuCommandAttribute : Attribute
        {
            public MenuCommandAttribute(params Keys[] keys)
            {
                Keys = keys;
                Commands = Array.ConvertAll<Keys, string>(keys, (k) => k.ToString().ToUpper());
            }
            public Keys[] Keys { get; private set; }
            public string[] Commands { get; private set; }
        }
        #endregion

        #region Private Class : InputCommandAttribute
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class InputCommandAttribute : Attribute
        {
            Dictionary<string, string[]> _Prompts = null;
            /// <summary>將選單函示定義成可輸入的自訂屬性類別</summary>
            /// <param name="funcName">函示名稱</param>
            /// <param name="prompt">輸入提示字串</param>
            /// <param name="keys">可輸入的文字字串，如無限制，則輸入null或空陣列</param>
            public InputCommandAttribute(string funcName, string prompt, string[] keys)
            {
                Method = typeof(TelnetMenus).GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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
                Method = typeof(TelnetMenus).GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
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
            public MethodInfo Method { get; private set; }
            public Dictionary<string, string[]> Prompts { get { return _Prompts; } }
        }
        #endregion

        #region Private Class : CursorControlAttribute
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class CursorControlAttribute : Attribute
        {
            /// <summary>建立 CursorControlAttribute 類別，供 Telnet 選單用，此建立方式適用於核選類(Check)的功能</summary>
            /// <param name="key">控制按鍵</param>
            /// <param name="funcName">欲執行的函示名稱</param>
            /// <param name="dev">設備代碼</param>
            /// <param name="mode">設備模組(如果有)</param>
            /// <param name="minY">第一選擇項的所在 Y 位置</param>
            public CursorControlAttribute(Keys key, string funcName, string dev, string mode, int minY) : this(key, funcName, new object[] { dev, mode, minY, null, null, null }) { }
            /// <summary>建立 CursorControlAttribute 類別，供 Telnet 選單用</summary>
            /// <param name="key">控制按鍵</param>
            /// <param name="funcName">欲執行的函示名稱</param>
            /// <param name="variables">傳入的參數資料, 0:設備代碼, 1:設備模組, 2:第一選擇項的所在 Y 位置, 3:選擇項數量/核選類項目文字 X 位置, 4:選擇項可調整的最小 X 位置, 5:選擇項可調整的格數</param>
            public CursorControlAttribute(Keys key, string funcName, params object[] variables)
            {
                Key = key;
                switch (key)
                {
                    case Keys.Up: this.KeyCode = "\x1B[A"; break;
                    case Keys.Down: this.KeyCode = "\x1B[B"; break;
                    case Keys.Left: this.KeyCode = "\x1B[D"; break;
                    case Keys.Right: this.KeyCode = "\x1B[C"; break;
                    case Keys.Space: this.KeyCode = "\x20"; break;
                    case Keys.Enter | Keys.Return: this.KeyCode = "\x0A"; break;
                    case Keys.Escape: this.KeyCode = "\x1B"; break;
                    default: break;
                }
                this.Method = typeof(TelnetMenus).GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (variables != null)
                {
                    this.Variables = new object[variables.Length];
                    Array.Copy(variables, this.Variables, this.Variables.Length);
                }
            }
            public Keys Key { get; private set; }
            /// <summary>取得控制鍵</summary>
            public string KeyCode { get; private set; }
            /// <summary>取得函示呼叫點</summary>
            public MethodInfo Method { get; private set; }
            /// <summary>取得傳入的參數資料</summary>
            public object[] Variables { get; private set; }
        }
        #endregion

        #region Private Class : ExecuteCommandAttribute
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        class ExecuteCommandAttribute : Attribute
        {
            /// <summary>建立 ExecuteCommandAttribute 類別，供 Telnet 選單用</summary>
            /// <param name="key">按鍵碼。</param>
            /// <param name="funcName">欲執行的函示</param>
            /// <param name="goNext">是否接續執行</param>
            /// <param name="variables">傳入的參數資料</param>
            public ExecuteCommandAttribute(Keys key, string funcName, bool goNext, params object[] variables)
            {
                this.Key = key;
                this.Method = typeof(TelnetMenus).GetMethod(funcName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (Method == null)
                    throw new ArgumentException();
                if (variables != null)
                {
                    this.Variables = new object[variables.Length];
                    Array.Copy(variables, this.Variables, this.Variables.Length);
                }
                this.ContinueNextStep = goNext;
            }

            /// <summary>建立 ExecuteCommandAttribute 類別，供 Telnet 選單用</summary>
            /// <param name="key">按鍵指令</param>
            /// <param name="funcClass">欲執行函式的類別型別。</param>
            /// <param name="funcName">欲執行的函示名稱</param>
            /// <param name="variables">傳入的參數資料</param>
            public ExecuteCommandAttribute(Keys key, string funcName, params object[] variables) : this(key, funcName, true, variables) { }
            /// <summary>取得按鍵指令</summary>
            public Keys Key { get; private set; }
            /// <summary>取得函示呼叫點</summary>
            public MethodInfo Method { get; private set; }
            /// <summary>取得傳入的參數資料</summary>
            public object[] Variables { get; private set; }
            /// <summary>取得是否接續執行</summary>
            public bool ContinueNextStep { get; private set; }
        }
        #endregion

        /// <summary>Telnet 指令執行後的暫停時間，單位豪秒</summary>
        const int TELNET_CMD_EXEC_SLEEP = 2000;
        /// <summary>調整列最小值</summary>
        const short TRACK_BAR_MIN = 0;
        /// <summary>調整列最大值</summary>
        const short TRACK_BAR_MAX = 100;
        /// <summary>顯示在 Telnet 用戶端的調整條的寬度</summary>
        const int TRACK_BAR_SIZE = 40;

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
            public bool IsEmpty { get => this.Row == 0 && this.Col == 0; }
            public static CursorLocation Empty { get => new CursorLocation(0, 0); }
        }
        #endregion

        /// <summary>處理 Telnet 每條連線之選單當前執行路徑</summary>
        public string CurrentPath { get; set; } = string.Empty;
        /// <summary>處理 Telnet 每條連線之選單輸入的內容</summary>
        /// <summary>紀錄 Telnet 使用者是否正在監控設備狀態資料</summary>
        /// <summary>紀錄 Telnet 使用者選單游標位置</summary>

        private string _TelnetInput = null;
        private CursorLocation _TelnetCursor;
        private readonly AsyncClient _Client = null;

        public static TelnetServer Server = null;
        public static ConcurrentDictionary<string, int> TrackValues = null;

        public TelnetMenus(AsyncClient ac)
        {
            _Client = ac;
            if (TrackValues == null)
            {
                TrackValues = new ConcurrentDictionary<string, int>();
                TrackValues.AddOrUpdate("Track 1", 100, (k, v) => v = 10);
                TrackValues.AddOrUpdate("Track 2", 25, (k, v) => v = 25);
                TrackValues.AddOrUpdate("Track 3", 50, (k, v) => v = 50);
                TrackValues.AddOrUpdate("Track 4", 100, (k, v) => v = 100);
            }
        }

        public void GoMainMenu()
        {
            Menu_M(null, null);
        }

        public void Execute(string cmdLine)
        {
            string[] args = cmdLine.Split(' ');
            //string[] allowCommands = null;
            Keys[] allowKeys = null;
            string[] paths = CurrentPath.Split('\\');
            string previousPath = string.Empty;
            string lastCommand = string.Empty;
            MenuKeys mk = MenuKeys.None;
            if (args.Length == 1 && args[0].Length == 1)
            {
                switch (args[0].ToUpper())
                {
                    case "M": mk = MenuKeys.MainMenu; break;
                    case "B": mk = MenuKeys.BackToParent; break;
                    case "Q": mk = MenuKeys.Quit; break;
                    default: mk = MenuKeys.None; break;
                }
            }
            KeysConverter kc = new KeysConverter();
            Keys pressKey = Keys.None;
            try { pressKey = (Keys)kc.ConvertFromString(args[0].ToUpper()); }
            catch (NotSupportedException) { }
            catch (ArgumentException) { }

            if (!string.IsNullOrEmpty(CurrentPath))
            {
                previousPath = string.Join("\\", paths, 0, paths.Length - 1);
                lastCommand = paths[paths.Length - 1];
            }
            try
            {
                MethodInfo mi = GetMenuMethod(CurrentPath);
                if (mi == null)
                {
                    Menu_M(null, null);
                    return;
                }

                #region 處理輸入類選單 - 未檢驗
                var ics = mi.GetCustomAttributes(typeof(InputCommandAttribute), false);
                if (ics != null && ics.Length != 0 && mk == MenuKeys.None)
                {
                    InputCommandAttribute ica = ics[0] as InputCommandAttribute;
                    string[] prompts = new string[ica.Prompts.Keys.Count];
                    ica.Prompts.Keys.CopyTo(prompts, 0);
                    if (string.IsNullOrEmpty(_TelnetInput))
                    {
                        if (ica.Prompts[prompts[0]] != null && ica.Prompts[prompts[0]].Length != 0)
                        {
                            bool isExists = false;
                            foreach (string s in ica.Prompts[prompts[0]])
                            {
                                isExists = s.ToUpper().Equals(args[0].ToUpper(), StringComparison.OrdinalIgnoreCase);
                                if (isExists) break;
                            }
                            if (isExists)
                            {
                                _TelnetInput = cmdLine;
                                _Client.SendData("\r\n" + GetCommandPrompt(prompts[1]));
                            }
                            else
                            {
                                _Client.SendData(CsiBuilder.CreateString(SgrColors.Red, $"{Environment.NewLine}Invalid input!!!{Environment.NewLine}", true));
                                _Client.SendData(GetCommandPrompt(prompts[0]));
                            }
                        }
                        else
                        {
                            _TelnetInput = cmdLine;
                            _Client.SendData(GetCommandPrompt(prompts[1]));
                        }
                    }
                    else
                    {
                        Server.SetCommandEndChar(_Client.RemoteEndPoint, TelnetServer.CommandEndCharType.None);
                        List<string> tmp = new List<string>()
                        {
                            _TelnetInput,
                            cmdLine
                        };
                        if (tmp.Count >= ica.Prompts.Keys.Count)
                        {
                            bool callFN = false;
                            if (ica.Prompts[prompts[prompts.Length - 1]] != null && ica.Prompts[prompts[prompts.Length - 1]].Length != 0)
                            {
                                bool isExists = false;
                                foreach (string s in ica.Prompts[prompts[prompts.Length - 1]])
                                {
                                    isExists = s.Equals(cmdLine, StringComparison.OrdinalIgnoreCase);
                                    if (isExists) break;
                                }
                                if (isExists)
                                    callFN = true;
                                else
                                {
                                    _Client.SendData(CsiBuilder.CreateString(SgrColors.Red, $"{Environment.NewLine}Invalid input, please try again!!!{Environment.NewLine}", true));
                                    _Client.SendData(GetCommandPrompt(prompts[prompts.Length - 1]));
                                }
                            }
                            else
                                callFN = true;
                            if (callFN)
                            {
                                ica.Method.Invoke(this, new object[] { tmp.ToArray() });
                                _TelnetInput = string.Empty;
                                mi = GetMenuMethod(previousPath);
                                if (mi != null)
                                {
                                    paths = previousPath.Split('\\');
                                    previousPath = string.Join("\\", paths, 0, paths.Length - 1);
                                    lastCommand = paths[paths.Length - 1];
                                    mi.Invoke(this, new object[] { previousPath, lastCommand });
                                }
                            }
                        }
                        else
                        {
                            bool isExists = false;
                            foreach (string s in ica.Prompts[prompts[prompts.Length - 1]])
                            {
                                isExists = s.Equals(cmdLine, StringComparison.OrdinalIgnoreCase);
                                if (isExists) break;
                            }
                            if (isExists)
                            {
                                _TelnetInput = string.Join(" ", tmp.ToArray());
                                _Client.SendData(Environment.NewLine + GetCommandPrompt(prompts[tmp.Count]));
                            }
                            else
                            {
                                _Client.SendData(CsiBuilder.CreateString(SgrColors.Red, $"{Environment.NewLine}Invalid input, please try again!!!{Environment.NewLine}", true));
                                _Client.SendData(GetCommandPrompt(prompts[tmp.Count - 1]));
                            }
                        }
                    }
                    return;
                }
                else if (Array.IndexOf(new string[] { "Q", "M", "B" }, args[0].ToUpper()) != -1)
                    Server.SetCommandEndChar(_Client.RemoteEndPoint, TelnetServer.CommandEndCharType.None);
                #endregion

                #region 處理選擇類選單, 包含方向鍵控制
                if (pressKey != Keys.None)
                {
                    allowKeys = ((MenuCommandAttribute)mi.GetCustomAttributes(typeof(MenuCommandAttribute), false)[0]).Keys;
                    if (mk != MenuKeys.None)
                    {
                        _TelnetCursor = CursorLocation.Empty;
                    }
                    if (mk == MenuKeys.Quit)
                    {
                        _Client.Close();
                    }
                    else if (paths.Length <= 2 && (mk == MenuKeys.MainMenu || mk == MenuKeys.BackToParent))
                    {
                        Menu_M(null, null);
                    }
                    else if (mk == MenuKeys.BackToParent)
                    {
                        mi = GetMenuMethod(previousPath);
                        paths = previousPath.Split('\\');
                        previousPath = string.Join("\\", paths, 0, paths.Length - 1);
                        lastCommand = paths[paths.Length - 1];
                        mi.Invoke(this, new object[] { previousPath, lastCommand });

                    }
                    else if (Array.IndexOf(allowKeys, pressKey) != -1)
                    {
                        string nextMenu = $"{CurrentPath}\\{args[0].ToUpper()}";
                        mi = GetMenuMethod(nextMenu);
                        mi?.Invoke(this, new object[] { CurrentPath, args[0].ToUpper() });
                    }
                    else
                    {
                        object[] atts = null;
                        bool isExecuteCmd = false;
                        #region 處理命令執行類函示
                        atts = mi.GetCustomAttributes(typeof(ExecuteCommandAttribute), false);
                        ExecuteCommandAttribute eca = null;
                        foreach (object o in atts)
                        {
                            eca = o as ExecuteCommandAttribute;
                            if (eca.Key.Equals(pressKey))
                            {
                                eca.Method.Invoke(this, new object[] { eca.Variables });
                                if (eca.ContinueNextStep)
                                {
                                    mi = GetMenuMethod(CurrentPath);
                                    mi.Invoke(this, new object[] { CurrentPath, null });
                                }
                                isExecuteCmd = true;
                                break;
                            }
                        }
                        #endregion
                        #region 處理方向鍵控制類功能
                        if (!isExecuteCmd)
                        {
                            atts = mi.GetCustomAttributes(typeof(CursorControlAttribute), false);
                            if (atts != null && atts.Length != 0)
                            {
                                CursorControlAttribute cca = null;
                                foreach (object o in atts)
                                {
                                    cca = (CursorControlAttribute)o;
                                    if (cca.KeyCode.Equals(args[0]) || cca.KeyCode.Equals(cmdLine))
                                    {
                                        cca.Method.Invoke(this, new object[] { cca.Variables });
                                        isExecuteCmd = true;
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            }
            catch (NotImplementedException)
            {
                if (_Client != null)
                {
                    _Client.SendData(CsiBuilder.CreateString(SgrColors.Red, "Function not implemented!!!", true));
                    _Client.SendData(GetCommandPrompt());
                }
            }
            catch (Exception)
            {
                if (_Client != null)
                    Menu_M(null, null);
            }
        }


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
            CsiBuilder sb = new CsiBuilder();
            if (cleanScreen)
            {
                sb.AppendCommand(CsiCommands.Cls);
                sb.AppendCommand(CsiCommands.CursorPosition, 1, 1);
            }
            sb.AppendLine("Telnet Server Tester");
            sb.AppendLine();
            return sb.ToString();
        }
        #endregion

        #region Private Method : string GetCommandPrompt(string prompt = "Choose")
        private string GetCommandPrompt(string prompt = "Choose")
        {
            CsiBuilder sb = new CsiBuilder();
            if (string.IsNullOrEmpty(prompt) || prompt.Equals("Choose", StringComparison.OrdinalIgnoreCase))
            {
                sb.Append(SgrColors.Yellow, "Choose");
                sb.Append(SgrColors.None, "(");
                sb.Append(SgrColors.Cyan, Server.Connections.ToString());
                sb.Append(SgrColors.None, "/");
                sb.Append(SgrColors.Cyan, Server.MaxConnections.ToString());
                sb.Append(SgrColors.None, "):");
            }
            else
            {
                sb.Append(SgrColors.Yellow, prompt);
                sb.Append(SgrColors.None);
            }
            return sb.ToString();
        }
        #endregion

        #region Private Method : string GetMenuFooter(string prompt = "Choose")
        private string GetMenuFooter(string prompt = "Choose")
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine(" [M]ain menu, [B]ack up menu, [Q]uit");
            sb.AppendLine("=".PadRight(30, '='));
            sb.Append(GetCommandPrompt(prompt));
            return sb.ToString();
        }
        #endregion

        #region Private Method : string GetTrackBar(string title, int value, bool focus)
        private string GetTrackBar(string title, int value, bool focus)
        {
            CsiBuilder csi = new CsiBuilder("#".PadRight(TRACK_BAR_SIZE, '#'));

            float step = (TRACK_BAR_MAX - TRACK_BAR_MIN) / (float)TRACK_BAR_SIZE;
            int addr = (int)Math.Round((value - TRACK_BAR_MIN) / step);
            csi.Insert(addr, SgrColors.DarkGray);
            csi.Insert(0, SgrColors.Green);
            csi = new CsiBuilder(title + " " + csi.ToString());
            if (focus)
                csi.Insert(0, SgrColors.White, SgrColors.DarkBlue, " >");
            else
                csi.Insert(0, SgrColors.Gray, "  ");
            csi.Append(SgrColors.White, SgrColors.DarkBlue, $" {value}");
            csi.Append(SgrColors.None);
            return csi.ToString();
        }
        #endregion

        #region Private Method : void Exec_SendMessage(AsyncClient ac, object[] data)
        private void Exec_SendMessage(AsyncClient ac, object[] data)
        {
            _Client.SendData(data[0].ToString());
            Thread.Sleep(TELNET_CMD_EXEC_SLEEP);
        }
        #endregion

        #region Private Method : void Menu_M(string parent, string cmd)
        /// <summary>主選單</summary>
        /// <param name="parent">上層路徑</param>
        /// <param name="cmd">指令</param>
        [MenuCommand(Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.Q)]
        [ExecuteCommand(Keys.D4, nameof(Exec_SendMessage), "Message...")]
        private void Menu_M(string parent, string cmd)
        {
            // 主選單
            StringBuilder sb = new StringBuilder();
            sb.Append(GetMenuHeader(true));
            sb.AppendLine(" Main Menu");
            sb.AppendLine("=".PadRight(30, '='));
            sb.AppendLine("  [1] Menu 1 - General Menu");
            sb.AppendLine("  [2] Menu 2 - TrackBar Menu");
            sb.AppendLine("  [3] Menu 3 - CheckBox/RadioBox Menu");
            sb.AppendLine("  [4] Menu 4 - Show Text \"Message...\"");
            sb.AppendLine();
            sb.AppendLine("  [Q]uit");
            sb.AppendLine("=".PadRight(30, '='));
            sb.Append(GetCommandPrompt());
            _Client.SendData(sb.ToString());
            CurrentPath = "M";
        }
        #endregion

        #region Private Method : void Menu_M_1(string parent, string cmd)
        /// <summary>第一層選單：Device Test</summary>
        /// <param name="parent">上層路徑</param>
        /// <param name="cmd">指令</param>
        [MenuCommand(Keys.D1, Keys.M, Keys.B, Keys.Q)]
        private void Menu_M_1(string parent, string cmd)
        {
            CurrentPath = parent;
            if (!string.IsNullOrEmpty(cmd))
                CurrentPath += "\\" + cmd;
            StringBuilder sb = new StringBuilder();
            sb.Append(GetMenuHeader());
            sb.AppendLine(" Main Menu > Menu 1 - General Menu");
            sb.AppendLine("=".PadRight(30, '='));
            sb.AppendLine(" [1] Level 1 - Menu 1(Do nothing)");
            sb.Append(GetMenuFooter());
            _Client.SendData(sb.ToString());
        }
        #endregion

        #region Private Method : void Menu_M_2(string parent, string cmd)
        /// <summary>第三層選單：Device Test\PICU\Adjust MIC Volume</summary>
        /// <param name="parent">上層路徑</param>
        /// <param name="cmd">指令</param>
        [MenuCommand(Keys.M, Keys.B, Keys.Q)]
        [CursorControl(Keys.Up, nameof(Menu_CursorMoveUp))]
        [CursorControl(Keys.Down, nameof(Menu_CursorMoveDn))]
        [CursorControl(Keys.Left, nameof(Menu_CursorMoveLf))]
        [CursorControl(Keys.Right, nameof(Menu_CursorMoveRt))]
        [CursorControl(Keys.Space, nameof(Menu_CursorCheck))]
        private void Menu_M_2(string parent, string cmd)
        {
            CurrentPath = parent;
            if (!string.IsNullOrEmpty(cmd))
                CurrentPath += "\\" + cmd;
            StringBuilder sb = new StringBuilder();

            if (false)
            {
                sb.Append(GetMenuHeader());
                sb.AppendLine(" Main Menu > Menu 2 - TrackBar Menu");
                sb.AppendLine("=".PadRight(74, '='));
                if (_TelnetCursor.IsEmpty)
                {
                    int c = 18;
                    int step = (TRACK_BAR_MAX - TRACK_BAR_MIN) / TRACK_BAR_SIZE;
                    c += TRACK_BAR_MIN / step;
                    _TelnetCursor = new CursorLocation(6, c);
                }
                string tmp = string.Empty;
                string[] ks = new string[TrackValues.Count];
                TrackValues.Keys.CopyTo(ks, 0);
                Array.Sort<string>(ks);
                for (int i = 0; i < ks.Length; i++)
                    sb.AppendLine(GetTrackBar(ks[i], TrackValues[ks[i]], _TelnetCursor.Row == i));
                //sb.AppendLine("123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
                //sb.AppendLine("         1         2         3         4         5         6         7         8         9");
            }
            for (int i = 0; i < 16; i++)
                sb.AppendLine($"\x1B[48;5;{i}m　");
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 36; j++)
                    sb.Append($"\x1B[48;5;{(16 + i * 36 + j)}m　");
                sb.AppendLine();
            }
            for (int i = 0; i < 24; i++)
                sb.AppendLine($"\x1B[48;5;{(i + 232)}m　");
            sb.AppendLine(GetMenuFooter());
            _Client.SendData(sb.ToString());
        }
        #endregion

        #region Menu Cursor Control
        #region Private Method : void Menu_CursorMoveUp(params object[] data)
        private void Menu_CursorMoveUp(params object[] data)
        {
            //* data Array:
            //* 0 : Cursor Min Y
            //* 1 : Rows
            int minY = Convert.ToInt32(data[0]);
            CursorLocation orig = _TelnetCursor.Clone();
            _TelnetCursor.Row--;
            if (_TelnetCursor.Row < minY)
                _TelnetCursor.Row = minY + Convert.ToInt32(data[1]);
            CsiBuilder csi = new CsiBuilder();
            csi.AppendCommand(CsiCommands.SaveCursorPosition);
            if (!_TelnetCursor.Equals(orig))
            {
                csi.AppendCommand(CsiCommands.CursorPosition, orig.Row, TrackValues[$"Track {orig.Row - minY}"] + minY);
                csi.Append(GetTrackBar($"Track {orig.Row - minY}", TrackValues[$"Track {orig.Row - minY}"], true));
                csi.AppendCommand(CsiCommands.CursorPosition, _TelnetCursor.Row, TrackValues[$"Track {_TelnetCursor.Row - minY}"] + minY);
                csi.Append(GetTrackBar($"Track {_TelnetCursor.Row - minY}", TrackValues[$"Track {_TelnetCursor.Row - minY}"], true));
            }
            csi.AppendCommand(CsiCommands.RestoreCursorPosition);
            _Client.SendData(csi.ToString());
        }
        #endregion

        #region Private Method : void Menu_CursorMoveDn(params object[] data)
        private void Menu_CursorMoveDn(params object[] data)
        {
            //* data Array:
            //* 0 : Cursor Min Y
            //* 1 : Rows
            int minY = Convert.ToInt32(data[0]);
            int maxY = minY + Convert.ToInt32(data[1]) - 1;
            CursorLocation orig = _TelnetCursor.Clone();
            _TelnetCursor.Row++;
            if (_TelnetCursor.Row >= maxY)
                _TelnetCursor.Row = minY;
            CsiBuilder csi = new CsiBuilder();
            csi.AppendCommand(CsiCommands.SaveCursorPosition);
            if (!_TelnetCursor.Equals(orig))
            {
                Thread.Sleep(50);
                csi.AppendCommand(CsiCommands.CursorPosition, orig.Row, TrackValues[$"Track {orig.Row - minY}"] + minY);
                csi.Append(GetTrackBar($"Track {orig.Row - minY}", TrackValues[$"Track {orig.Row - minY}"], true));
                csi.AppendCommand(CsiCommands.CursorPosition, _TelnetCursor.Row, TrackValues[$"Track {_TelnetCursor.Row - minY}"] + minY);
                csi.Append(GetTrackBar($"Track {_TelnetCursor.Row - minY}", TrackValues[$"Track {_TelnetCursor.Row - minY}"], true));
            }
            csi.AppendCommand(CsiCommands.RestoreCursorPosition);
            _Client.SendData(csi.ToString());
        }
        #endregion

        #region Private Method : void Menu_CursorMoveLf(params object[] data)
        private void Menu_CursorMoveLf(params object[] data)
        {
            //* data Array:
            //* 0 : Cursor Min Y
            //* 1 : Cursor Min X
            //* 2 : Min Value
            //* 3 : Max Value
            int minY = Convert.ToInt32(data[0]);
            int minX = Convert.ToInt32(data[1]);
            int min = Convert.ToInt32(data[2]);
            int max = Convert.ToInt32(data[3]);
            int steps = (TRACK_BAR_MAX - TRACK_BAR_MIN) / TRACK_BAR_SIZE;
            int stepVal = (max - min) / steps;
            CursorLocation orig = _TelnetCursor.Clone();
            _TelnetCursor.Col--;
            if (_TelnetCursor.Col < minX)
                _TelnetCursor.Col = minX;
            CsiBuilder csi = new CsiBuilder();
            csi.AppendCommand(CsiCommands.SaveCursorPosition);
            if (!_TelnetCursor.Equals(orig))
            {
                short val = (short)((_TelnetCursor.Col - minX) * stepVal + min);
                TrackValues[$"Track {_TelnetCursor.Row - minY}"] = val;
                Thread.Sleep(50);
                csi.AppendCommand(CsiCommands.CursorPosition, _TelnetCursor.Row, 1);
                csi.Append(GetTrackBar($"Track {_TelnetCursor.Row - minY}", val, true));
            }
            csi.AppendCommand(CsiCommands.RestoreCursorPosition);
            _Client.SendData(csi.ToString());
        }
        #endregion

        #region Private Method : void Menu_CursorMoveRt(params object[] data)
        private void Menu_CursorMoveRt(params object[] data)
        {
            //* data Array:
            //* 0 : Cursor Min Y
            //* 1 : Cursor Min X
            //* 2 : Min Value
            //* 3 : Max Value
            int minY = Convert.ToInt32(data[0]);
            int minX = Convert.ToInt32(data[1]);
            int min = Convert.ToInt32(data[2]);
            int max = Convert.ToInt32(data[3]);
            int steps = (TRACK_BAR_MAX - TRACK_BAR_MIN) / TRACK_BAR_SIZE;
            int stepVal = (max - min) / steps;
            CursorLocation orig = _TelnetCursor.Clone();
            _TelnetCursor.Col++;
            if (_TelnetCursor.Col > minX + TRACK_BAR_SIZE)
                _TelnetCursor.Col = minX + TRACK_BAR_SIZE;
            CsiBuilder csi = new CsiBuilder();
            csi.AppendCommand(CsiCommands.SaveCursorPosition);
            if (!_TelnetCursor.Equals(orig))
            {
                short val = (short)((_TelnetCursor.Col - minX) * stepVal + min);
                TrackValues[$"Track {_TelnetCursor.Row - minY}"] = val;
                Thread.Sleep(50);
                csi.AppendCommand(CsiCommands.CursorPosition, _TelnetCursor.Row, _TelnetCursor.Col);
                csi.Append(GetTrackBar($"Track {_TelnetCursor.Row - minY}", val, true));
            }
            csi.AppendCommand(CsiCommands.RestoreCursorPosition);
            _Client.SendData(csi.ToString());
        }
        #endregion

        #region Private Method : void Menu_CursorCheck(params object[] data)
        private void Menu_CursorCheck(params object[] data)
        {
            CsiBuilder csi = new CsiBuilder();
            csi.AppendCommand(CsiCommands.SaveCursorPosition);

            csi.AppendCommand(CsiCommands.RestoreCursorPosition);
            _Client.SendData(csi.ToString());
        }
        #endregion
        #endregion
    }
    #endregion

}
