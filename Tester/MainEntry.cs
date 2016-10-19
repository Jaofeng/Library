using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CJF.Utility;

namespace Tester
{
	public partial class MainEntry : Form
	{
		public MainEntry()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			FAsyncServer fas = new FAsyncServer();
			fas.Show();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			FAsyncUdpServer fas = new FAsyncUdpServer();
			fas.Show();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			FAsyncClient fas = new FAsyncClient();
			fas.Show();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			FMulticast fmc = new FMulticast();
			fmc.Show();
		}

		private void button5_Click(object sender, EventArgs e)
		{
			FPing fp = new FPing();
			fp.Show();
			
		}

		private void button6_Click(object sender, EventArgs e)
		{
			FThreadSafeList fp = new FThreadSafeList();
			fp.Show();
		}

		private void btnUnitConv_Click(object sender, EventArgs e)
		{
			MessageBox.Show(ConvUtils.ConvertUnit(Convert.ToInt64(txtConvUnit.Text)));
		}

		private void btnGetBytes_Click(object sender, EventArgs e)
		{
			switch (cbDateType.SelectedItem.ToString())
			{
				case "Short":
					txtGetBytesResult.Text = ConvUtils.Byte2HexString(ConvUtils.GetBytes(short.Parse(txtGetBytes.Text), chkBigEndian.Checked));
					break;
				case "Int":
					txtGetBytesResult.Text = ConvUtils.Byte2HexString(ConvUtils.GetBytes(int.Parse(txtGetBytes.Text), chkBigEndian.Checked));
					break;
				case "Long":
					txtGetBytesResult.Text = ConvUtils.Byte2HexString(ConvUtils.GetBytes(long.Parse(txtGetBytes.Text), chkBigEndian.Checked));
					break;
				case "Double":
					txtGetBytesResult.Text = ConvUtils.Byte2HexString(ConvUtils.GetBytes(double.Parse(txtGetBytes.Text), chkBigEndian.Checked));
					break;
				case "Float":
					txtGetBytesResult.Text = ConvUtils.Byte2HexString(ConvUtils.GetBytes(float.Parse(txtGetBytes.Text), chkBigEndian.Checked));
					break;
				default:
					MessageBox.Show("No Support");
					break;
			}
		}

		private void button7_Click(object sender, EventArgs e)
		{
			listBox1.Items.Clear();
			foreach (string s in AnsiString.AllKeyWords)
				listBox1.Items.Add(s);
		}

		private void button8_Click(object sender, EventArgs e)
		{
			FActivePorts fp = new FActivePorts();
			fp.Show();
		}

		private void button9_Click(object sender, EventArgs e)
		{
			FTelnetServer fp = new FTelnetServer();
			fp.Show();
		}
	}
}
