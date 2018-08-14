using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Net;
using System.Windows.Forms;
using CJF.Net.Routing;
using System.Reflection;

namespace Tester
{
    public partial class FRoutingTable : Form
    {
        public FRoutingTable()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int res = RouteTableManager.GetIpForwardTable(out IPForwardRow[] rows);
            dgvRows.DataSource = rows;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tvResult.Nodes.Clear();
            TreeNode tn;
            string dIP = txtDestIP.Text, sIP = txtSourceIP.Text;
            string nextHop = string.Empty;
            tvResult.Nodes.Add($"GetBestInterface() = 0x{RouteTableManager.GetBestInterface(IPAddress.Parse(dIP), out uint idx).ToString("X8")}, idx : {idx}{Environment.NewLine}");
            {
                tn = tvResult.Nodes.Add($"GetBestRoute() = 0x{RouteTableManager.GetBestRoute(IPAddress.Parse(dIP), IPAddress.Parse(sIP), out IPForwardRow row).ToString("X8")}{Environment.NewLine}");
                var pis = row.GetType().GetProperties();
                foreach (PropertyInfo pi in pis)
                {
                    tn.Nodes.Add($"  {pi.Name} = {pi.GetValue(row, null)}{Environment.NewLine}");
                }
                nextHop = row.NextHop.ToString();
            }
            {
                tn = tvResult.Nodes.Add($"GetIpInterfaceEntry() = 0x{RouteTableManager.GetIpInterfaceEntry(idx, out RouteTableManager.MIB_IPINTERFACE_ROW row).ToString("X8")}{Environment.NewLine}");
                var fis = row.GetType().GetFields();
                foreach (var fi in fis)
                {
                    tn.Nodes.Add($"  {fi.Name} = {fi.GetValue(row)}{Environment.NewLine}");
                }
            }
            tvResult.ExpandAll();
        }
    }
}
