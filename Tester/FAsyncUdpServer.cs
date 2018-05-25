using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;
using CJF.Net;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace Tester
{
	public partial class FAsyncUdpServer : Form
	{
		AsyncUDP _Server = null;
		public FAsyncUdpServer()
		{
			InitializeComponent();
			_Server = new AsyncUDP(Convert.ToInt32(txtBuffer.Text));
			_Server.DataReceived += new EventHandler<AsyncUdpEventArgs>(Server_OnDataReceived);
			_Server.MonitorStarted += new EventHandler<AsyncUdpEventArgs>(Server_OnStarted);
			_Server.MonitorStoped += new EventHandler<AsyncUdpEventArgs>(Server_OnShutdown);
			_Server.Exception += new EventHandler<AsyncUdpEventArgs>(Server_OnException);
			foreach (IPAddress ipa in TcpManager.GetHostIP())
				cboIP.Items.Add(ipa.ToString());
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
		void Server_OnShutdown(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("M:伺服器已關閉");	
		}

		void Server_OnStarted(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("M:伺服器已於 {0} 啟動", _Server.Socket.LocalEndPoint);
		}
		void Server_OnException(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("M:伺服器發生錯誤:{0}", e.Exception.Message);
		}
		#endregion

		void Server_OnDataReceived(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("收到資料 {0} Bytes 來自 {1}", e.Data.Length, e.RemoteEndPoint);
			WriteLog(" > : {0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex: {0}", e.Data.ToHexString());
		}

		#region Button Events
		private void btnStart_Click(object sender, EventArgs e)
		{
			btnStart.Enabled = false;
			_Server.Start(new IPEndPoint(IPAddress.Parse(cboIP.Text), Convert.ToInt32(txtPort.Text)));
			if (_Server.IsStarted)
				btnStop.Enabled = true;
			else
				btnStart.Enabled = true;
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			_Server.Shutdown();
			btnStop.Enabled = false;
			btnStart.Enabled = true;
		}
		#endregion

		private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
				return;
			string[] arr = txtSendTo.Text.Split(':');
			byte[] buf = null;
			if (chkHexString.Checked)
				buf = txtSendMsg.Text.ToByteArray();
			else
				buf = Encoding.Default.GetBytes(txtSendMsg.Text);
			if (chkBroadcast.Checked)
				AsyncUDP.Broadcast(Convert.ToInt32(arr[1]), buf);
			else
				_Server.SendData(new IPEndPoint(IPAddress.Parse(arr[0]), Convert.ToInt32(arr[1])), buf);
			txtSendMsg.SelectAll();
		}

		private void chkBroadcast_CheckedChanged(object sender, EventArgs e)
		{
			_Server.EnableBroadcast = chkBroadcast.Checked;
		}

		private void cboIP_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtSendTo.Text))
				txtSendTo.Text = string.Format("{0}:{1}", cboIP.Text, txtPort.Text);
		}
	}
}
