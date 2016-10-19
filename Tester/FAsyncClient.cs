using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using CJF.Net;
using CJF.Utility;

namespace Tester
{
	public partial class FAsyncClient : Form
	{
		AsyncClient _Client = null;
		public FAsyncClient()
		{
			InitializeComponent();
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			btnStart.Enabled = false;
			_Client = new AsyncClient(txtIP.Text, Convert.ToInt32(txtPort.Text));
			try
			{
				_Client.OnConnected += new EventHandler<AsyncClientEventArgs>(Client_OnConnected);
				_Client.OnDataReceived += new EventHandler<AsyncClientEventArgs>(Client_OnDataReceived);
				_Client.OnDataSended += new EventHandler<AsyncClientEventArgs>(Client_OnDataSended);
				_Client.OnClosing += new EventHandler<AsyncClientEventArgs>(Client_OnClosing);
				_Client.OnClosed += new EventHandler<AsyncClientEventArgs>(Client_OnClosed);
				_Client.OnException += new EventHandler<AsyncClientEventArgs>(Client_OnException);
				_Client.ConnectTimeout = 3000;
				_Client.Connect();
			}
			catch (Exception ex)
			{
				WriteLog("無法連接至伺服器!!!");
				if (ex.GetType().Equals(typeof(System.Net.Sockets.SocketException)))
				{
					System.Net.Sockets.SocketException se = (System.Net.Sockets.SocketException)ex;
					WriteLog("{0} - {1}", se.SocketErrorCode, se.Message);
				}
				btnStart.Enabled = true;
			}
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

		#region Control Invoke
		delegate void SetTextBoxFocusCallback(TextBox txt);
		void SetTextBoxFocus(TextBox txt)
		{
			if (txt.InvokeRequired)
				this.Invoke(new SetTextBoxFocusCallback(SetTextBoxFocus), txt);
			else
				txt.Focus();
		}

		delegate void SetControlEnabledCallback(Control c, bool enabled);
		void SetControlEnabled(Control c, bool enabled)
		{
			if (c.InvokeRequired)
				this.Invoke(new SetControlEnabledCallback(SetControlEnabled), c, enabled);
			else
				c.Enabled = enabled;
		}
		#endregion

		void Client_OnException(object sender, AsyncClientEventArgs e)
		{
			WriteLog("發生錯誤");
			WriteLog(e.Exception.Message);
		}

		void Client_OnClosed(object sender, AsyncClientEventArgs e)
		{
			WriteLog("已與伺服器中斷連線");
			SetControlEnabled(btnStart, true);
			SetControlEnabled(btnStop, false);
		}

		void Client_OnClosing(object sender, AsyncClientEventArgs e)
		{
			WriteLog("正與伺服器中斷連線中");
		}

		void Client_OnDataSended(object sender, AsyncClientEventArgs e)
		{
			WriteLog("已發送 {0} Bytes 至伺服器", e.Data.Length);
			WriteLog(" < :{0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex:{0}", ConvUtils.Byte2HexString(e.Data));
		}

		void Client_OnDataReceived(object sender, AsyncClientEventArgs e)
		{
			WriteLog("從伺服器收到 {0} Bytes", e.Data.Length);
			WriteLog(" > :{0}", Encoding.Default.GetString(e.Data));
			WriteLog("Hex:{0}", ConvUtils.Byte2HexString(e.Data));
		}

		void Client_OnConnected(object sender, AsyncClientEventArgs e)
		{
			WriteLog("已連接至伺服器");
			SetTextBoxFocus(txtSendMsg);
			SetControlEnabled(btnStart, false);
			SetControlEnabled(btnStop, true);
		}

		private void btnStop_Click(object sender, EventArgs e)
		{
			try
			{
				_Client.Close();
				_Client.Dispose();
				_Client = null;
			}
			catch { }
			SetControlEnabled(btnStart, true);
			SetControlEnabled(btnStop, false);
		}

		private void txtSendMsg_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode != Keys.Enter)
				return;
			e.SuppressKeyPress = false;
			if (_Client == null || !_Client.Connected) return;
			if (chkHexString.Checked)
				_Client.SendData(ConvUtils.HexStringToBytes(txtSendMsg.Text));
			else
			{
				string str = txtSendMsg.Text;
				str = str.Replace("<CR>", "\r").Replace("<cr>", "\r");
				str = str.Replace("<LF>", "\n").Replace("<lf>", "\n");
				str = str.Replace("<TAB>", "\t").Replace("<tab>", "\t");
				str = str.Replace("<BS>", "\u0008").Replace("<bs>", "\u0008");
				str = str.Replace("<CTRL-Z>", "\u001A").Replace("<ctrl-z>", "\u001A");
				str = str.Replace("<ESC>", "\u001B").Replace("<esc>", "\u001B");
				_Client.SendData(str);
			}
			txtSendMsg.SelectAll();
		}
	}
}
