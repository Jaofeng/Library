using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tester
{
	static class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (args == null || args.Length == 0)
			{
				MainEntry main = new MainEntry();
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
					default:
						break;
				}
				if (f != null)
					Application.Run(f);
			}
		}
	}
}
