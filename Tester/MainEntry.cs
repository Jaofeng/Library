using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CJF.Utility;
using CJF.Utility.CRC;
using CJF.Utility.Extensions;
using CJF.Utility.Ansi;
using AnsiLabel = CJF.Utility.WinKits.AnsiLabel;
using MessageBox = CJF.Utility.WinKits.MessageBox;

namespace Tester
{
    #pragma warning disable IDE1006
    public partial class MainEntry : Form
	{

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();		// 開啟 Console

		[DllImport("Kernel32")]
		static extern void FreeConsole();		// 釋放 Console

		private const int STD_OUTPUT_HANDLE = -11;
		private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
		private const uint DISABLE_NEWLINE_AUTO_RETURN = 0x0008;

		[DllImport("kernel32.dll")]
		private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

		[DllImport("kernel32.dll")]
		private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll")]
		public static extern uint GetLastError();

		public MainEntry(int defTab)
		{
			InitializeComponent();
			cbDateType.SelectedIndex = 0;
			cbLogLevel.SelectedIndex = 0;
			cbMsgBoxBtn.SelectedIndex = 0;
			cbMsgBoxIcon.SelectedIndex = 0;
			cbMsgBoxDefBtn.SelectedIndex = 0;
			InstalledFontCollection fc = new InstalledFontCollection();
			cbMsgBoxFont.DataSource = fc.Families;
			cbMsgBoxFont.DisplayMember = "Name";
			foreach (FontFamily ff in fc.Families)
			{
				if (ff.Name.Equals(SystemFonts.MessageBoxFont.Name))
				{
					cbMsgBoxFont.SelectedItem = ff;
					break;
				}
			}
			float[] fs = new float[] { 7, 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 28, 30, 32, 36 };
			cbMsgBoxFontSize.DataSource = fs;
			cbMsgBoxFontSize.SelectedItem = SystemFonts.MessageBoxFont.Size;
			tabControl1.SelectedIndex = defTab;

			richTextBox1.Text = "-----";
			richTextBox1.SelectionStart = richTextBox1.TextLength;
			richTextBox1.SelectionColor = Color.Red;
			richTextBox1.AppendText("1234567890");
			richTextBox1.SelectionStart = richTextBox1.TextLength;
			richTextBox1.SelectionColor = Color.Blue;
			richTextBox1.AppendText("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			richTextBox1.SelectionStart = richTextBox1.TextLength;
			richTextBox1.SelectionColor = richTextBox1.ForeColor;
			richTextBox1.AppendText("\x1B[95mABCDEFGHIJKLMNOPQRSTUVWXYZ\x1B[0m");
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
			if (string.IsNullOrEmpty(txtSource.Text))
				return;
			txtTarget.Text = Security.Encrypt(txtSource.Text, txtKey.Text, txtIV.Text);
		}

		private void btnDecrypt_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(txtSource.Text))
				return;
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
			lab16B.Text = res.ToHexString("") + " > " + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc16.ComputeHash(fs);
			watch.Stop();
			lab16S.Text = res.ToHexString("") + " > " + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc16.ComputeHash(ms);
			watch.Stop();
			lab16M.Text = res.ToHexString("") + " > " + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			Crc32 crc32 = Crc32.Create();
			watch.Start();
			for (int i = 0; i < times; i++)
				res = crc32.ComputeHash(bs);
			watch.Stop();
			lab32B.Text = res.ToHexString("") + " > " + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc32.ComputeHash(fs);
			watch.Stop();
			lab32S.Text = res.ToHexString("") + " > " + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
			watch.Restart();
			for (int i = 0; i < times; i++)
				res = crc32.ComputeHash(ms);
			watch.Stop();
			lab32M.Text = res.ToHexString("") + " > " + (watch.ElapsedTicks / times).ToString();
			Application.DoEvents();
		}

		private void btnShowMsgBox_Click(object sender, EventArgs e)
		{
			labDialogResult.Text = string.Empty;
			MessageBoxButtons btn = (MessageBoxButtons)Enum.Parse(typeof(MessageBoxButtons), cbMsgBoxBtn.Text);
			MessageBoxIcon icon = (MessageBoxIcon)Enum.Parse(typeof(MessageBoxIcon), cbMsgBoxIcon.Text);
			MessageBoxDefaultButton defBtn = (MessageBoxDefaultButton)Enum.Parse(typeof(MessageBoxDefaultButton), cbMsgBoxDefBtn.Text);
			if (chkFont.Checked)
				MessageBox.Font = new Font((FontFamily)cbMsgBoxFont.SelectedItem, (float)cbMsgBoxFontSize.SelectedItem);
			else if (MessageBox.Font != null)
				MessageBox.Font = null;
			string msg = txtMsgBoxText.Text.Replace("\\x1b", "\\x1B").Replace("\\x1B", "\x1B");
            System.Windows.Forms.MessageBox.Show(msg, "Original MessageBox", btn, icon, defBtn);
            MessageBox.Show(this, msg, "Owner MessageBox", btn, icon, defBtn);
            labDialogResult.Text = MessageBox.Show(CsiBuilder.GetPureText(msg), txtMsgBoxCaption.Text, btn, icon, defBtn).ToString();
			labDialogResult.Text = MessageBox.Show(msg, txtMsgBoxCaption.Text, btn, icon, defBtn).ToString();
        }

        private void chkFont_CheckedChanged(object sender, EventArgs e)
		{
			cbMsgBoxFont.Enabled = cbMsgBoxFontSize.Enabled = chkFont.Checked;
		}

		private void button7_Click(object sender, EventArgs e)
		{
			lstAnsi.Items.Clear();
            #pragma warning disable 0618
            foreach (string s in AnsiString.AllKeyWords)
				lstAnsi.Items.Add(s);
		}

		private void button13_Click(object sender, EventArgs e)
		{
			string ansi = AnsiString.ToANSI(txtAnsiCode.Text);
			txtAnsiStr.Tag = ansi;
			txtAnsiStr.Text = ansi.Replace("\x1B", "\\x1B");
			CsiBuilder cb1 = new CsiBuilder(ansi);
			txtCsi.Text = string.Format("Pure Text : {0}{1}{2}{1}", cb1.ToPureText(), Environment.NewLine, "-".PadLeft(50, '-'));
			txtCsi.Text += string.Format("Items Count:{0}, Length:{1}{2}", cb1.Count, cb1.Length, Environment.NewLine);
			for (int i = 0; i < cb1.Count; i++)
				txtCsi.Text += string.Format("[{0}] {1}{2}", i, cb1[i].Replace("\x1B", "\\x1B").Replace("\r", "\\r").Replace("\n", "\\n"), Environment.NewLine);
			CsiBuilder cb2 = new CsiBuilder();
			cb2.Append("\x1B[J");
			cb2.Append("123");
			cb2.Append(SgrColors.Red, "456");
			cb2.AppendCommand(CsiCommands.Cls);
			cb2.Append(SgrColors.Yellow, "789");
			cb2.AppendCommand(CsiCommands.ResetSGR);
			//var stdout = Console.OpenStandardOutput();
			//var con = new StreamWriter(stdout, Encoding.ASCII);
			//con.AutoFlush = true;
			//Console.SetOut(con);
			Console.WriteLine("\x1b[36mTEST\x1b[0m".Replace("\x1B", "\u001B"));
			Console.WriteLine("ANSIString Class : ");
			Console.WriteLine(ansi.Replace("\x1B", "\u001B"));
			Console.WriteLine("CsiBuilder Class : ");
			Console.WriteLine(cb2.ToString().Replace("\x1B", "\u001B"));
		}

		private void lstAnsi_DoubleClick(object sender, EventArgs e)
		{
			txtAnsiCode.Text += lstAnsi.SelectedItem.ToString();
		}

		private void rbMsgCsiSample_CheckedChanged(object sender, EventArgs e)
		{
			RadioButton rb = (RadioButton)sender;
			if (!rb.Checked)
				return;
			if (rb.Equals(rbMsgCsiSample1))
				txtMsgBoxText.Text = @"Message : 
Colors: \x1B[31m[CSI 31 m]Red\x1B[0m, \x1B[32m[CSI 32 m]Green\x1B[0m, \x1B[33m[CSI 33 m]Yellow\x1B[0m, \x1B[34m[CSI 34 m]Blue\x1B[0m
\x1B[7m[CSI 7 m]反相\x1B[0m, \x1B[1;91m[CSI 1;91 m]亮紅粗體\x1B[0m, \x1B[4;91m[CSI 4;91 m]亮紅底線\x1B[0m, \x1B[9;91m[CSI 9;91 m]亮紅刪除線\x1B[0m
\x1B[91m[CSI 91 m]亮紅\x1B[0m, \x1B[92m[CSI 92 m]亮綠\x1B[0m, \x1B[93m[CSI 93 m]亮黃\x1B[0m, \x1B[94m[CSI 94 m]亮藍\x1B[39;49m
\x1B[97;101m[CSI 97;101 m]亮紅底白字\x1B[0m, \x1B[97;42m[CSI 97;42 m]綠底白字\x1B[39;49m
";
			else if (rb.Equals(rbMsgCsiSample2))
				txtMsgBoxText.Text = @"Message : 
Colors \x1B[31m[CSI 31 m]Red\x1B[0m, \x1B[32m[CSI 32 m]Green\x1B[0m, \x1B[33m[CSI 33 m]Yellow\x1B[0m, \x1B[34m[CSI 34 m]Blue\x1B[0m
超長文字：一二三四五六七八九十，一二三四五六七八九十。一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十一二三四五六七八九十
超長文字：一二三四五六七八九\x1B[91m十一二三四五六七八九十\x1B[92m一二三四五六七八九十\x1B[93m一二三四五六七八九十\x1B[94m一二三四五六七八九十
一二三四五六七八九十

\x1B[1;31m[CSI 1;31 m]Bright Red\x1B[0m, \x1B[1;32m[CSI 1;32 m]Bright Green\x1B[0m, \x1B[1;33m[CSI 1;33 m]Bright Yellow\x1B[0m, \x1B[1;34m[CSI 1;34 m]Bright Blue\x1B[0m
\x1B[91m[CSI 91 m]亮紅\x1B[0m, \x1B[92m[CSI 92 m]亮綠\x1B[0m, \x1B[93m[CSI 93 m]亮黃\x1B[0m, \x1B[94m[CSI 94 m]亮藍\x1B[39;49m
";
			else if (rb.Equals(rbMsgCsiSample3))
				txtMsgBoxText.Text = @"發生錯誤！
\x1B[33m警告\x1B[0m : \x1B[31m'CJF.Utility.AnsiString' 已過時: '請改用 CJF.Utility.Ansi.CsiBuilder'\x1B[33m
D:\WorkSpace\Source\Common\Library\Tester\MainEntry.cs \x1B[94mLine:394, Colume:18 \x1B[0m@ \x1B[32mTester

";
			else if (rb.Equals(rbMsgCsiSample4))
				txtMsgBoxText.Text = "Message :\nColors: \x1B[31m[CSI 31 m]Red\x1B[0m, \x1B[32m[CSI 32 m]Green\x1B[0m, \x1B[33m[CSI 33 m]Yellow\x1B[0m, \x1B[34m[CSI 34 m]Blue\x1B[0m\n" +
					"\x1B[1;31m[CSI 1;31 m]Bright Red\x1B[0m, \x1B[1;32m[CSI 1;32 m]Bright Green\x1B[0m, \x1B[1;33m[CSI 1;33 m]Bright Yellow\x1B[0m, \x1B[1;34m[CSI 1;34 m]Bright Blue\x1B[0m\n" +
					"\x1B[91m[CSI 91 m]亮紅\x1B[0m, \x1B[92m[CSI 92 m]亮綠\x1B[0m, \x1B[93m[CSI 93 m]亮黃\x1B[0m, \x1B[94m[CSI 94 m]亮藍\x1B[39;49m\n";
			else if (rb.Equals(rbMsgCsiSample5))
			{
				txtMsgBoxText.Text = "";
				for (int i = 0; i < 16; i++)
					txtMsgBoxText.Text += string.Format("\x1B[48;5;{0}m　", i);
				txtMsgBoxText.Text += Environment.NewLine;
				for (int i = 0; i < 6; i++)
				{
					for (int j = 0; j < 36; j++)
						txtMsgBoxText.Text += string.Format("\x1B[48;5;{0}m　", 16 + i * 36 + j);
					txtMsgBoxText.Text += Environment.NewLine;
				}
				for (int i = 0; i < 24; i++)
					txtMsgBoxText.Text += string.Format("\x1B[48;5;{0}m　", i + 232);
			}
			else if (rb.Equals(rbMsgCsiSample6))
			{
				txtMsgBoxText.Text = @"24-bits Color :
\x1B[48;2;255;0;0;38;2;255;255;255m[48;2;255;0;0;38;2;255;255;255m紅底白字\x1B[39;49m\x1B[48;2;0;128;0;38;2;255;255;255m[48;2;0;128;0;38;2;255;255;255m綠底白字
\x1B[48;2;0;0;255;38;2;255;255;255m[48;2;0;0;255;38;2;255;255;255m藍底白字\x1B[39;49m\x1B[48;2;255;255;0;38;2;0;0;0m[48;2;255;255;0;38;2;0;0;0m黃底黑字
\x1B[48;5;9;38;5;15m[48;5;9;38;5;15m紅底白字\x1B[39;49m\x1B[48;5;2;38;5;15m[48;5;2;38;5;15m綠底白字
\x1B[48;5;12;38;5;15m[48;5;12;38;5;15m藍底白字\x1B[39;49m\x1B[48;5;11;38;5;0m[48;5;11;38;5;0m黃底黑字
";
			}
		}

		private void btnOpenConsole_Click(object sender, EventArgs e)
		{
			AllocConsole();
			Console.CancelKeyPress += new ConsoleCancelEventHandler(delegate(object snd, ConsoleCancelEventArgs ee)
				{
					ee.Cancel = true;
				});

			var iStdOut = GetStdHandle(STD_OUTPUT_HANDLE);
			if (!GetConsoleMode(iStdOut, out uint outConsoleMode))
			{
				Console.WriteLine("failed to get output console mode");
				Console.ReadKey();
				return;
			}

			//outConsoleMode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING | DISABLE_NEWLINE_AUTO_RETURN;
			if (!SetConsoleMode(iStdOut, outConsoleMode))
			{
				Console.WriteLine("failed to set output console mode, error code: {0}", GetLastError());
				Console.ReadKey();
				return;
			}

		}

		private void button14_Click(object sender, EventArgs e)
		{
			for (int i = 0; i < 100; i++)
			{
				ansiBox1.Append(Environment.NewLine + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff") + " " + txtAnsi.Text);
				ansiBox1.ScrollToEnd();
				Application.DoEvents();
			}
		}

		private void ansiViewer1_Click(object sender, EventArgs e)
		{

		}

		private void button15_Click(object sender, EventArgs e)
		{
			Random rnd = new Random(DateTime.Now.Millisecond);
			int rv = rnd.Next();
			ansiLabel1.Text = string.Format("\x1B[94m{0}\x1B[39m - \x1B[91m{1}\x1B[0m", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"), rv);
			Debug.Print("Label Size = {0} - {1}\n---------------", ansiLabel1.Size, rv);
		}

		private void button16_Click(object sender, EventArgs e)
		{
			if (ansiLabel1.Font.Equals(this.Font))
				ansiLabel1.Font = new Font("微軟正黑體", this.Font.Size);
			else
				ansiLabel1.Font = new Font(this.Font, this.Font.Style);
		}

		private void ansiLabel1_TextChanged(object sender, EventArgs e)
		{
			AnsiLabel lab = (AnsiLabel)sender;
			Debug.Print("{0} TextChanged : {1}", lab.Name, lab.Text.Replace("\x1B", "\\x1B"));
		}

		private void ansiLabel1_Resize(object sender, EventArgs e)
		{
			AnsiLabel lab = (AnsiLabel)sender;
			Debug.Print("{0} Resize : {1}", lab.Name, lab.Size);
		}

		private void ansiLabel1_Paint(object sender, PaintEventArgs e)
		{
			AnsiLabel lab = (AnsiLabel)sender;
			Debug.Print("{0} Paint", lab.Name);
		}

		private void button17_Click(object sender, EventArgs e)
		{
			FSslTcpServer fp = new FSslTcpServer();
			fp.Show();
		}

		private void button18_Click(object sender, EventArgs e)
		{
			FSslTcpClient fp = new FSslTcpClient();
			fp.Show();
		}
    }
}
