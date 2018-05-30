using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CJF.Utility;
using CJF.Utility.CRC;
using CJF.Utility.Extensions;

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
					txtGetBytesResult.Text = short.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					txtGetBytesResult.Text += " ; " + short.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					break;
				case "Int":
					txtGetBytesResult.Text = int.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					txtGetBytesResult.Text += " ; " + int.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					break;
				case "Long":
					txtGetBytesResult.Text = long.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					txtGetBytesResult.Text += " ; " + long.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					break;
				case "UShort":
					txtGetBytesResult.Text = ushort.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					txtGetBytesResult.Text += " ; " + ushort.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					break;
				case "UInt":
					txtGetBytesResult.Text = uint.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					txtGetBytesResult.Text += " ; " + uint.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					break;
				case "ULong":
					txtGetBytesResult.Text = ulong.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
					txtGetBytesResult.Text += " ; " + ulong.Parse(txtGetBytes.Text).GetBytes(chkBigEndian.Checked).ToHexString();
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
			Crc16 crc = new Crc16();
			labCrc16Res.Text = Crc16.Compute(source).ToString("X");
			labCrc16Byte.Text = crc.ComputeHash(source).ToHexString();
			watch.Stop();
			labCrc16Time.Text = watch.ElapsedTicks.ToString() + " Ticks";
			watch.Start();
			Crc16 modbus = new Crc16(Crc16.DefaultPolynomial, Crc16.ModbusSeed);
			labCrc16ResM.Text = Crc16.Compute(source, Crc16.DefaultPolynomial, Crc16.ModbusSeed).ToString("X");
			labCrc16ByteM.Text = modbus.ComputeHash(source).ToHexString();
			watch.Stop();
			labCrc16TimeM.Text = watch.ElapsedTicks.ToString() + " Ticks";
			watch.Start();
			Crc32 crc32 = new Crc32();
			labCrc32Res.Text = Crc32.Compute(source).ToString("X");
			labCrc32Byte.Text = crc32.ComputeHash(source).ToHexString();
			watch.Stop();
			labCrc32Time.Text = watch.ElapsedTicks.ToString() + " Ticks";
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
			Button btn = (Button)sender;
			if (btn.Tag == null)
			{
				MessageBox.Show("本按鈕未設定對應之文字框!");
				return;
			}
			Control[] cs = btn.Parent.Controls.Find(btn.Tag.ToString(), true);
			if (cs == null || cs.Length == 0)
				return;
			using (OpenFileDialog ofd = new OpenFileDialog())
			{
				ofd.Filter = "所有檔案(*.*)|*.*";
				ofd.RestoreDirectory = true;
				ofd.AutoUpgradeEnabled = true;
				ofd.CheckFileExists = true;
				ofd.CheckPathExists = true;
				ofd.Multiselect = false;
				ofd.Title = "請選擇檔案";
				if (string.IsNullOrEmpty(txtFile.Text))
					ofd.FileName = string.Empty;
				else
					ofd.FileName = Path.GetFileName(txtFile.Text);
				DialogResult dr = ofd.ShowDialog(this);
				if (dr != DialogResult.OK)
					return;
				cs[0].Text = ofd.FileName;
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

			byte[] sch = txtHexStr.Text.ToByteArray();
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
					idx = buf.IndexOfBytes(sch, start);
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

		private void btnEncrypt_Click(object sender, EventArgs e)
		{
			txtTarget.Text = Security.Encrypt(txtSource.Text, txtKey.Text, txtIV.Text);
		}

		private void btnDecrypt_Click(object sender, EventArgs e)
		{
			txtTarget.Text = Security.Decrypt(txtSource.Text, txtKey.Text, txtIV.Text);
		}

		private void button10_Click(object sender, EventArgs e)
		{
			fModbusTcpMaster fp = new fModbusTcpMaster();
			fp.Show();
		}

		private void button12_Click(object sender, EventArgs e)
		{
			FWebSocketServer fp = new FWebSocketServer();
			fp.Show();
		}

		private void btnXor_Click(object sender, EventArgs e)
		{
			byte[] code = txtXor.Text.ToByteArray();
			if (code.Length == 1)
				txtXorResult.Text = txtXorSource.Text.ToByteArray().Xor(code[0]).ToHexString();
			else
				txtXorResult.Text = txtXorSource.Text.ToByteArray().Xor(code).ToHexString();
		}

		private void btnCrcFile_Click(object sender, EventArgs e)
		{
			btnFile_Click(btnCrcFile, new EventArgs());
			Application.DoEvents();
			int times = 1000;
			byte[] bs = File.ReadAllBytes(txtCrcFile.Text);
			FileStream fs = File.OpenRead(txtCrcFile.Text);
			MemoryStream ms = new MemoryStream(bs);
			byte[] res = null;
			System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
			Crc16 crc16 = Crc16.Create();
			watch.Start();
			for (int i = 0; i < times; i++)
				res = crc16.ComputeHash(bs);
			watch.Stop();
			lab16B.Text = res.ToHexString("") + ">" + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc16.ComputeHash(fs);
			watch.Stop();
			lab16S.Text = res.ToHexString("") + ">" + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc16.ComputeHash(ms);
			watch.Stop();
			lab16M.Text = res.ToHexString("") + ">" + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			Crc32 crc32 = Crc32.Create();
			watch.Start();
			for (int i = 0; i < times; i++)
				res = crc32.ComputeHash(bs);
			watch.Stop();
			lab32B.Text = res.ToHexString("") + ">" + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc32.ComputeHash(fs);
			watch.Stop();
			lab32S.Text = res.ToHexString("") + ">" + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc32.ComputeHash(ms);
			watch.Stop();
			lab32M.Text = res.ToHexString("") + ">" + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
		}
	}
}
