using System;
using System.Diagnostics;
using System.IO;
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

		private void btnSaveLog_Click(object sender, EventArgs e)
		{
			LogManager.LogLevel lv = LogManager.LogLevel.Info;
			if (!Enum.TryParse<LogManager.LogLevel>(cbLogLevel.SelectedItem.ToString(), true, out lv))
				return;
			LogManager.WriteLog(lv, txtLog.Text);
		}

		private void button11_Click(object sender, EventArgs e)
		{
			FHttpService fp = new FHttpService();
			fp.Show();
		}

		private void btnFile_Click(object sender, EventArgs e)
		{
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Filter = "所有檔案(*.*)|*.*";
				ofd.RestoreDirectory = true;
				ofd.AutoUpgradeEnabled = true;
				ofd.CheckFileExists = true;
				ofd.CheckPathExists = true;
				ofd.Multiselect = false;
				ofd.DefaultExt = ".zip";
				ofd.Title = "請選擇欲搜尋內容的檔案";
				if (string.IsNullOrEmpty(txtFile.Text))
					ofd.FileName = string.Empty;
				else
					ofd.FileName = Path.GetFileName(txtFile.Text);
				DialogResult dr = ofd.ShowDialog(this);
				if (dr != DialogResult.OK)
					return;
				txtFile.Text = ofd.FileName;
			}
		}

		private void btnSearchHex_Click(object sender, EventArgs e)
		{
			if (!File.Exists(txtFile.Text))
			{
				MessageBox.Show("檔案不存在!!");
				return;
			}
			if (string.IsNullOrEmpty(txtHexStr.Text))
			{
				MessageBox.Show("請輸入欲搜尋的 16 進位陣列字串!!");
				return;
			}
			this.UseWaitCursor = true;
			Application.DoEvents();
			string msg = string.Empty;

			byte[] sch = ConvUtils.HexStringToBytes(txtHexStr.Text);
			int idx = 0, start = 0;
			if (btnSearchHex.Tag != null)
				start = Convert.ToInt32(btnSearchHex.Tag) + 1;
			Stopwatch w = new Stopwatch();
			w.Start();
			if (!rbFindIndexes.Checked)
			{
				if (rbIndexOfBytes.Checked)
				{
					byte[] buf = File.ReadAllBytes(txtFile.Text);
					idx = ConvUtils.IndexOfBytes(buf, sch, start);
				}
				else if (rbFindIndex.Checked)
				{
					idx = ConvUtils.IndexOfBytesInFile(txtFile.Text, sch, start);
				}
				w.Stop();
				if (idx == -1)
				{
					msg = string.Format("找不到欲搜尋的 16 進位陣列字串!!\n耗時：{0}ms / {1}ticks\n\n", w.ElapsedMilliseconds, w.ElapsedTicks);
					btnSearchHex.Tag = null;
				}
				else
				{
					msg = string.Format("欲搜尋的 16 進位陣列字串的索引值為 {0:X8}\n耗時：{1}ms / {2}ticks\n\n", idx, w.ElapsedMilliseconds, w.ElapsedTicks);
					btnSearchHex.Tag = idx;
				}
			}
			else
			{
				int[] idxs = ConvUtils.IndexesOfBytesInFile(txtFile.Text, sch);
				w.Stop();
				if (idxs.Length == 0)
				{
					msg = string.Format("找不到欲搜尋的 16 進位陣列字串!!\n耗時：{0}ms / {1}ticks\n\n", w.ElapsedMilliseconds, w.ElapsedTicks);
					btnSearchHex.Tag = null;
				}
				else
				{
					msg = string.Format("搜尋 16 進位陣列字串耗時：{0}ms / {1}ticks, 索引值為：\n", w.ElapsedMilliseconds, w.ElapsedTicks);
					foreach (int ii in idxs)
						msg += string.Format("{0:X8}\n", ii);
					btnSearchHex.Tag = null;
				}
			}
			this.UseWaitCursor = false;
			Application.DoEvents();
			MessageBox.Show(msg);
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			btnSearchHex.Tag = null;
		}

		private void rbFunc_CheckedChanged(object sender, EventArgs e)
		{
			btnSearchHex.Tag = null;
		}
	}
}
