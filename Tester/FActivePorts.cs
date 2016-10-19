using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CJF.Net;

namespace Tester
{
	public partial class FActivePorts : Form
	{
		public FActivePorts()
		{
			InitializeComponent();
		}

		#region Private Method : void btnRefresh_Click(object sender, EventArgs e)
		private void btnRefresh_Click(object sender, EventArgs e)
		{
			SocketInfo[] sis = TcpManager.GetTable();
			dgvList.Rows.Clear();
			cbLocalIP.Items.Clear();
			cbRemoteIP.Items.Clear();
			cbStatus.Items.Clear();
			cbProcess.Items.Clear();
			List<string> lip = new List<string>();
			List<string> rip = new List<string>();
			List<string> sls = new List<string>();
			List<string> pid = new List<string>();
			string tmp = string.Empty;
			foreach (SocketInfo si in sis)
			{
				dgvList.Rows.Add(si.LocalEndPoint, (si.RemoteEndPoint.Port == 0 ? null : si.RemoteEndPoint), si.State, si.OwnerProcess.Id, si.OwnerProcess.ProcessName);
				tmp = si.LocalEndPoint.Address.ToString();
				if (!lip.Exists(s => s.Equals(tmp)))
					lip.Add(tmp);
				tmp = si.RemoteEndPoint.Address.ToString();
				if (!rip.Exists(s => s.Equals(tmp)))
					rip.Add(tmp);
				tmp = si.State.ToString();
				if (!sls.Exists(s => s.Equals(tmp)))
					sls.Add(tmp);
				tmp = string.Format("{0} : {1}", si.OwnerProcess.Id, si.OwnerProcess.ProcessName);
				if (!pid.Exists(s => s.Equals(tmp)))
					pid.Add(tmp);
			}
			lip.Sort((c1, c2) => ComparisonIP(c1, c2));
			lip.Insert(0, "--- All ---");
			cbLocalIP.Items.AddRange(lip.ToArray());
			rip.Sort((c1, c2) => ComparisonIP(c1, c2));
			rip.Insert(0, "--- All ---");
			cbRemoteIP.Items.AddRange(rip.ToArray());
			sls.Sort();
			sls.Insert(0, "--- All ---");
			cbStatus.Items.AddRange(sls.ToArray());
			pid.Sort((c1, c2) => ComparisonPID(c1, c2));
			pid.Insert(0, "--- All ---");
			cbProcess.Items.AddRange(pid.ToArray());
			cbLocalIP.SelectedIndex = cbRemoteIP.SelectedIndex = cbStatus.SelectedIndex = cbProcess.SelectedIndex = 0;
		}
		#endregion

		#region Private Method : int ComparisonIP(string ip1, string ip2)
		private int ComparisonIP(string ip1, string ip2)
		{
			string[] i1 = ip1.Split('.');
			string[] i2 = ip2.Split('.');
			int c1 = Convert.ToInt32(i1[0]).CompareTo(Convert.ToInt32(i2[0]));
			if (c1 != 0)
				return c1;
			c1 = Convert.ToInt32(i1[1]).CompareTo(Convert.ToInt32(i2[1]));
			if (c1 != 0)
				return c1;
			c1 = Convert.ToInt32(i1[2]).CompareTo(Convert.ToInt32(i2[2]));
			if (c1 != 0)
				return c1;
			return Convert.ToInt32(i1[3]).CompareTo(Convert.ToInt32(i2[3]));
		}
		#endregion

		#region Private Method : int ComparisonPID(string p1, string p2)
		private int ComparisonPID(string p1, string p2)
		{
			string[] s1 = p1.Split(':');
			string[] s2 = p2.Split(':');
			return Convert.ToInt32(s1[0]).CompareTo(Convert.ToInt32(s2[0]));
		}
		#endregion
	}
}
