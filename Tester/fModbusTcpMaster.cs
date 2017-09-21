using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using CJF.Utility;
using CJF.Modbus;

namespace Tester
{
	public partial class fModbusTcpMaster : Form
	{
		const int DEF_PORT = 502;
		const ushort DEF_DEVICE_ID = 1;
		const ushort DEF_PLC_ADDRESS = 1;	// PLC Address = Protocol Address + 1
		const ushort DEF_CYCLE = 1000;
		const ushort DEF_LENGTH = 1;

		TcpMaster _Master = null;
		bool _NetworkIsOK = false;
		DateTime dtDisconnect = new DateTime();
		DateTime dtNow = new DateTime();

		#region static extern bool InternetGetConnectedState
		[DllImport("WININET", CharSet = CharSet.Auto)]
		static extern bool InternetGetConnectedState(ref InternetConnectionState lpdwFlags, int dwReserved);
		enum InternetConnectionState : int
		{
			INTERNET_CONNECTION_MODEM = 0x1,
			INTERNET_CONNECTION_LAN = 0x2,
			INTERNET_CONNECTION_PROXY = 0x4,
			INTERNET_RAS_INSTALLED = 0x10,
			INTERNET_CONNECTION_OFFLINE = 0x20,
			INTERNET_CONNECTION_CONFIGURED = 0x40
		}
		#endregion

		#region Construct Method : MainForm()
		public fModbusTcpMaster()
		{
			InitializeComponent();
			this.Text = string.Format("{0} - v{1}", Application.ProductName, Application.ProductVersion);
			LogManager.WriteLog(this.Text);
			txtRemoteIP.Text = GetHostIP();
			txtPort.Text = DEF_PORT.ToString();
			txtRegister.Text = DEF_PLC_ADDRESS.ToString();
			ndCycle.Value = DEF_CYCLE;
			txtLen.Text = DEF_LENGTH.ToString();
			dgData.Columns["dcAddress"].ValueType = typeof(ushort);
			dgData.Columns["dcDecimal"].ValueType = typeof(ushort);
			dgData.Columns["dcHex"].ValueType = typeof(string);
			dgData.Tag = "word";
		}
		#endregion

		#region Private Method : void btnStart_Click(object sender, EventArgs e)
		private void btnStart_Click(object sender, EventArgs e)
		{
			try
			{
				Regex reg = new Regex(@"^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
				if (!reg.IsMatch(txtRemoteIP.Text))
				{
					MessageBox.Show("Invalid IP!!!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				ushort port = 0;
				if (!ushort.TryParse(txtPort.Text, out port))
				{
					MessageBox.Show("Invalid Port!!!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				byte sId = 0;
				ushort addr = 0;
				if (!ushort.TryParse(txtRegister.Text, out addr) || addr < 1 || addr > 65535)
				{
					MessageBox.Show("Invalid Register!!!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
				}
				tmrRefrash.Interval = (int)ndCycle.Value;
				_NetworkIsOK = Connect();
				tmrRefrash.Enabled = true;
				btnStart.Enabled = false;
				btnStop.Enabled = true;
				gbConnect.Enabled = false;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		#endregion

		#region Private Method : void tmrRefrash_Tick(object sender, EventArgs e)
		private void tmrRefrash_Tick(object sender, EventArgs e)
		{
			if (cbPause.Checked) return;
			RequestData();
		}
		#endregion

		#region Private Method : void btnRequest_Click(object sender, EventArgs e)
		private void btnRequest_Click(object sender, EventArgs e)
		{
			RequestData();
			if (btnStart.Enabled)
				Disconnect();
		}
		#endregion

		#region Private Method : void btnStop_Click(object sender, EventArgs e)
		private void btnStop_Click(object sender, EventArgs e)
		{
			Disconnect();
			btnStop.Enabled = false;
			btnStart.Enabled = true;
			gbConnect.Enabled = true;
		}
		#endregion

		#region Private Method : dgData_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		private void dgData_CurrentCellDirtyStateChanged(object sender, EventArgs e)
		{
			if (!cbPause.Checked) return;
			dgData.CommitEdit(DataGridViewDataErrorContexts.Commit);
			ushort addr = Convert.ToUInt16(dgData.Rows[dgData.CurrentCell.RowIndex].Cells["dcAddress"].Value);
			addr--;
			try
			{
				if (rbFC01.Checked)
					_Master.WriteSingleCoil(addr, (bool)dgData.CurrentCell.Value);
			}
			catch (Exception ex)
			{
				LogManager.LogException(ex);
				if (ex.Source.Equals("nModbusPC"))
				{
					string[] s = ex.Message.Replace("\r", "").Split('\n');
					if (s.Length >= 3)
						MessageBox.Show("Slave Response : " + s[1] + ", " + s[2].Split('-')[0], Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					else
						MessageBox.Show("Slave Response : \n" + ex.Message);
				}
				else
					MessageBox.Show(ex.Message);
			}
		}
		#endregion

		#region Private Method : void ndCycle_ValueChanged(object sender, EventArgs e)
		private void ndCycle_ValueChanged(object sender, EventArgs e)
		{
			tmrRefrash.Interval = (int)ndCycle.Value;
		}
		#endregion

		#region Private Method : string GetHostIP()
		/// <summary>取得本機IP</summary>
		/// <returns></returns>
		private string GetHostIP()
		{
			IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress[] addrs = ipEntry.AddressList;
			IPAddress ipAddr = null;
			foreach (IPAddress ipa in addrs)
			{
				if (ipa.IsIPv6LinkLocal || ipa.AddressFamily.HasFlag(AddressFamily.InterNetworkV6))
					continue;
				ipAddr = ipa;
			}
			return ipAddr.ToString();
		}
		#endregion

		#region Private Method : bool CheckInternet()
		/// <summary>檢查網路連線</summary>
		/// <returns></returns>
		private bool CheckInternet()
		{
			// http://msdn.microsoft.com/en-us/library/windows/desktop/aa384702(v=vs.85).aspx
			InternetConnectionState flag = InternetConnectionState.INTERNET_CONNECTION_LAN;
			return InternetGetConnectedState(ref flag, 0);
		}
		#endregion

		#region Private Method : bool Connect()
		private bool Connect()
		{
			if (_Master != null) _Master.Dispose();
			if (CheckInternet())
			{
				try
				{
					// Create Modbus TCP Master
					_Master = new TcpMaster(txtRemoteIP.Text, int.Parse(txtPort.Text));
					_Master.Retries = 0; // 不需要重試
					_Master.ReadTimeout = 1500;
					LogManager.WriteLog("Cannot to Remote Modbus Device({0}:{1})", txtRemoteIP.Text, txtPort.Text);
					return true;
				}
				catch (Exception ex)
				{
					LogManager.LogException(ex);
					return false;
				}
			}
			return false;
		}
		#endregion

		#region Private Method : void Disconnect()
		private void Disconnect()
		{
			try
			{
				this._NetworkIsOK = false;
				tmrRefrash.Enabled = false;
				if (_Master != null)
					_Master.Dispose();
				_Master = null;
			}
			catch (Exception ex)
			{
				LogManager.LogException(ex);
			}
		}
		#endregion

		#region Private Method : void SetData(ushort addr, ushort[] data)
		private void SetData(ushort addr, ushort[] data)
		{
			int selectedRow = -1;
			if (dgData.Tag == null || dgData.Tag.ToString() != "word" || dgData.Rows.Count != 0 && Convert.ToUInt16(dgData.Rows[0].Cells["dcAddress"].Value) != addr + 1)
			{
				dgData.Rows.Clear();
				if (dgData.Tag == null || dgData.Tag.ToString() != "word")
				{
					dgData.Columns.Clear();
					dgData.Columns.Add("dcAddress", "Address");
					dgData.Columns["dcAddress"].Width = 80;
					dgData.Columns.Add("dcDecimal", "Decimal");
					dgData.Columns["dcDecimal"].Width = 80;
					dgData.Columns.Add("dcHex", "Hex");
					dgData.Columns["dcHex"].Width = 80;
				}
			}
			if (dgData.Rows.Count != 0 && dgData.SelectedRows.Count != 0)
				selectedRow = dgData.SelectedRows[0].Index;
			for (int i = 0; i < data.Length; i++)
			{
				if (dgData.Rows.Count <= i)
					dgData.Rows.Add(addr + 1 + i, data[i], data[i].ToString("X4"));
				else if (dgData.Rows[i].Cells["dcAddress"].Value.Equals(addr + 1 + i))
				{
					dgData.Rows[i].Cells["dcDecimal"].Value = data[i];
					dgData.Rows[i].Cells["dcHex"].Value = data[i].ToString("X4");
				}
			}
			if (dgData.Rows.Count > data.Length)
			{
				for (int i = data.Length; i < dgData.Rows.Count; i++)
					dgData.Rows.RemoveAt(data.Length);
			}
			if (selectedRow != -1)
				dgData.Rows[selectedRow].Selected = true;
			dgData.Tag = "word";
			labUpdateTime.Text = string.Format("Last Update : {0:yyyy/MM/dd HH\\:mm\\:ss.fff}", DateTime.Now);
		}
		#endregion

		#region Private Method : void SetData(ushort addr, bool[] data)
		private void SetData(ushort addr, bool[] data)
		{
			int selectedRow = -1;
			if (dgData.Tag == null || dgData.Tag.ToString() != "bit" || dgData.Rows.Count != 0 && Convert.ToUInt16(dgData.Rows[0].Cells["dcAddress"].Value) != addr + 1)
			{
				dgData.Rows.Clear();
				if (dgData.Tag == null || dgData.Tag.ToString() != "bit")
				{
					dgData.Columns.Clear();
					dgData.Columns.Add("dcAddress", "Address");
					dgData.Columns["dcAddress"].Width = 80;
					DataGridViewCheckBoxColumn dc = new DataGridViewCheckBoxColumn();
					dc.Name = "dcBit";
					dc.HeaderText = "Bit";
					dc.Width = 40;
					dc.Resizable = DataGridViewTriState.False;
					dgData.Columns.Add(dc);
				}
			}
			dgData.Columns["dcBit"].ReadOnly = rbFC02.Checked;
			if (dgData.Rows.Count != 0 && dgData.SelectedRows.Count != 0)
				selectedRow = dgData.SelectedRows[0].Index;
			for (int i = 0; i < data.Length; i++)
			{
				if (dgData.Rows.Count <= i)
					dgData.Rows.Add(addr + 1 + i, data[i]);
				else if (dgData.Rows[i].Cells["dcAddress"].Value.Equals(addr + 1 + i))
					dgData.Rows[i].Cells["dcBit"].Value = data[i];
			}
			if (dgData.Rows.Count > data.Length)
			{
				for (int i = data.Length; i < dgData.Rows.Count; i++)
					dgData.Rows.RemoveAt(data.Length);
			}
			if (selectedRow != -1)
				dgData.Rows[selectedRow].Selected = true;
			dgData.Tag = "bit";
			labUpdateTime.Text = string.Format("Last Update : {0:yyyy/MM/dd HH\\:mm\\:ss.fff}", DateTime.Now);
		}
		#endregion

		#region Private Method : void RequestData()
		private void RequestData()
		{
			try
			{
				#region Retry connecting
				int retry = 0;
				while (!this._NetworkIsOK && retry < 3)
				{
					dtNow = DateTime.Now;
					if (dtNow.Subtract(dtDisconnect).TotalSeconds > 10)
					{
						LogManager.WriteLog("Start connecting...");
						this._NetworkIsOK = Connect();
						if (!this._NetworkIsOK)
						{
							LogManager.WriteLog("Connecting fail. Wait for retry");
							dtDisconnect = DateTime.Now;
							retry++;
						}
					}
					Application.DoEvents();
					Thread.Sleep(50);
				}
				#endregion

				if (!this._NetworkIsOK)
				{
					MessageBox.Show("Can't connect to remote!!!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Stop);
					return;
				}

				ushort addr = 0;
				if (!ushort.TryParse(txtRegister.Text, out addr) || addr < 1)
				{
					MessageBox.Show("Invalid Register!!!", Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					txtRegister.Focus();
					txtRegister.SelectAll();
					return;
				}
				addr--;		// Convert to Protocol Address
				ushort len = ushort.Parse(txtLen.Text);
				bool[] bs = null;
				ushort[] us = null;
				if (rbFC01.Checked)
				{
					bs = _Master.ReadCoils(addr, len);
					SetData(addr, bs);
				}
				else if (rbFC02.Checked)
				{
					bs = _Master.ReadInputs(addr, len);
					SetData(addr, bs);
				}
				else if (rbFC03.Checked)
				{
					us = _Master.ReadHoldingRegisters(addr, len);
					SetData(addr, us);
				}
				else if (rbFC04.Checked)
				{
					us = _Master.ReadInputRegisters(addr, len);
					SetData(addr, us);
				}
			}
			catch (Exception ex)
			{
				bool resume = false;
				if (tmrRefrash.Enabled)
				{
					tmrRefrash.Enabled = false;
					resume = true;
				}
				LogManager.LogException(ex);
				if (ex.Source.Equals("System"))
				{
					//set NetworkIsOK to false and retry connecting
					_NetworkIsOK = false;
					Console.WriteLine(ex.Message);
					dtDisconnect = DateTime.Now;
				}
				else if (ex.Source.Equals("nModbusPC"))
				{
					string[] s = ex.Message.Replace("\r", "").Split('\n');
					if (s.Length >= 3)
						MessageBox.Show("Slave Response : " + s[1] + ", " + s[2].Split('-')[0], Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
					else
						MessageBox.Show("Slave Response : \n" + ex.Message);
				}
				if (resume)
					tmrRefrash.Enabled = true;
			}
		}
		#endregion

	}
}
