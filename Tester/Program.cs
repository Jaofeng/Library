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
            if (Environment.OSVersion.Version.Major >= 6)
                SetProcessDPIAware();
            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

            MessageBox.Show("Custom MessageBox Demo...", Application.ProductName, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

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
					default:
						break;
				}
				if (f != null)
					Application.Run(f);
				else
					Application.Run(new MainEntry(0));
			}
		}
	}
}
