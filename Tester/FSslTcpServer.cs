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
using CJF.Net.Ssl;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace Tester
{
    #pragma warning disable IDE1006
    public partial class FSslTcpServer : Form
	{
		SslTcpServer _Server = null;


		public FSslTcpServer()
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

		#region Server Actions
		void Server_OnShutdown(object sender, EventArgs e)
		{
			WriteLog("伺服器已關閉");
		}

		void Server_OnStarted(object sender, EventArgs e)
		{
			WriteLog("伺服器已於 {0} 啟動", _Server.LocalEndPoint);
		}
		void Server_OnClientClosed(object sender, SslTcpEventArgs e)
		{
			WriteLog("用戶端 {0} 已關閉連線", e.RemoteEndPoint);
		}
		void Server_OnClientConnected(object sender, SslTcpEventArgs e)
		{
			WriteLog("用戶端 {0} 已連線", e.RemoteEndPoint);
		}
		void Server_OnDataReceived(object sender, SslTcpEventArgs e)
		{
			string data = Encoding.UTF8.GetString(e.Data);
			WriteLog("自 {0} 收到資料, {1} Bytes", e.RemoteEndPoint, e.Data.Length);
			if (chkHexString.Checked)
				WriteLog("Hex:{0}", e.Data.ToHexString());
			else
				WriteLog(" > :{0}", data);
		}
		void Server_OnDataSended(object sender, SslTcpEventArgs e)
		{
			WriteLog("送出資料 {0} Bytes", e.Data.Length);
			if (chkHexString.Checked)
				WriteLog("Hex:{0}", e.Data.ToHexString());
			else
				WriteLog(" > :{0}", Encoding.Default.GetString(e.Data));
		}
		void Server_AuthenticateFail(object sender, SslTcpEventArgs e)
		{
			WriteLog("用戶端 {0} 認證失敗", e.RemoteEndPoint);
		}

		#endregion

		#region Button Events
		private void btnStart_Click(object sender, EventArgs e)
		{
            btnStart.Enabled = false;
            if (_Server == null)
			{
				string pfx = txtPfx.Text;
				if (string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(pfx)))
					pfx = System.IO.Path.Combine(Environment.CurrentDirectory, "Web", pfx);
				if (txtIP.Text.Equals("0.0.0.0"))
					_Server = new SslTcpServer(IPAddress.Any, Convert.ToInt32(txtPort.Text), pfx, txtPfxPwd.Text);
				else
					_Server = new SslTcpServer(txtIP.Text, Convert.ToInt32(txtPort.Text), pfx, txtPfxPwd.Text);
				_Server.IdleTime = 0;
				_Server.ClientConnected += new EventHandler<SslTcpEventArgs>(Server_OnClientConnected);
				_Server.ClientClosed += new EventHandler<SslTcpEventArgs>(Server_OnClientClosed);
				_Server.DataReceived += new EventHandler<SslTcpEventArgs>(Server_OnDataReceived);
				_Server.DataSended += new EventHandler<SslTcpEventArgs>(Server_OnDataSended);
				_Server.AuthenticateFail += new EventHandler<SslTcpEventArgs>(Server_AuthenticateFail);
				_Server.Started += new EventHandler(Server_OnStarted);
				_Server.Shutdowned += new EventHandler(Server_OnShutdown);
                //_Server.CertificateStoreName = System.Security.Cryptography.X509Certificates.StoreName.TrustedPeople;
                _Server.CertificateValid = false;
			}
            try { _Server.Start(); }
            catch (NotImplementedException)
            {
                MessageBox.Show($"伺服器並未安裝此憑證 {txtPfx.Text}，或該憑證不在 SslTcpServer.CertificateStoreName 指定的區域內。", "SslTcpServer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                _Server.Dispose();
                _Server = null;
                btnStart.Enabled = true;
                return;
            }
            catch (NotSupportedException)
            {
                MessageBox.Show($"此憑證 {txtPfx.Text} 為非信任憑證，無法使用該憑證。\n請將 SslTcpServer.CertificateValid 設為 false，或將該憑證移至新任區域。", "SslTcpServer", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                _Server.Dispose();
                _Server = null;
                btnStart.Enabled = true;
                return;
            }
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
				this.Invoke(new MethodInvoker(() => SetButtonEnabled(btn, enabled)));
			else
				btn.Enabled = enabled;
		}
		#endregion

		private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
		{
			if (_Server != null && e.KeyCode == Keys.Enter)
			{
				foreach (EndPoint ep in _Server.GetAllPoints())
				{
					if (!_Server[ep].Connected)
						continue;
					if (chkHexString.Checked)
						_Server.SendData(ep, txtSendMsg.Text.ToByteArray());
					else
						_Server.SendData(ep, Encoding.UTF8.GetBytes(txtSendMsg.Text));
				}
				txtSendMsg.SelectAll();
				e.SuppressKeyPress = true;
				e.Handled = false;
			}
		}
	}
}
