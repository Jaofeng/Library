using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using MessageBox = CJF.Utility.WinKits.MessageBox;

namespace Tester
{
    static class Program
    {
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        [STAThread]
        static void Main(string[] args)
        {
            //if (Environment.OSVersion.Version.Major >= 6)
            //    SetProcessDPIAware();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            object[] os = CalculatePi(100);
            //MessageBox.Show($"PI = {os[0]}{Environment.NewLine}Time = {os[1]}ms");
            //return;
            //MessageBox.Show("Custom MessageBox Demo...", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (args == null || args.Length == 0)
            {
                MainEntry main = new MainEntry(0);
                Application.Run(main);
            }
            else
            {
                Form f = null;
                switch (args[0].ToLower())
                {
                    case "-as":
                        if (args.Length >= 3)
                            f = new FAsyncServer(args[1], int.Parse(args[2]));
                        else
                            f = new FAsyncServer();
                        break;
                    case "-tab":
                        int idx = 0;
                        if (args.Length >= 2 && int.TryParse(args[1], out idx))
                            f = new MainEntry(idx);
                        break;
                    case "-ssl":
                        Application.Run(new MyApplicationContext(new FSslTcpServer(), new FSslTcpClient()));
                        return;
                    case "-http":
                        f = new FHttpService();
                        break;
                    case "-routing":
                        f = new FRoutingTable();
                        break;
                    case "-skt":
                        f = new FSocketMonitor();
                        break;
                    default:
                        break;
                }
                if (f != null)
                    Application.Run(f);
                else
                    Application.Run(new MainEntry(0));
            }
        }

        static object[] CalculatePi(int digits)
        {
            DateTime startTime = DateTime.Now;
            string result = "";
            digits++;

            uint[] x = new uint[digits * 3 + 2];
            uint[] r = new uint[digits * 3 + 2];

            for (int j = 0; j < x.Length; j++)
                x[j] = 20;

            for (int i = 0; i < digits; i++)
            {
                uint carry = 0;
                for (int j = 0; j < x.Length; j++)
                {
                    uint num = (uint)(x.Length - j - 1);
                    uint dem = num * 2 + 1;

                    x[j] += carry;

                    uint q = x[j] / dem;
                    r[j] = x[j] % dem;

                    carry = q * num;
                }
                if (i < digits - 1)
                    result += (x[x.Length - 1] / 10).ToString();
                if (i == 0)
                    result += ".";
                r[x.Length - 1] = x[x.Length - 1] % 10;
                for (int j = 0; j < x.Length; j++)
                    x[j] = r[j] * 10;

            }
            return new object[] { result, DateTime.Now.Subtract(startTime).TotalMilliseconds };
        }
    }
    class MyApplicationContext : ApplicationContext
    {
        int formCount = 0;
        public MyApplicationContext(params Form[] forms)
        {
            Application.ApplicationExit += (s, e) => ExitThread();
            foreach (Form f in forms)
            {
                f.FormClosed += (s, e) => { formCount--; if (formCount == 0) ExitThread(); };
                f.Show();
                formCount++;
            }
        }
    }

}
