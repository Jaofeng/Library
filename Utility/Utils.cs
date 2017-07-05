using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Runtime.CompilerServices;

namespace CJF.Utility
{
	/// <summary>
	/// 轉換公用程式
	/// </summary>
	public class ConvUtils
	{
		#region Public Static Method : string Byte2HexString(byte[] arr)
		/// <summary>
		/// 將位元組陣列轉換成16進位字串
		/// </summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <returns>16進位字串</returns>
		public static string Byte2HexString(byte[] arr)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte c in arr)
				sb.AppendFormat("{0:X2} ", c);
			return sb.ToString();
		}
		#endregion

		#region Public Static Method : string Byte2HexString(byte[] arr, int count)
		/// <summary>
		/// 將位元組陣列轉換成16進位字串
		/// </summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="count">欲轉換的長度</param>
		/// <returns>16進位字串</returns>
		public static string Byte2HexString(byte[] arr, int count)
		{
			if (arr.Length < count)
				throw new ArgumentException("count 參數值不得大於 arr 陣列長度!");
			byte[] tmp = new byte[count];
			Array.Copy(arr, tmp, count);
			return Byte2HexString(tmp);
		}
		#endregion

		#region Public Static Method : string Byte2HexString(byte[] arr, int start, int count)
		/// <summary>
		/// 將位元組陣列轉換成16進位字串
		/// </summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="start">開始位置</param>
		/// <param name="count">欲轉換的長度</param>
		/// <returns>16進位字串</returns>
		public static string Byte2HexString(byte[] arr, int start, int count)
		{
			if (start < 0 || start >= arr.Length)
				throw new ArgumentException("start 參數值必須大於等於 0 且小於 arr 陣列長度!");
			if (arr.Length < count)
				throw new ArgumentException("count 參數值不得大於 arr 陣列長度!");
			if (start + count > arr.Length)
				throw new ArgumentException("start 與 count 總和值不得大於 arr 陣列長度!");
			byte[] tmp = new byte[count];
			Array.Copy(arr, start, tmp, 0, count);
			return Byte2HexString(tmp);
		}
		#endregion

		#region Public Static Method : string FormatHexString(string hex)
		/// <summary>將16進位字串轉換成實際的字串值</summary>
		/// <param name="hex">16進位字串</param>
		/// <returns>字串值</returns>
		public static string FormatHexString(string hex)
		{
			string outStr = string.Empty;
			string nHex = hex;
			if (nHex.IndexOf(' ') == -1)
			{
				if (nHex.Length % 2 != 0)
					nHex = "0" + nHex;
				int count = nHex.Length / 2;
				for (int i = 1; i < count; i++)
					nHex = nHex.Insert(i * 3 - 1, " ");
			}
			else
				nHex = hex;
			for (int i = 0; i < nHex.Length; i += 24)
			{
				if (i + 24 < nHex.Length)
					outStr += nHex.Substring(i, 24);
				else
					outStr += nHex.Substring(i);
				if (i != 0 && ((i / 24) % 2) == 1)
					outStr += "\n";
				else if ((i == 0 || (i % 24) == 0))
					outStr += "- ";
			}
			return outStr;
		}
		#endregion

		#region Public Static Method : byte[] HexStringToBytes(string hex)
		/// <summary>將16進位字串轉換成位元組陣列</summary>
		/// <param name="hex">16進位字串</param>
		/// <returns></returns>
		public static byte[] HexStringToBytes(string hex)
		{
			try
			{
				string ns = hex.Replace(" ", "").Replace("\n", "").Replace("\r", "");
				if (ns.Length % 2 != 0)
					ns = "0" + ns;
				byte[] bytes = new byte[ns.Length / 2];
				for (int i = 0; i < ns.Length; i += 2)
					bytes[i / 2] = Convert.ToByte(ns.Substring(i, 2), 16);
				return bytes;
			}
			catch { return null; }
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(short value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(short value, bool getBigEndian)
		{
			byte[] tmp = GetBytes((long)value, getBigEndian);
			byte[] res = new byte[2];
			if (getBigEndian)
				Array.Copy(tmp, 6, res, 0, 2);
			else
				Array.Copy(tmp, 0, res, 0, 2);
			return res;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(ushort value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(ushort value, bool getBigEndian)
		{
			byte[] tmp = GetBytes((ulong)value, getBigEndian);
			byte[] res = new byte[2];
			if (getBigEndian)
				Array.Copy(tmp, 6, res, 0, 2);
			else
				Array.Copy(tmp, 0, res, 0, 2);
			return res;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(int value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(int value, bool getBigEndian)
		{
			byte[] tmp = GetBytes((long)value, getBigEndian);
			byte[] res = new byte[4];
			if (getBigEndian)
				Array.Copy(tmp, 4, res, 0, 4);
			else
				Array.Copy(tmp, 0, res, 0, 4);
			return res;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(uint value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(uint value, bool getBigEndian)
		{
			byte[] tmp = GetBytes((ulong)value, getBigEndian);
			byte[] res = new byte[4];
			if (getBigEndian)
				Array.Copy(tmp, 4, res, 0, 4);
			else
				Array.Copy(tmp, 0, res, 0, 4);
			return res;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(long value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(long value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(ulong value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(ulong value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(double value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(double value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1d)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(float value, bool getBigEndian)
		/// <summary>
		/// 取得數值的位元組陣列
		/// </summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回LittleEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(float value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1f)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion

		#region Public Static Method : long ToInt64(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成含符號長整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static long ToInt64(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}
		#endregion

		#region Public Static Method : long ToInt64(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成含符號長整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 8 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static long ToInt64(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[8];
			Array.Copy(bytes, start, buffer, 0, 8);
			return ToInt64(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : ulong ToUInt64(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成無符號長整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static ulong ToUInt64(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToUInt64(bytes, 0);
		}
		#endregion

		#region Public Static Method : ulong ToUInt64(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成無符號長整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 8 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static ulong ToUInt64(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[8];
			Array.Copy(bytes, start, buffer, 0, 8);
			return ToUInt64(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : int ToInt32(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成含符號整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static int ToInt32(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}
		#endregion

		#region Public Static Method : int ToInt32(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成含符號整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 4 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static int ToInt32(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[4];
			Array.Copy(bytes, start, buffer, 0, 4);
			return ToInt32(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : uint ToUInt32(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成無符號整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static uint ToUInt32(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToUInt32(bytes, 0);
		}
		#endregion

		#region Public Static Method : uint ToUInt32(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成無符號整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 4 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static uint ToUInt32(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[4];
			Array.Copy(bytes, start, buffer, 0, 4);
			return ToUInt32(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : short ToInt16(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成含符號短整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static short ToInt16(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt16(bytes, 0);
		}
		#endregion

		#region Public Static Method : short ToInt16(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成含符號短整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 2 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static short ToInt16(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[2];
			Array.Copy(bytes, start, buffer, 0, 2);
			return ToInt16(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : ushort ToUInt16(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成無符號短整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static ushort ToUInt16(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToUInt16(bytes, 0);
		}
		#endregion

		#region Public Static Method : ushort ToUInt16(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成無符號短整數
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 2 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static ushort ToUInt16(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[2];
			Array.Copy(bytes, start, buffer, 0, 2);
			return ToUInt16(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : double ToDouble(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成倍精度數值
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static double ToDouble(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1d)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToDouble(bytes, 0);
		}
		#endregion

		#region Public Static Method : double ToDouble(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成倍精度數值
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 8 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static double ToDouble(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[8];
			Array.Copy(bytes, start, buffer, 0, 8);
			return ToDouble(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : float ToSingle(byte[] bytes, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成單精度數值
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static float ToSingle(byte[] bytes, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1f)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToSingle(bytes, 0);
		}
		#endregion

		#region Public Static Method : float ToSingle(byte[] bytes, int start, bool getBigEndian)
		/// <summary>
		/// 將位元組陣列轉換成單精度數值
		/// </summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 4 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		public static float ToSingle(byte[] bytes, int start, bool getBigEndian)
		{
			byte[] buffer = new byte[4];
			Array.Copy(bytes, start, buffer, 0, 4);
			return ToSingle(buffer, getBigEndian);
		}
		#endregion

		#region Public Static Method : string GetContentType(string file)
		/// <summary>
		/// 取得檔案的ContentType(MIME)
		/// </summary>
		/// <param name="file">檔案名稱</param>
		/// <returns>ContentType</returns>
		public static string GetContentType(string file)
		{
			string mime = "application/octetstream";
			string ext = System.IO.Path.GetExtension(file).ToLower();
			Microsoft.Win32.RegistryKey rk = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
			if (rk != null && rk.GetValue("Content Type") != null)
				mime = rk.GetValue("Content Type").ToString();
			return mime;
		}
		#endregion

		#region Public Static Method : string GetMD5HashFromFile(string fileName)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fileName"></param>
		/// <returns></returns>
		public static string GetMD5HashFromFile(string fileName)
		{
			System.IO.FileStream file = new System.IO.FileStream(fileName, System.IO.FileMode.Open, FileAccess.Read, FileShare.Read);
			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(file);
			file.Close();

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
				sb.Append(retVal[i].ToString("X2"));
			return sb.ToString();
		}
		#endregion

		#region Public Static Method : string Replace1ByteTo2Byte(string text)
		/// <summary>
		/// 將半形字轉為全形字
		/// </summary>
		/// <param name="text">包含半形字的字串</param>
		/// <returns>傳回全形字的字串</returns>
		public static string Replace1ByteTo2Byte(string text)
		{
			text = text.Replace('[', '〔').Replace(']', '〕').Replace(' ', '　');
			char[] chars = text.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] >= 33 && chars[i] <= 126)
					chars[i] = Convert.ToChar(chars[i] + 65248);

			}
			return new string(chars);
		}
		#endregion

		#region Public Static Method : string ConvertToNCR(string rawString)
		/// <summary>將中文難字變成NCR，如字串中包含中文難字，則傳回編碼字串</summary>
		/// <param name="rawString">原始字串</param>
		/// <returns>NCR編碼字串</returns>
		public static string ConvertToNCR(string rawString)
		{
			StringBuilder sb = new StringBuilder();
			Encoding big5 = Encoding.GetEncoding("big5");
			foreach (char c in rawString)
			{
				// 強迫轉碼成Big5，看會不會變成問號
				string cInBig5 = big5.GetString(big5.GetBytes(new char[] { c }));
				// 原來不是問號，轉碼後變問號，判定為難字
				if (c != '?' && cInBig5 == "?")
					sb.AppendFormat("&#{0};", Convert.ToInt32(c));
				else
					sb.Append(c);
			}
			return sb.ToString();
		}
		#endregion

		#region Public Static Method : byte[] Compress(byte[] buffer)
		/// <summary>
		/// 壓縮位元組資料
		/// </summary>
		/// <param name="buffer">欲壓縮的位元組資料</param>
		/// <returns>壓縮完畢之位元組資料</returns>
		public static byte[] Compress(byte[] buffer)
		{
			MemoryStream ms = new MemoryStream();
			GZipStream zip = new GZipStream(ms, CompressionMode.Compress, true);
			zip.Write(buffer, 0, buffer.Length);
			zip.Close();
			ms.Position = 0;

			MemoryStream outStream = new MemoryStream();

			byte[] compressed = new byte[ms.Length];
			ms.Read(compressed, 0, compressed.Length);

			byte[] gzBuffer = new byte[compressed.Length + 4];
			Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
			return gzBuffer;
		}
		#endregion

		#region Public Static Method : string Compress(string text)
		/// <summary>
		/// 壓縮字串
		/// </summary>
		/// <param name="text">欲壓縮的字串</param>
		/// <returns>壓縮完畢之的字串，Base64格式字串</returns>
		public static string Compress(string text)
		{
			return Compress(text, Encoding.Default);
		}
		#endregion

		#region Public Static Method : string Compress(string source, Encoding enc)
		/// <summary>
		/// 壓縮字串
		/// </summary>
		/// <param name="source">原始字串</param>
		/// <param name="enc">編碼方式</param>
		/// <returns>壓縮完畢之的字串，Base64格式字串</returns>
		public static string Compress(string source, Encoding enc)
		{
			return Convert.ToBase64String(Compress(enc.GetBytes(source)));
		}
		#endregion

		#region Public Static Method : byte[] Decompress(byte[] gzBuffer)
		/// <summary>
		/// 解壓縮位元組資料
		/// </summary>
		/// <param name="gzBuffer">欲解壓縮的位元組資料</param>
		/// <returns>解壓縮完畢之位元組資料</returns>
		public static byte[] Decompress(byte[] gzBuffer)
		{
			MemoryStream ms = new MemoryStream();
			int msgLength = BitConverter.ToInt32(gzBuffer, 0);
			ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

			byte[] buffer = new byte[msgLength];

			ms.Position = 0;
			GZipStream zip = new GZipStream(ms, CompressionMode.Decompress);
			zip.Read(buffer, 0, buffer.Length);

			return buffer;
		}
		#endregion

		#region Public Static Method : string Decompress(string compressedText)
		/// <summary>
		/// 解壓縮字串
		/// </summary>
		/// <param name="compressedText">已壓縮的字串</param>
		/// <returns>已解壓的字串</returns>
		public static string Decompress(string compressedText)
		{
			return Decompress(compressedText, Encoding.Default);
		}
		#endregion

		#region Public Static Method : string Decompress(string source, Encoding enc)
		/// <summary>
		/// 解壓縮字串
		/// </summary>
		/// <param name="source">壓縮過的Base64字串</param>
		/// <param name="enc">編碼方式</param>
		/// <returns>解壓縮後的字串</returns>
		public static string Decompress(string source, Encoding enc)
		{
			return enc.GetString(Decompress(Convert.FromBase64String(source)));
		}
		#endregion

		#region Public Static Method : int ByteArrayIndexOf(byte[] pattern, byte[] bytes, int startIndex = 0)
		/// <summary>
		/// 尋找位元組陣列所在位置
		/// </summary>
		/// <param name="source">來源陣列</param>
		/// <param name="pattern">比對陣列</param>
		/// <param name="startIndex">開始比對位置</param>
		/// <returns></returns>
		[Obsolete("請使用 IndexOfBytes(byte[] source, byte[] pattern, [int startIndex = 0])", true)]
		public static int ByteArrayIndexOf(byte[] source, byte[] pattern, int startIndex = 0)
		{
			int sLen = source.Length;
			int pLen = pattern.Length;
			byte fByte = pattern[0];
			for (int i = startIndex; i < sLen; i++)
			{
				if (fByte == source[i] && sLen - i >= pLen)
				{
					byte[] match = new byte[pLen];
					Array.Copy(source, i, match, 0, pLen);

					if (ArraySequenceEqual(match, pattern))
						return i;
				}
			}
			return -1;
		}
		#endregion

		#region Public Static Method : bool ArraySequenceEqual(byte[] source, byte[] pattern)
		/// <summary>
		/// 比對兩字元組陣列是否相同
		/// </summary>
		/// <param name="source">來源陣列</param>
		/// <param name="pattern">比對陣列</param>
		/// <returns></returns>
		public static bool ArraySequenceEqual(byte[] source, byte[] pattern)
		{
			if (source.Length != pattern.Length)
				return false;
			for (int i = 0; i < source.Length; i++)
				if (source[i] != pattern[i])
					return false;
			return true;
		}
		#endregion

		#region Public Static Method : string ConvertUnit(int length)
		/// <summary>
		/// 將數值轉換成 K,M,G,T... 度量單位，如1024=1K
		/// </summary>
		/// <param name="val">欲轉換的值</param>
		/// <returns></returns>
		public static string ConvertUnit(long val)
		{
			string bin = Convert.ToString(val, 2);
			int unit = bin.Length / 10;
			float num = (float)val / ((float)Math.Pow(1024, unit));
			if (unit > 0 && num < 0.8)
			{
				unit--;
				num *= 1024;
			}
			for (int i = 5; i <= unit; i++)
				num *= 1024;
			string sNum = num.ToString("0.0");
			switch (unit)
			{
				case 0:
					return string.Format("{0}", val);
				case 1:
					return string.Format("{0}K", sNum);
				case 2:
					return string.Format("{0}M", sNum);
				case 3:
					return string.Format("{0}G", sNum);
				default:
					{
						if (sNum.Equals("0.0"))
							return unit + "/" + unit;
						else
							return string.Format("{0}T", sNum);
					}
			}
		}
		#endregion

		#region Public Static Method : string PadRight(string source, int totalByteLength)
		/// <summary>以原始字串靠左，向右填滿空</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadRight(string source, int totalByteLength)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len >= totalByteLength) return source;
			return source.PadRight(totalByteLength - (len - source.Length));
		}
		#endregion

		#region Public Static Method : string PadRight(string source, int totalByteLength, char paddingChar)
		/// <summary>以原始字串靠左，向右填滿傳入的字元</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <param name="paddingChar">填滿的字元</param>
		/// <returns></returns>
		public static string PadRight(string source, int totalByteLength, char paddingChar)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len >= totalByteLength) return source;
			return source.PadRight(totalByteLength - (len - source.Length), paddingChar);
		}
		#endregion

		#region Public Static Method : string PadLeft(string source, int totalByteLength)
		/// <summary>以原始字串靠右，向左填滿空</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadLeft(string source, int totalByteLength)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len >= totalByteLength) return source;
			return source.PadLeft(totalByteLength - (len - source.Length));
		}
		#endregion

		#region Public Static Method : string PadLeft(string source, int totalByteLength, char paddingChar)
		/// <summary>以原始字串靠右，向左填滿傳入的字元</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <param name="paddingChar">填滿的字元</param>
		/// <returns></returns>
		public static string PadLeft(string source, int totalByteLength, char paddingChar)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len >= totalByteLength) return source;
			return source.PadLeft(totalByteLength - (len - source.Length), paddingChar);
		}
		#endregion

		#region Public Static Method : string PadRightFixLength(string source, int totalByteLength)
		/// <summary>以原始字串靠左，向右填滿固定長度的空白</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadRightFixLength(string source, int totalByteLength)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len == totalByteLength) return source;
			if (len > totalByteLength)
			{
				string result = string.Empty, tmp = string.Empty;
				char[] chars = source.ToCharArray();
				int idx = 0;
				tmp = chars[idx].ToString();
				int tmpLen = Encoding.Default.GetByteCount(tmp);
				len = Encoding.Default.GetByteCount(result);
				while (len + tmpLen <= totalByteLength)
				{
					result += tmp;
					idx++;
					tmp = chars[idx].ToString();
					tmpLen = Encoding.Default.GetByteCount(tmp);
					len = Encoding.Default.GetByteCount(result);
				}
				len = Encoding.Default.GetByteCount(result);
				if (len < totalByteLength)
					result.PadRight(totalByteLength - (len - source.Length));
				return result;
			}
			return source.PadRight(totalByteLength - (len - source.Length));
		}
		#endregion

		#region Public Static Method : string PadLeftFixLength(string source, int totalByteLength)
		/// <summary>以原始字串靠右，向左填滿固定長度的空白</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadLeftFixLength(string source, int totalByteLength)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len == totalByteLength) return source;
			if (len > totalByteLength)
			{
				string result = string.Empty, tmp = string.Empty;
				char[] chars = source.ToCharArray();
				int idx = 0;
				tmp = chars[idx].ToString();
				int tmpLen = Encoding.Default.GetByteCount(tmp);
				len = Encoding.Default.GetByteCount(result);
				while (len + tmpLen <= totalByteLength)
				{
					result += tmp;
					idx++;
					tmp = chars[idx].ToString();
					tmpLen = Encoding.Default.GetByteCount(tmp);
					len = Encoding.Default.GetByteCount(result);
				}
				len = Encoding.Default.GetByteCount(result);
				if (len < totalByteLength)
					result.PadLeft(totalByteLength - (len - source.Length));
				return result;
			}
			return source.PadLeft(totalByteLength - (len - source.Length));
		}
		#endregion

		#region Public Static Method : string PadCenter(string source, int totalByteLength)
		/// <summary>以原始字串靠中，向左右填滿空</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadCenter(string source, int totalByteLength)
		{
			int len = Encoding.Default.GetByteCount(source);
			if (len >= totalByteLength) return source;
			int num = (totalByteLength - len) / 2;
			if (num * 2 + len == totalByteLength)
				return source.PadRight(len + num).PadLeft(len + num * 2);
			else
				return source.PadRight(len + num).PadLeft(len + num * 2 + 1);
		}
		#endregion

		#region Public Static Method : Encoding GetFileEncoding(string fileName)
		/// <summary>取得文字檔的編碼方式，僅會判斷 Unicode, UTF8, Default</summary>
		/// <param name="fileName">檔案名稱</param>
		/// <returns></returns>
		public static Encoding GetFileEncoding(string fileName)
		{
			Encoding encoder = null;
			byte[] header = new byte[4];
			using (FileStream reader = File.OpenRead(fileName))
			{
				reader.Read(header, 0, 4);
				reader.Close();
			}
			if (header[0] == 0xFF && header[1] == 0xFE)
				encoder = Encoding.Unicode;				// UniCode File
			else if (header[0] == 0xFE && header[1] == 0xFF)
				encoder = Encoding.BigEndianUnicode;	// UniCode BigEndian
			else if (header[0] == 0xEF && header[1] == 0xBB && header[2] == 0xBF)
				encoder = Encoding.UTF8;				// UTF-8
			else
				encoder = Encoding.Default;				// Default
			return encoder;
		}
		#endregion

		#region Public Static Method : string GetDescription(object o)
		/// <summary>取得列舉值的說明(Description 非 Summary)</summary>
		/// <param name="o">列舉值</param>
		/// <returns></returns>
		public static string GetDescription(object o)
		{
			Type type = o.GetType();
			System.Reflection.MemberInfo[] mis = type.GetMember(o.ToString());
			object[] atts = mis[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
			return (atts.Length > 0) ? ((System.ComponentModel.DescriptionAttribute)atts[0]).Description : null;
		}
		#endregion

		#region Public Static Method : string GetPropertyDescription(object o, string memberName)
		/// <summary>取得成員的說明(DescriptionAttribute 非 Summary)</summary>
		/// <param name="o">物件類別</param>
		/// <param name="memberName">成員名稱</param>
		/// <returns></returns>
		public static string GetPropertyDescription(object o, string memberName)
		{
			Type type = o.GetType();
			System.Reflection.MemberInfo[] mis = type.GetMember(memberName);
			object[] atts = mis[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
			return (atts.Length > 0) ? ((System.ComponentModel.DescriptionAttribute)atts[0]).Description : null;
		}
		#endregion

		#region Public Static Method : bool ToBooean(string val, bool defVal)
		/// <summary>字串轉為布林值</summary>
		/// <param name="val">與轉換的字串值</param>
		/// <param name="defVal">預設值</param>
		/// <returns></returns>
		public static bool ToBooean(string val, bool defVal)
		{
			if (Array.IndexOf(new string[] { "y", "1", "true", "yes", "t" }, val.ToLower()) != -1)
				return true;
			else if (Array.IndexOf(new string[] { "n", "0", "false", "no", "f" }, val.ToLower()) != -1)
				return false;
			else
				return defVal;
		}
		#endregion

		#region Public Static Method : int IndexOfBytes(byte[] source, byte[] pattern, int startIndex = 0)
		/// <summary>尋找位元組陣列中的特定陣列值
		/// <para>請注意：此函示需要較大的記憶體空間(source陣列的大小)</para>
		/// </summary>
		/// <param name="source">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <param name="startIndex">起始位置</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位元組的位置</returns>
		public static int IndexOfBytes(byte[] source, byte[] pattern, int startIndex = 0)
		{
			if (source == null || source.Length == 0 || pattern == null || pattern.Length == 0)
				return -1;
			int idx = Array.IndexOf<byte>(source, pattern[0], startIndex);
			while (idx != -1)
			{
				if (!IsMatch(source, idx, pattern))
				{
					idx = Array.IndexOf<byte>(source, pattern[0], idx + 1);
					continue;
				}
				return idx;
			}
			return -1;
		}
		#endregion

		#region Public Static Method : int IndexOfBytesInFile(string fileName, byte[] pattern, int startIndex = 0)
		/// <summary>搜尋位元組陣列在檔案中的索引值
		/// <para>請注意：此函示比 ConvUtils.IndexOfBytes 較為節省記憶體，但每次呼叫都開啟檔案，如檔案系統較弱的儲存裝置(如CF)，不建議使用</para>
		/// </summary>
		/// <param name="fileName">被搜尋的檔案名稱，包含完整路徑</param>
		/// <param name="pattern">欲搜尋的位元組陣列</param>
		/// <param name="startIndex">開始索引值，預設值為 0</param>
		/// <returns>0 &lt;= 索引值；-1 = 沒找到</returns>
		public static int IndexOfBytesInFile(string fileName, byte[] pattern, int startIndex = 0)
		{
			if (!File.Exists(fileName))
				throw new FileNotFoundException();
			byte[] buf = new byte[64 * 1024];
			int readed = 0, idx = -1, page = 0;
			bool exists = false;
			using (FileStream fs = File.OpenRead(fileName))
			{
				// 解區段
				if (startIndex != 0)
				{
					page = startIndex / buf.Length;
					idx = startIndex % buf.Length - 1;
					fs.Seek(page * buf.Length, SeekOrigin.Begin);
				}
				// 讀取區段開始搜尋
				while ((readed = fs.Read(buf, 0, buf.Length)) > 0)
				{
					// 搜尋第一個位元組在此區段的索引值
					idx = Array.IndexOf<byte>(buf, pattern[0], idx + 1, readed - (idx + 1));
					while (idx != -1)
					{
						// 此區段中含有欲搜尋陣列的第一個位元組
						if (buf.Length - pattern.Length >= idx)
						{
							// 該索引值與搜尋長度仍此區段範圍內
							if (IsMatch(buf, idx, pattern))
								return idx + page * buf.Length;
							else
								idx = Array.IndexOf<byte>(buf, pattern[0], idx + 1, readed - (idx + 1));
						}
						else
						{
							// 該索引值與搜尋長度超過此區段範圍
							exists = true;
							int check = buf.Length - idx;
							for (int i = 0; i < check; i++)
							{
								if (!buf[idx + i].Equals(pattern[i]))
								{
									exists = false;
									break;
								}
							}
							if (exists)
							{
								// 剩餘的數量與檢查的前幾個位元組相同
								// 讀取下一區段繼續檢查
								if ((readed = fs.Read(buf, 0, buf.Length)) > 0)
								{
									// 還有下一個區段
									for (int i = 0; i < pattern.Length - check; i++)
									{
										if (!buf[i].Equals(pattern[i + check]))
										{
											exists = false;
											break;
										}
									}
									if (exists)
									{
										// 下一區段的前幾個位元組與剩餘的位元組相同
										return idx + page * buf.Length;
									}
									else
									{
										idx = Array.IndexOf<byte>(buf, pattern[0], pattern.Length - check, readed - (pattern.Length - check));
										page++;
									}
								}
								else
								{
									// 沒有下一區段
									return -1;
								}

							}
							else
								idx = -1;
						}
					}
					page++;
				}
				fs.Close();
			}
			return -1;
		}
		#endregion

		#region Public Static Method : int[] IndexesOfBytesInFile(string fileName, byte[] pattern)
		/// <summary>搜尋位元組陣列在檔案中的所有索引值位置清單
		/// <para>此函示比 ConvUtils.IndexOfBytes 較為節省記憶體，且僅於搜尋時開啟檔案</para>
		/// </summary>
		/// <param name="fileName">被搜尋的檔案名稱，包含完整路徑</param>
		/// <param name="pattern">欲搜尋的位元組陣列</param>
		/// <returns>0 &lt;= 索引值；-1 = 沒找到</returns>
		public static int[] IndexesOfBytesInFile(string fileName, byte[] pattern)
		{
			System.Collections.Generic.List<int> res = new System.Collections.Generic.List<int>();
			using (FileStream fs = File.OpenRead(fileName))
			{
				byte[] buf = new byte[64 * 1024];
				int readed = 0, idx = -1, page = 0;
				bool exists = false;
				// 讀取區段開始搜尋
				fs.Position = 0;
				while ((readed = fs.Read(buf, 0, buf.Length)) > 0)
				{
					// 搜尋第一個位元組在此區段的索引值
					idx = Array.IndexOf<byte>(buf, pattern[0], idx + 1, readed - (idx + 1));
					while (idx != -1)
					{
						// 此區段中含有欲搜尋陣列的第一個位元組
						if (buf.Length - pattern.Length >= idx)
						{
							// 該索引值與搜尋長度仍此區段範圍內
							if (IsMatch(buf, idx, pattern))
								res.Add(idx + page * buf.Length);
							idx = Array.IndexOf<byte>(buf, pattern[0], idx + 1, readed - (idx + 1));
						}
						else
						{
							// 該索引值與搜尋長度超過此區段範圍
							exists = true;
							int check = buf.Length - idx;
							for (int i = 0; i < check; i++)
							{
								if (!buf[idx + i].Equals(pattern[i]))
								{
									exists = false;
									break;
								}
							}
							if (exists)
							{
								// 剩餘的數量與檢查的前幾個位元組相同
								// 讀取下一區段繼續檢查
								if ((readed = fs.Read(buf, 0, buf.Length)) > 0)
								{
									// 還有下一個區段
									for (int i = 0; i < pattern.Length - check; i++)
									{
										if (!buf[i].Equals(pattern[i + check]))
										{
											exists = false;
											break;
										}
									}
									if (exists)
									{
										// 下一區段的前幾個位元組與剩餘的位元組相同
										res.Add(idx + page * buf.Length);
									}
									idx = Array.IndexOf<byte>(buf, pattern[0], pattern.Length - check, readed - (pattern.Length - check));
									page++;
								}
							}
							else
								idx = -1;
						}
					}
					page++;
				}
				fs.Close();
			}
			return res.ToArray();
		}
		#endregion

		#region Private Method : bool IsMatch<T>(T[] array, int position, T[] candidate)
		private static bool IsMatch<T>(T[] array, int position, T[] candidate)
		{
			if (candidate.Length > (array.Length - position))
				return false;
			for (int i = 0; i < candidate.Length; i++)
				if (!array[position + i].Equals(candidate[i]))
					return false;
			return true;
		}
		#endregion
	}
}
