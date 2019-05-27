using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CJF.Net;
using CJF.Net.Multicast;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace Tester
{
	public partial class FMulticast : Form
	{
		CastReceiver _Receiver = null;
		//CastSender _Sender = null;
		public FMulticast()
		{
			InitializeComponent();
			cbIP.Items.AddRange(TcpManager.GetHostIP());
			//string[] ip = txtSendIP.Text.Split(':');
			//IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip[0]), int.Parse(ip[1]));
			//_Sender = new CastSender(ep, int.Parse(ip[2]));
			//_Sender.DataSended += new EventHandler<AsyncUdpEventArgs>(Client_DataSended);
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
		void Server_Shutdowned(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("伺服器已關閉");
		}

		void Server_Started(object sender, AsyncUdpEventArgs e)
		{
            CastReceiver cr = sender as CastReceiver;
            WriteLog($"伺服器已於 {cr.LocalEndPort} 啟動");
            WriteLog("已加入監聽的群組:");
            foreach (System.Net.Sockets.MulticastOption mo in cr.JoinedGroups)
                WriteLog($"-> {mo.Group}");
		}
		#endregion

		void Server_DataReceived(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("收到資料({0}->{1}) {2} Bytes", e.RemoteEndPoint, e.LocalEndPoint, e.Data.Length);
			WriteLog(" > : {0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex: {0}", e.Data.ToHexString());
		}

		#region Button Events
		private void btnStart_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(cbIP.Text)) return;
			_Receiver = new CastReceiver(new IPEndPoint(IPAddress.Parse(cbIP.Text), Convert.ToInt32(txtPort.Text)));
			_Receiver.DataReceived += new EventHandler<AsyncUdpEventArgs>(Server_DataReceived);
			_Receiver.Started += new EventHandler<AsyncUdpEventArgs>(Server_Started);
			_Receiver.Shutdowned += new EventHandler<AsyncUdpEventArgs>(Server_Shutdowned);
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
            byte[] buf = null;
            if (chkHexString.Checked)
                buf = txtSendMsg.Text.ToByteArray();
            else
                buf = Encoding.Default.GetBytes(txtSendMsg.Text);
            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip[0]), int.Parse(ip[1]));
            bool sended = false;
            int ttl = 100;
            if (ip.Length >= 3)
                ttl = int.Parse(ip[2]);
            using (CastSender ser = new CastSender(ep, ttl))
            {
                ser.DataSended += (s, se) =>
                {
                    WriteLog("發送資料 {0} Bytes", se.Data.Length);
                    WriteLog(" < : {0}", Encoding.Default.GetString(se.Data));
                    WriteLog("Hex: {0}", se.Data.ToHexString());
                    sended = true;
                };
                ser.SendData(buf);
                SpinWait.SpinUntil(() => sended, 1000);
            }
            //CastSender.SendData(ep, buf);
            txtSendMsg.SelectAll();
			e.SuppressKeyPress = true;
		}

		void Client_DataSended(object sender, AsyncUdpEventArgs e)
		{
			WriteLog("發送資料 {0} Bytes", e.Data.Length);
			WriteLog(" < : {0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex: {0}", e.Data.ToHexString());
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
