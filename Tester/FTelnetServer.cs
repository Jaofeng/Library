﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CJF.Net;
using CJF.Net.Telnet;
using CJF.Utility;
using CJF.Utility.Extensions;

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

		readonly string[] AllowAccount = new string[] { "root", "1234" };

		TelnetServer _Server = null;
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

		#region Server Actions
		void Server_OnShutdown(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:伺服器已關閉");
			ClearListBoxItem(lbRemotes);
		}

		void Server_OnStarted(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:伺服器已於 {0} 啟動", _Server.LocalEndPort);
			WriteLog("M:接線池剩餘：{0}", _Server.MaxConnections - _Server.Connections);
		}
		void Server_OnClientClosed(object sender, SocketServerEventArgs e)
		{
			TelnetServer svr = (TelnetServer)sender;
			if (svr == null || !svr.IsStarted)
				return;
			WriteLog("M:用戶端 {0} 已關閉連線", e.RemoteEndPoint);
			WriteLog("M:接線池剩餘：{0}", svr.MaxConnections - svr.Connections);
			RemoveListBoxItem(lbRemotes, e.RemoteEndPoint);
			SetConteolText(gbClients, string.Format("Clients : {0}", svr.Connections));
		}
		void Server_OnClientClosing(object sender, SocketServerEventArgs e)
		{
			WriteLog("M:用戶端 {0} 正停止連線", e.RemoteEndPoint);
		}

		void Server_OnClientConnected(object sender, SocketServerEventArgs e)
		{
			TelnetServer svr = (TelnetServer)sender;
			WriteLog("用戶端 {0} 已連線", e.RemoteEndPoint);
			WriteLog("M:接線池剩餘：{0}", svr.MaxConnections - svr.Connections);
			SetConteolText(gbClients, string.Format("Clients : {0}", svr.Connections));
			AppendListBoxItem(lbRemotes, e.RemoteEndPoint);
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat("\x1B[2J\x1B[1;1H{0} v{1}", Application.ProductName, Application.ProductVersion);
			sb.AppendLine();
			sb.AppendLine("=".PadLeft(80, '='));
			sb.AppendLine("[1] Menu 1");
			e.Client.SendData(sb.ToString());
			svr.SetCommandEndChar(e.RemoteEndPoint, TelnetServer.CommandEndCharType.CrLf);
		}
		#endregion

		void Server_OnDataReceived(object sender, SocketServerEventArgs e)
		{
			TelnetServer svr = (TelnetServer)sender;
			string data = Encoding.Default.GetString(e.Data);
			WriteLog("[{0}] > ({2:D3}){1}", e.RemoteEndPoint, e.Data.ToHexString(), e.Data.Length);
			if (data.Equals("close", StringComparison.OrdinalIgnoreCase))
				e.Client.Close();
					}

		void Server_OnDataSended(object sender, SocketServerEventArgs e)
		{
			WriteLog("[{0}] < ({2:D3}){1}", e.RemoteEndPoint, e.Data.ToHexString(), e.Data.Length);
		}

		#region Button Events
		private void btnListen_Click(object sender, EventArgs e)
		{
			if (_Server == null)
			{
				_Server = new TelnetServer(10, Encoding.UTF8);
				_Server.Authentication = "root,1234,admin,P@ssW0rd";
				_Server.OnClientConnected += new EventHandler<SocketServerEventArgs>(Server_OnClientConnected);
				_Server.OnClientClosing += new EventHandler<SocketServerEventArgs>(Server_OnClientClosing);
				_Server.OnClientClosed += new EventHandler<SocketServerEventArgs>(Server_OnClientClosed);
				_Server.OnDataReceived += new EventHandler<SocketServerEventArgs>(Server_OnDataReceived);
				_Server.OnDataSended += new EventHandler<SocketServerEventArgs>(Server_OnDataSended);
				_Server.OnStarted += new EventHandler<SocketServerEventArgs>(Server_OnStarted);
				_Server.OnShutdown += new EventHandler<SocketServerEventArgs>(Server_OnShutdown);
				_Server.PopupCommands = chkPopupCommands.Checked;
			}
			_Server.Start(new IPEndPoint(IPAddress.Any, Convert.ToInt32(txtPort.Text)));
			btnListen.Enabled = false;
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
			if (e.KeyCode != Keys.Enter)
				return;
			if (lbRemotes.SelectedItems == null || lbRemotes.SelectedItems.Count == 0)
			{
				AsyncClient[] acs = _Server.SocketServer.Clients;
				for (int i = 0; i < acs.Length; i++)
				{
					if (chkHexString.Checked)
						acs[i].SendData(txtSendMsg.Text.ToByteArray());
					else
						acs[i].SendData(txtSendMsg.Text);
				}
			}
			else
			{
				AsyncClient ac = null;
				foreach (object o in lbRemotes.SelectedItems)
				{
					ac = _Server.SocketServer.FindClient((System.Net.EndPoint)o);
					if (ac != null)
					{
						if (chkHexString.Checked)
							ac.SendData(txtSendMsg.Text.ToByteArray());
						else
							ac.SendData(txtSendMsg.Text);
					}
				}
			}
			txtSendMsg.SelectAll();
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
	}
}
