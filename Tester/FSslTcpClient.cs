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
using CJF.Net.Http;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace Tester
{
	public partial class FSslTcpClient : Form
	{
		SslTcpClient _Client = null;


		public FSslTcpClient()
		{
			InitializeComponent();
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

		#region Client Actions
		void Client_OnClosed(object sender, SslTcpEventArgs e)
		{
			WriteLog("已與伺服器 {0} 關閉連線", e.RemoteEndPoint);
		}
		void Client_OnConnected(object sender, SslTcpEventArgs e)
		{
			WriteLog("已與伺服器 {0} 連線", e.RemoteEndPoint);
		}
		void Client_OnDataReceived(object sender, SslTcpEventArgs e)
		{
			string data = Encoding.UTF8.GetString(e.Data);
			WriteLog("自 {0} 收到資料, {1} Bytes", e.RemoteEndPoint, e.Data.Length);
			if (chkHexString.Checked)
				WriteLog("Hex:{0}", e.Data.ToHexString());
			else
				WriteLog(" > :{0}", data);
		}
		void Client_OnDataSended(object sender, SslTcpEventArgs e)
		{
			WriteLog("送出資料 {0} Bytes", e.Data.Length);
			if (chkHexString.Checked)
				WriteLog("Hex:{0}", e.Data.ToHexString());
			else
				WriteLog(" > :{0}", Encoding.Default.GetString(e.Data));
		}
		void Client_AuthenticateFail(object sender, SslTcpEventArgs e)
		{
			WriteLog("與伺服器 {0} 認證失敗", e.RemoteEndPoint);
			btnStop_Click(null, null);
		}

		#endregion

		#region Button Events
		private void btnStart_Click(object sender, EventArgs e)
		{
			if (_Client == null)
			{
				string cer = txtCer.Text;
				if (string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(cer)))
					cer = System.IO.Path.Combine(Environment.CurrentDirectory, "Web", cer);
				System.Security.Cryptography.X509Certificates.X509Certificate cert =
					System.Security.Cryptography.X509Certificates.X509Certificate.CreateFromCertFile(cer);
				SslTcpClient.BypassIssuer = cert.Issuer;
				_Client = SslTcpClient.ConnectTo(new IPEndPoint(IPAddress.Parse(txtIP.Text), Convert.ToInt32(txtPort.Text)), cer);
				_Client.Connected += new EventHandler<SslTcpEventArgs>(Client_OnConnected);
				_Client.Closed += new EventHandler<SslTcpEventArgs>(Client_OnClosed);
				_Client.DataReceived += new EventHandler<SslTcpEventArgs>(Client_OnDataReceived);
				_Client.DataSended += new EventHandler<SslTcpEventArgs>(Client_OnDataSended);
				_Client.AuthenticateFail += new EventHandler<SslTcpEventArgs>(Client_AuthenticateFail);
			}
			btnStart.Enabled = false;
			btnStop.Enabled = true;
		}
		private void btnStop_Click(object sender, EventArgs e)
		{
			SetButtonEnabled(btnStop, false);
			_Client.Close();
			_Client = null;
			SetButtonEnabled(btnStart, true);
		}
		#endregion

		#region SetButtonEnabled
		delegate void SetButtonEnabledCallback(Button btn, bool enabled);
		void SetButtonEnabled(Button btn, bool enabled)
		{
			if (btn.InvokeRequired)
				this.Invoke(new MethodInvoker(() => SetButtonEnabled(btn, enabled)));
			else
				btn.Enabled = enabled;
		}
		#endregion

		private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
		{
			if (_Client != null && e.KeyCode == Keys.Enter)
			{
				if (chkHexString.Checked)
					_Client.SendData(txtSendMsg.Text.ToByteArray());
				else
					_Client.SendData(Encoding.UTF8.GetBytes(txtSendMsg.Text));
				txtSendMsg.SelectAll();
				e.SuppressKeyPress = true;
				e.Handled = false;
			}
		}
	}
}
