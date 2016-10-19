using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using CJF.Net;

namespace Tester
{
	public partial class FPing : Form
	{
		PingTester _Tester = null;
		public FPing()
		{
			InitializeComponent();
		}

		private void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				if (_Tester != null)
					_Tester.Dispose();
				_Tester = null;
				_Tester = new PingTester(txtRemote.Text, Convert.ToInt32(txtTTL.Text), Convert.ToInt32(txtCycle.Text), Convert.ToInt32(txtTimeout.Text), Convert.ToInt32(txtDataLength.Text), Convert.ToInt32(txtTimes.Text));
				_Tester.OnResult += new EventHandler<PingResultEventArgs>(Tester_OnResult);
				_Tester.OnException += new EventHandler<PingResultEventArgs>(Tester_OnException);
				_Tester.OnFinished += new EventHandler(Tester_OnFinished);
				btnStart.Enabled = false;
				btnStop.Enabled = true;
			}
			catch
			{
				WriteLog("無法啟動!!!");
			}
		}

		void Tester_OnFinished(object sender, EventArgs e)
		{
			ControlEnable(btnStart, true);
			ControlEnable(btnStop, false);
		}

		void Tester_OnException(object sender, PingResultEventArgs e)
		{
			WriteLog("錯誤發生 : {0}", e.Exception.Message);
			WriteLog("============================");
		}

		void Tester_OnResult(object sender, PingResultEventArgs e)
		{
			WriteLog("回應狀態 : {0}", e.Status);
			WriteLog("對象主機IP : {0}", e.RemoteIP);
			WriteLog("回應時間 {0} 豪秒", e.RoundtripTime);
			WriteLog("轉送次數 : {0}", _Tester.TimeToLive - e.Ttl);
			WriteLog("============================");
		}

		#region WriteLog
		delegate void ControlEnableCallback(Control c, bool enable);
		void ControlEnable(Control c, bool enable)
		{
			if (this.InvokeRequired)
				this.Invoke(new ControlEnableCallback(ControlEnable), c, enable);
			else
				c.Enabled = enable;
		}

		delegate void WriteLogCallback(string txt);
		void WriteLog(string txt)
		{
			if (rtbConsole.InvokeRequired)
				this.Invoke(new WriteLogCallback(WriteLog), txt);
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

		private void btnStop_Click(object sender, EventArgs e)
		{
			if (_Tester != null)
				_Tester.Dispose();
			_Tester = null;
			btnStart.Enabled = true;
			btnStop.Enabled = false;
		}

	}
}
