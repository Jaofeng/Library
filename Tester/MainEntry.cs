using System;
using System.Text;
using System.Windows.Forms;
using CJF.Utility;
using CJF.Utility.CRC;

namespace Tester
{
	public partial class MainEntry : Form
	{
		public MainEntry()
		{
			InitializeComponent();
			cbDateType.SelectedIndex = 0;
			cbLogLevel.SelectedIndex = 0;
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
			labConvResult.Text = ConvUtils.ConvertUnit(Convert.ToInt64(txtConvUnit.Text));
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

		private void btnCRC16_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			Encoding enc = Encoding.GetEncoding(950);
			byte[] source = enc.GetBytes(txtCrcSource.Text);
			watch.Start();
			Crc16 normal = new Crc16();
			labCrc16ResN.Text = normal.ComputeChecksum(source).ToString("X");
			labCrc16ByteN.Text = ConvUtils.Byte2HexString(normal.ComputeChecksumBytes(source));
			watch.Stop();
			labCrc16TimeN.Text = watch.ElapsedTicks.ToString() + " Ticks";
			watch.Restart();
			Crc16Ccitt ccitt = new Crc16Ccitt(InitialCrcValue.Zeros);
			labCrc16ResC.Text = ccitt.ComputeChecksum(source).ToString("X");
			labCrc16ByteC.Text = ConvUtils.Byte2HexString(ccitt.ComputeChecksumBytes(source));
			watch.Stop();
			labCrc16TimeC.Text = watch.ElapsedTicks.ToString() + " Ticks";
			watch.Restart();
			Crc16Table table = new Crc16Table(InitialCrcValue.Zeros);
			labCrc16ResT.Text = table.ComputeChecksum(source).ToString("X");
			labCrc16ByteT.Text = ConvUtils.Byte2HexString(table.ComputeChecksumBytes(source));
			watch.Stop();
			labCrc16TimeT.Text = watch.ElapsedTicks.ToString() + " Ticks";
		}

		private void button10_Click(object sender, EventArgs e)
		{
			LogManager.LogLevel lv = LogManager.LogLevel.Info;
			if (!Enum.TryParse<LogManager.LogLevel>(cbLogLevel.SelectedItem.ToString(),true, out lv))
				return;
			LogManager.WriteLog(lv, txtLog.Text);
		}

		private void button11_Click(object sender, EventArgs e)
		{
			FHttpService fp = new FHttpService();
			fp.Show();
		}
	}
}
