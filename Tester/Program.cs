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
			MainEntry main = new MainEntry();
			Application.Run(main);
		}
	}
}
