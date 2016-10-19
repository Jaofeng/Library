using System;
using System.Windows.Forms;
using CJF.Utility.ThreadSafe;

namespace Tester
{
	public partial class FThreadSafeList : Form
	{
		public FThreadSafeList()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ThreadSafeList<string> list = new ThreadSafeList<string>();
			Random rnd = new Random(DateTime.Now.Millisecond);
			for (int i = 0; i < 10; i++)
				list.Add(rnd.Next(1, 20).ToString());
			string[] aa = { "A1", "B2", "C3", "D4" };
			list.AddRange(aa);
			MessageBox.Show("OK");
			list = new ThreadSafeList<string>(aa);
			list.RemoveAll(s => { int zz; return !int.TryParse(s, out zz); });
			ThreadSafeCollection<string> cl = new ThreadSafeCollection<string>();
			
		}
	}
}
