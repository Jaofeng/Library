using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using CJF.Net;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace Tester
{
	public partial class FWebSocketServer : Form
	{
		WebSocketServer _Server = null;

		/// <summary>SHA1 加密類別</summary>
		SHA1 m_sha = null;
		/// <summary>WebSocket 專用 GUID</summary>
		static readonly String GUID = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

		struct WebClientInfo
		{
			public bool ShankHands;
		}

		public FWebSocketServer()
		{
			InitializeComponent();
			m_sha = SHA1CryptoServiceProvider.Create();
			Process.Start(System.IO.Path.Combine(Environment.CurrentDirectory, "web", "Test.htm"));
		}

		#region WriteLog
		delegate void WriteLogCallback(string txt);
		void WriteLog(string txt)
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
		void WriteLog(string format, params object[] args)
		{
			WriteLog(string.Format(format, args));
		}
		#endregion

		#region Server Actions
		void Server_OnShutdown(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:伺服器已關閉");
		}

		void Server_OnStarted(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:伺服器已於 {0} 啟動", _Server.Socket.LocalEndPoint);
			WriteLog("M:接線池剩餘：{0}", _Server.PoolSurplus);
		}
		void Server_OnClientClosed(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:用戶端 {0} 已關閉連線", e.RemoteEndPoint);
			if (_Server != null)
				WriteLog("M:接線池剩餘：{0}", _Server.PoolSurplus);
		}
		void Server_OnClientClosing(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:用戶端 {0} 正停止連線", e.RemoteEndPoint);
		}

		void Server_OnClientConnected(object sender, SocketServerEventArgs e)
		{
			WriteLog("用戶端 {0} 已連線", e.RemoteEndPoint);
			WriteLog("M:接線池剩餘：{0}", _Server.PoolSurplus);
		}
		#endregion

		void Server_OnDataReceived(object sender, SocketServerEventArgs e)
		{
			string data = Encoding.UTF8.GetString(e.Data);
			WriteLog("收到資料 {0} Bytes", e.Data.Length);
			if (chkHexString.Checked)
				WriteLog("Hex:{0}", e.Data.ToHexString());
			else
				WriteLog(" > :{0}", data);
		}

		void Server_OnDataSended(object sender, SocketServerEventArgs e)
		{
			WriteLog("送出資料 {0} Bytes", e.Data.Length);
			if (chkHexString.Checked)
				WriteLog("Hex:{0}", e.Data.ToHexString());
			else
				WriteLog(" > :{0}", Encoding.Default.GetString(e.Data));
		}

		#region Button Events
		private void btnStart_Click(object sender, EventArgs e)
		{
			if (_Server == null)
			{
				_Server = new WebSocketServer(Convert.ToInt32(txtMaxConnect.Text));
				_Server.ClientConnected += new EventHandler<SocketServerEventArgs>(Server_OnClientConnected);
				_Server.ClientClosing += new EventHandler<SocketServerEventArgs>(Server_OnClientClosing);
				_Server.ClientClosed += new EventHandler<SocketServerEventArgs>(Server_OnClientClosed);
				_Server.DataReceived += new EventHandler<SocketServerEventArgs>(Server_OnDataReceived);
				_Server.DataSended += new EventHandler<SocketServerEventArgs>(Server_OnDataSended);
				_Server.Started += new EventHandler<SocketServerEventArgs>(Server_OnStarted);
				_Server.Shutdowned += new EventHandler<SocketServerEventArgs>(Server_OnShutdown);
				_Server.AppendService("/echo");
			}
			_Server.Start(new IPEndPoint(IPAddress.Parse(txtIP.Text), Convert.ToInt32(txtPort.Text)));
			btnStart.Enabled = false;
			btnStop.Enabled = true;
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			SetButtonEnabled(btnStop, false);
			_Server.Shutdown();
			_Server.Dispose();
			_Server = null;
			SetButtonEnabled(btnStart, true);
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

		private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				WebSocketClient[] acs = _Server.Clients;
				for (int i = 0; i < acs.Length; i++)
				{
					if (chkHexString.Checked)
						acs[i].SendData(txtSendMsg.Text.ToByteArray());
					else
						acs[i].SendData(txtSendMsg.Text);
				}
				txtSendMsg.SelectAll();
				e.SuppressKeyPress = true;
				e.Handled = false;
			}
		}
	}
}
