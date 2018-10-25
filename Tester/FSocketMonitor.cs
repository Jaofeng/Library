using CJF.Net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#pragma warning disable IDE1006
namespace Tester
{
    public partial class FSocketMonitor : Form
    {
        Stopwatch sw = new Stopwatch();
        SocketMonitor _Svr = null;
        Task _Monitor = null;
        bool _Exit = false;

        #region Public Static Method : IPAddress[] GetHostIP()
        /// <summary>取得本機IP</summary>
        /// <returns></returns>
        public static IPAddress[] GetHostIP()
        {
            IPHostEntry ipEntry = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress[] addrs = ipEntry.AddressList;
            List<IPAddress> ipAddr = new List<IPAddress>();
            foreach (IPAddress ipa in addrs)
            {
                if (ipa.IsIPv6LinkLocal || ipa.AddressFamily.HasFlag(System.Net.Sockets.AddressFamily.InterNetworkV6))
                    continue;
                ipAddr.Add(ipa);
            }
            return ipAddr.ToArray();
        }
        #endregion

        public FSocketMonitor()
        {
            InitializeComponent();
            IPAddress[] ips = GetHostIP();
            cbIP.Items.Clear();
            foreach (IPAddress ipa in ips)
                cbIP.Items.Add(ipa.ToString());
            cbIP.SelectedIndex = 0;
        }

        private void btnExec_Click(object sender, EventArgs e)
        {
            if (btnExec.Text.Equals("Start"))
            {
                _Exit = false;
                btnExec.Text = "Stop";
                cbFillter.Items.Clear();
                cbFillter.Items.Add("<None>");
                cbFillter.SelectedIndex = 0;
                cbProtocol.Items.Clear();
                cbProtocol.Items.Add("<None>");
                cbProtocol.SelectedIndex = 0;
                lvSkt.View = View.Details;
                lvSkt.Items.Clear();
                sw.Restart();
                _Svr = new SocketMonitor(cbIP.Text);
                //_Svr.Start();
                _Monitor = Task.Factory.StartNew(MonitorTask);
            }
            else
            {
                _Exit = true;
                _Monitor.Wait(2000);
                try { _Monitor.Dispose(); }
                catch { }
                _Monitor = null;
                //_Svr.Shutdown();
                sw.Stop();
                btnExec.Text = "Start";
            }
        }

        private void InsertPackage(AnalyzePackage package)
        {
            if (this.InvokeRequired)
            {
                try { this.Invoke(new MethodInvoker(() => InsertPackage(package))); }
                catch (ObjectDisposedException) { }
            }
            else
            {
                bool showIt = false;
                if (cbFillter.SelectedIndex == 0 && cbProtocol.SelectedIndex == 0)
                    showIt = true;
                else if (cbFillter.SelectedIndex != 0 && cbProtocol.SelectedIndex == 0 && (package.SrcIP.ToString().Equals(cbFillter.Text) || package.DesIP.ToString().Equals(cbFillter.Text)))
                    showIt = true;
                else if (cbFillter.SelectedIndex == 0 && cbProtocol.SelectedIndex != 0 && package.Protocol.ToString().Equals(cbProtocol.Text))
                    showIt = true;
                else if (cbFillter.SelectedIndex != 0 && cbProtocol.SelectedIndex != 0 && (package.SrcIP.ToString().Equals(cbFillter.Text) || package.DesIP.ToString().Equals(cbFillter.Text)) && package.Protocol.ToString().Equals(cbProtocol.Text))
                    showIt = true;
                if (showIt)
                {
                    ListViewItem lvi = new ListViewItem((sw.ElapsedMilliseconds / 1000F).ToString());
                    lvi.SubItems.Add(package.SrcIP.ToString());
                    lvi.SubItems.Add(package.DesIP.ToString());
                    lvi.SubItems.Add(package.Protocol.ToString());
                    lvi.SubItems.Add("0");
                    lvSkt.Items.Insert(0, lvi);
                }
                if (!cbFillter.Items.Contains(package.SrcIP.ToString()))
                    cbFillter.Items.Add(package.SrcIP.ToString());
                else if (!cbFillter.Items.Contains(package.DesIP.ToString()))
                    cbFillter.Items.Add(package.DesIP.ToString());
                if (!cbProtocol.Items.Contains(package.Protocol.ToString()))
                    cbProtocol.Items.Add(package.Protocol.ToString());
                if (lvSkt.Items.Count >= 100)
                    lvSkt.Items.RemoveAt(lvSkt.Items.Count - 1);
                Application.DoEvents();
            }
        }

        private void MonitorTask()
        {
            AnalyzePackage ap = null;
            while (!_Exit)
            {
                if (_Svr.PackageCount == 0) continue;
                while (!_Exit && _Svr.PackageCount != 0)
                {
                    ap = _Svr.GetPackage();
                    InsertPackage(ap);
                    Thread.Sleep(20);
                }
            }
        }

        private void cbFillter_SelectedIndexChanged(object sender, EventArgs e)
        {
            lvSkt.Items.Clear();
        }
    }
}
