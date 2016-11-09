using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;
using CJF.Net;
using CJF.Net.Multicast;
using CJF.Utility;

namespace Tester
{
	public partial class FMulticast : Form
	{
		CastReceiver _Receiver = null;
		CastSender _Sender = null;
		public FMulticast()
		{
			InitializeComponent();
			cbIP.Items.AddRange(TcpManager.GetHostIP());
			string[] ip = txtSendIP.Text.Split(':');
			IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip[0]), int.Parse(ip[1]));
			_Sender = new CastSender(ep, int.Parse(ip[2]));
			_Sender.OnDataSended += new EventHandler<AsyncUdpEventArgs>(Client_OnDataSended);
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
			WriteLog("伺服器已關閉");
		}

		void Server_OnStarted(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("伺服器已於 {0} 啟動", _Receiver.Socket.LocalEndPoint);
		}
		#endregion

		void Server_OnDataReceived(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("收到資料({0}->{1}) {2} Bytes", e.RemoteEndPoint, e.LocalEndPoint, e.Data.Length);
			WriteLog(" > : {0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex: {0}", ConvUtils.Byte2HexString(e.Data));
		}

		#region Button Events
		private void btnStart_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(cbIP.Text)) return;
			_Receiver = new CastReceiver(new IPEndPoint(IPAddress.Parse(cbIP.Text), Convert.ToInt32(txtPort.Text)));
			_Receiver.OnDataReceived += new EventHandler<AsyncUdpEventArgs>(Server_OnDataReceived);
			_Receiver.OnStarted += new EventHandler<AsyncUdpEventArgs>(Server_OnStarted);
			_Receiver.OnShutdown += new EventHandler<AsyncUdpEventArgs>(Server_OnShutdown);
			string[] ips = txtIP.Text.Split(',');
			foreach (string ip in ips)
			{
				if (ip.Length == 0) continue;
				_Receiver.JoinMulticastGroup(ip);
			}
			_Receiver.Start();

			btnStart.Enabled = false;
			btnStop.Enabled = true;
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			_Receiver.Shutdown();
			btnStop.Enabled = false;
			btnStart.Enabled = true;
		}
		#endregion

		private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
				return;
			string[] ip = txtSendIP.Text.Split(':');
			IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip[0]), int.Parse(ip[1]));
			if (_Sender == null || (_Sender != null && !_Sender.RemoteEndPoint.Equals(ep)))
			{
				if (_Sender != null)
					_Sender.Dispose();
				_Sender = new CastSender(ep, int.Parse(ip[2]));
				_Sender.OnDataSended += new EventHandler<AsyncUdpEventArgs>(Client_OnDataSended);
			}
			byte[] buf = null;
			if (chkHexString.Checked)
				buf = ConvUtils.HexStringToBytes(txtSendMsg.Text);
			else
				buf = Encoding.Default.GetBytes(txtSendMsg.Text);
			_Sender.SendData(buf);
			txtSendMsg.SelectAll();
			e.SuppressKeyPress = true;
		}

		void Client_OnDataSended(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("發送資料 {0} Bytes", e.Data.Length);
			WriteLog(" < : {0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex: {0}", ConvUtils.Byte2HexString(e.Data));
		}

		private void FMulticast_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_Receiver != null)
				_Receiver.Dispose();
			_Receiver = null;
		}

		private void txtSendIP_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
				return;
			e.SuppressKeyPress = true;
			txtSendMsg.Focus();
		}
	}
}
