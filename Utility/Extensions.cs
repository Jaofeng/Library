using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CJF.Utility.Extensions
{
	#region Public Static Class : StringExtension
	/// <summary>String 資料類型擴充函示</summary>
	public static class StringExtension
	{
		#region Public Static Method : byte[] ToByteArray(this string value)
		/// <summary>將 16 進位字串轉為位元組陣列</summary>
		/// <param name="value">欲轉換的 16 進位字串。</param>
		/// <returns>轉換完成的位元組陣列</returns>
		/// <exception cref="System.FormatException">value 中包含不可轉換的字元。</exception>
		public static byte[] ToByteArray(this string value)
		{
			string pattern = "(\\s|\\n|\\r|\\t)?";
			System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(pattern);
			string ns = reg.Replace(value, "");
			if (ns.Length % 2 != 0)
				ns = "0" + ns;
			byte[] bytes = new byte[ns.Length / 2];
			for (int i = 0; i < ns.Length; i += 2)
				bytes[i / 2] = Convert.ToByte(ns.Substring(i, 2), 16);
			return bytes;
		}
		#endregion

		#region Public Static Method : string PadRightFixLength(this string source, int totalByteLength)
		/// <summary>以原始字串靠左，向右填滿固定長度的空白；原始字串長度比總長度大時，將會截斷。</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadRightFixLength(this string source, int totalByteLength)
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

		#region Public Static Method : string PadLeftFixLength(this string source, int totalByteLength)
		/// <summary>以原始字串靠右，向左填滿固定長度的空白；原始字串長度比總長度大時，將會截斷。</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadLeftFixLength(this string source, int totalByteLength)
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

		#region Public Static Method : string PadCenter(this string source, int totalByteLength)
		/// <summary>以原始字串靠中，向左右填滿空</summary>
		/// <param name="source">來源字串</param>
		/// <param name="totalByteLength">總長度，位元為單位</param>
		/// <returns></returns>
		public static string PadCenter(this string source, int totalByteLength)
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
	}
	#endregion

	#region Public Static Class : Int64Extension
	/// <summary>Int64 資料類型擴充函示</summary>
	public static class Int64Extension
	{
		#region Public Static Method : bool[] ToBitArray(this Int64 value)
		/// <summary>將 Int64 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 Int64 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this Int64 value)
		{
			return Convert.ToString(value, 2).PadLeft(64, '0').Select(s => s.Equals('1')).ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this Int64 value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this Int64 value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : UInt64Extension
	/// <summary>UInt64 資料類型擴充函示</summary>
	public static class UInt64Extension
	{
		#region Public Static Method : bool[] ToBitArray(this UInt64 value)
		/// <summary>將 UInt64 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 UInt64 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this UInt64 value)
		{
			return ((Int64)value).ToBitArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this UInt64 value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this UInt64 value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : Int32Extension
	/// <summary>Int32 資料類型擴充函示</summary>
	public static class Int32Extension
	{
		#region Public Static Method : bool[] ToBitArray(this Int32 value)
		/// <summary>將 Int32 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 Int32 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this Int32 value)
		{
			return Convert.ToString(value, 2).PadLeft(32, '0').Select(s => s.Equals('1')).ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this Int32 value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this Int32 value, bool getBigEndian)
		{
			byte[] tmp = ((long)value).GetBytes(getBigEndian);
			byte[] res = new byte[4];
			if (getBigEndian)
				Array.Copy(tmp, 4, res, 0, 4);
			else
				Array.Copy(tmp, 0, res, 0, 4);
			return res;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : UInt32Extension
	/// <summary>UInt32 資料類型擴充函示</summary>
	public static class UInt32Extension
	{
		#region Public Static Method : bool[] ToBitArray(this UInt32 value)
		/// <summary>將 UInt32 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 UInt32 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this UInt32 value)
		{
			return Convert.ToString(value, 2).PadLeft(32, '0').Select(s => s.Equals('1')).ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this UInt32 value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this UInt32 value, bool getBigEndian)
		{
			byte[] tmp = ((ulong)value).GetBytes(getBigEndian);
			byte[] res = new byte[4];
			if (getBigEndian)
				Array.Copy(tmp, 4, res, 0, 4);
			else
				Array.Copy(tmp, 0, res, 0, 4);
			return res;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : Int16Extension
	/// <summary>Int16 資料類型擴充函示</summary>
	public static class Int16Extension
	{
		#region Public Static Method : bool[] ToBitArray(this Int16 value)
		/// <summary>將 Int16 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 Int16 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this Int16 value)
		{
			return Convert.ToString(value, 2).PadLeft(16, '0').Select(s => s.Equals('1')).ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this Int16 value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this Int16 value, bool getBigEndian)
		{
			byte[] tmp = ((long)value).GetBytes(getBigEndian);
			byte[] res = new byte[2];
			if (getBigEndian)
				Array.Copy(tmp, 6, res, 0, 2);
			else
				Array.Copy(tmp, 0, res, 0, 2);
			return res;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : UInt16Extension
	/// <summary>UInt16 資料類型擴充函示</summary>
	public static class UInt16Extension
	{
		#region Public Static Method : bool[] ToBitArray(this UInt16 value)
		/// <summary>將 UInt16 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 UInt16 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this UInt16 value)
		{
			return Convert.ToString(value, 2).PadLeft(16, '0').Select(s => s.Equals('1')).ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this UInt16 value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this UInt16 value, bool getBigEndian)
		{
			byte[] tmp = ((ulong)value).GetBytes(getBigEndian);
			byte[] res = new byte[2];
			if (getBigEndian)
				Array.Copy(tmp, 6, res, 0, 2);
			else
				Array.Copy(tmp, 0, res, 0, 2);
			return res;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : ByteExtension
	/// <summary>Byte 資料類型擴充函示</summary>
	public static class ByteExtension
	{
		#region Public Static Method : bool[] ToBitArray(this byte value)
		/// <summary>將 Byte 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 Byte 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this byte value)
		{
			return Convert.ToString(value, 2).PadLeft(8, '0').Select(s => s.Equals('1')).ToArray();
		}
		#endregion

		#region Public Static Method : bool BitValue(this byte value, int index)
		/// <summary>取得 Byte 資料的位元值</summary>
		/// <param name="value">欲轉換的 Byte 資料</param>
		/// <param name="index">位元索引值；HSB:0, LSB:7</param>
		/// <returns></returns>
		public static bool BitValue(this byte value, int index)
		{
			if (index < 0 || index > 8)
				throw new ArgumentOutOfRangeException("index");
			return value.ToBitArray()[index];
		}
		#endregion
	}
	#endregion

	#region Public Static Class : SingleExtension
	/// <summary>Single 資料類型擴充函示</summary>
	public static class SingleExtension
	{
		#region Public Static Method : bool[] ToBitArray(this Single value)
		/// <summary>將 Single 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 Single 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this Single value)
		{
			byte[] tmp = BitConverter.GetBytes(value);
			List<bool> v = new List<bool>();
			foreach (byte b in tmp)
				v.AddRange(b.ToBitArray());
			return v.ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this Single value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this Single value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1f)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : DoubleExtension
	/// <summary>Double 資料類型擴充函示</summary>
	public static class DoubleExtension
	{
		#region Public Static Method : bool[] ToBitArray(this Double value)
		/// <summary>將 Double 資料類型轉成 Bit Array</summary>
		/// <param name="value">欲轉換的 Double 資料</param>
		/// <returns></returns>
		public static bool[] ToBitArray(this Double value)
		{
			byte[] tmp = BitConverter.GetBytes(value);
			List<bool> v = new List<bool>();
			foreach (byte b in tmp)
				v.AddRange(b.ToBitArray());
			return v.ToArray();
		}
		#endregion

		#region Public Static Method : byte[] GetBytes(this double value, bool getBigEndian)
		/// <summary>取得數值的位元組陣列</summary>
		/// <param name="value">數值</param>
		/// <param name="getBigEndian">是否傳回BigEndian的陣列</param>
		/// <returns>位元組陣列</returns>
		public static byte[] GetBytes(this double value, bool getBigEndian)
		{
			bool sysIsLittleEndian = (BitConverter.GetBytes(1d)[0] == 0);
			byte[] tmp = BitConverter.GetBytes(value);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(tmp);
			return tmp;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : ArrayExtension
	/// <summary>陣列資料類型擴充函示</summary>
	public static class ArrayExtension
	{
		#region Public Static Method : string ToHexString(this byte[] arr)
		/// <summary>
		/// 將位元組陣列轉換成16進位字串
		/// </summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <returns>16進位字串</returns>
		public static string ToHexString(this byte[] arr)
		{
			return arr.ToHexString(" ");
		}
		#endregion

		#region Public Static Method : string ToHexString(this byte[] arr, string split)
		/// <summary>將位元組陣列轉換成16進位字串</summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="split">分隔字串</param>
		/// <returns>16進位字串</returns>
		public static string ToHexString(this byte[] arr, string split)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte c in arr)
				sb.AppendFormat("{0:X2}{1}", c, split);
			return sb.ToString().TrimEnd(split.ToCharArray());
		}
		#endregion

		#region Public Static Method : string ToHexString(this byte[] arr, int count)
		/// <summary>將位元組陣列轉換成16進位字串</summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="count">欲轉換的長度</param>
		/// <returns>16進位字串</returns>
		public static string ToHexString(this byte[] arr, int count)
		{
			return arr.ToHexString(count, " ");
		}
		#endregion

		#region Public Static Method : string ToHexString(this byte[] arr, int count, string split)
		/// <summary>將位元組陣列轉換成16進位字串</summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="count">欲轉換的長度</param>
		/// <param name="split">分隔字串</param>
		/// <returns>16進位字串</returns>
		public static string ToHexString(this byte[] arr, int count, string split)
		{
			if (arr.Length < count)
				throw new ArgumentException("count 參數值不得大於 arr 陣列長度!");
			byte[] tmp = new byte[count];
			Array.Copy(arr, tmp, count);
			return tmp.ToHexString(split);
		}
		#endregion

		#region Public Static Method : string ToHexString(this byte[] arr, int start, int count)
		/// <summary>將位元組陣列轉換成16進位字串</summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="start">開始位置</param>
		/// <param name="count">欲轉換的長度</param>
		/// <returns>16進位字串</returns>
		public static string ToHexString(this byte[] arr, int start, int count)
		{
			return arr.ToHexString(start, count, " ");
		}
		#endregion

		#region Public Static Method : string ToHexString(this byte[] arr, int start, int count, string split)
		/// <summary>
		/// 將位元組陣列轉換成16進位字串
		/// </summary>
		/// <param name="arr">欲轉換的位元組陣列</param>
		/// <param name="start">開始位置</param>
		/// <param name="count">欲轉換的長度</param>
		/// <param name="split">分隔字串</param>
		/// <returns>16進位字串</returns>
		/// <exception cref="System.IndexOutOfRangeException">start 與 count 總和值不得大於 arr 陣列長度!</exception>
		/// <exception cref="System.ArgumentException">start 參數值必須大於等於 0 且小於 arr 陣列長度!</exception>
		public static string ToHexString(this byte[] arr, int start, int count, string split)
		{
			if (start < 0 || start >= arr.Length)
				throw new ArgumentException("start 參數值必須大於等於 0 且小於 arr 陣列長度!");
			if (start + count > arr.Length)
				throw new IndexOutOfRangeException("start 與 count 總和值不得大於 arr 陣列長度!");
			byte[] tmp = new byte[count];
			Array.Copy(arr, start, tmp, 0, count);
			return tmp.ToHexString(split);
		}
		#endregion

		#region Public Static Method : string ToHexString<T>(this T[] arr, string split = " ")
		/// <summary>將泛型型別陣列轉換成 16 進位字串，每一元素使用 BigEndian 方式輸出 16 進位值。</summary>
		/// <param name="self">欲轉換的泛型型別陣列</param>
		/// <param name="split">分隔字串</param>
		/// <returns>16進位字串</returns>
		/// <exception cref="System.NotSupportedException">未支援的型別。僅支援數字類型的型別。</exception>
		public static string ToHexString<T>(this T[] self, string split = " ")
		{
			StringBuilder sb = new StringBuilder();
			byte[] buf = null;
			foreach (T c in self)
			{
				if (c is byte)
					buf = new byte[] { (byte)Convert.ChangeType(c, c.GetType()) };
				else if (c is ushort)
					buf = BitConverter.GetBytes((ushort)Convert.ChangeType(c, c.GetType()));
				else if (c is uint)
					buf = BitConverter.GetBytes((uint)Convert.ChangeType(c, c.GetType()));
				else if (c is ulong)
					buf = BitConverter.GetBytes((ulong)Convert.ChangeType(c, c.GetType()));
				else if (c is short)
					buf = BitConverter.GetBytes((short)Convert.ChangeType(c, c.GetType()));
				else if (c is int)
					buf = BitConverter.GetBytes((int)Convert.ChangeType(c, c.GetType()));
				else if (c is long)
					buf = BitConverter.GetBytes((long)Convert.ChangeType(c, c.GetType()));
				else if (c is float)
					buf = BitConverter.GetBytes((float)Convert.ChangeType(c, c.GetType()));
				else if (c is double)
					buf = BitConverter.GetBytes((double)Convert.ChangeType(c, c.GetType()));
				else if (c is char)
					buf = BitConverter.GetBytes((char)Convert.ChangeType(c, c.GetType()));
				else
					throw new NotSupportedException();
				if (BitConverter.IsLittleEndian)
					Array.Reverse(buf);
				sb.Append(buf.ToHexString(""));
				sb.Append(split);
			}
			return sb.ToString().TrimEnd(split.ToCharArray());

		}
		#endregion

		#region Public Static Method : int IndexOfBytes(this byte[] self, byte[] pattern)
		/// <summary>尋找位元組陣列中的特定陣列值</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位元組的位置</returns>
		public static int IndexOfBytes(this byte[] self, byte[] pattern)
		{
			return IndexOfPattern<byte>(self, pattern, 0);
		}
		#endregion

		#region Public Static Method : int IndexOfBytes(this byte[] self, byte[] pattern, int startIndex)
		/// <summary>尋找位元組陣列中的特定陣列值</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <param name="startIndex">起始位置</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位元組的位置</returns>
		public static int IndexOfBytes(this byte[] self, byte[] pattern, int startIndex)
		{
			return self.IndexOfPattern(pattern, startIndex);
		}
		#endregion

		#region Public Static Method : int IndexOfPattern<T>(this T[] self, T[] pattern)
		/// <summary>從索引位置 0 開始尋找 T 資料型態陣列中的特定陣列值</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位置</returns>
		public static int IndexOfPattern<T>(this T[] self, T[] pattern)
		{
			return IndexOfPattern<T>(self, pattern, 0);
		}
		#endregion

		#region Public Static Method : int IndexOfPattern<T>(this T[] self, T[] pattern, int startIndex)
		/// <summary>尋找 T 資料型態陣列中的特定陣列值</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <param name="startIndex">起始索引位置</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位置</returns>
		public static int IndexOfPattern<T>(this T[] self, T[] pattern, int startIndex)
		{
			if (self == null || self.Length == 0 || pattern == null || pattern.Length == 0)
				return -1;
			for (int i = startIndex; i < self.Length; i++)
			{
				if (!IsMatch(self, i, pattern)) continue;
				return i;
			}
			return -1;
		}
		#endregion

		#region Public Static Method : int IndexOfPattern<T>(this List<T> self, T[] pattern)
		/// <summary>尋找 T 資料型態陣列中的特定陣列值</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位置</returns>
		public static int IndexOfPattern<T>(this List<T> self, T[] pattern)
		{
			return IndexOfPattern<T>(self, pattern, 0);
		}
		#endregion

		#region Public Static Method : int IndexOfPattern<T>(this List<T> self, T[] pattern, int startIndex)
		/// <summary>尋找 T 資料型態陣列中的特定陣列值</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <param name="startIndex">起始索引位置</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位置</returns>
		public static int IndexOfPattern<T>(this List<T> self, T[] pattern, int startIndex)
		{
			if (self == null || self.Count == 0 || pattern == null || pattern.Length == 0)
				return -1;
			return self.ToArray().IndexOfPattern<T>(pattern, startIndex);
		}
		#endregion

		#region Public Static Method : int LastOfBytes(this byte[] source, byte[] pattern)
		/// <summary>自位元組陣列後面開始尋找特定陣列值</summary>
		/// <param name="source">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位元組的位置</returns>
		public static int LastOfBytes(this byte[] source, byte[] pattern)
		{
			return LastOfBytes(source, pattern, source.Length);
		}
		#endregion

		#region Public Static Method : int LastOfBytes(this byte[] source, byte[] pattern, int count)
		/// <summary>自位元組陣列後面開始尋找特定陣列值</summary>
		/// <param name="source">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <param name="count">欲搜尋的原始陣列長度</param>
		/// <returns>-1:未搜尋到結果；大於等於 0:第一個位元組的位置</returns>
		public static int LastOfBytes(this byte[] source, byte[] pattern, int count)
		{
			byte[] tmp = new byte[count];
			Array.Copy(source, tmp, tmp.Length);
			int result = Array.LastIndexOf<byte>(tmp, pattern[pattern.Length - 1]);
			if (result == -1)
				return -1;

			int fidx = 1;
			bool exists = true;
			Array.Copy(source, tmp, tmp.Length);
			while (result != -1)
			{
				fidx = 1;
				exists = true;
				while (exists && fidx < pattern.Length)
				{
					exists = (tmp[result - fidx] == pattern[pattern.Length - fidx - 1]);
					fidx++;
				}
				if (exists)
					return result - pattern.Length + 1;
				tmp = new byte[result - 1];
				Array.Copy(source, tmp, tmp.Length);
				result = Array.LastIndexOf<byte>(tmp, pattern[pattern.Length - 1]);
			}
			return -1;
		}
		#endregion

		#region Public Static Method : int[] PatternLocations(this byte[] self, byte[] pattern, int startIndex = 0)
		/// <summary>尋找位元組陣列中的特定陣列值，並傳回所有位置</summary>
		/// <param name="self">原始陣列</param>
		/// <param name="pattern">欲搜尋的陣列</param>
		/// <param name="startIndex">起始位置</param>
		/// <returns>null:未搜尋到結果；Length 大於等於 1:第一個位置的索引值</returns>
		public static int[] PatternLocations<T>(this T[] self, T[] pattern, int startIndex = 0)
		{
			if (self == null || self.Length == 0 || pattern == null || pattern.Length == 0)
				return null;
			System.Collections.Generic.List<int> list = new System.Collections.Generic.List<int>();
			for (int i = startIndex; i < self.Length; i++)
			{
				if (!IsMatch(self, i, pattern))
					continue;
				list.Add(i);
			}
			return list.Count == 0 ? null : list.ToArray();
		}
		#endregion

		#region Public Static Method : byte ToByte(this bool[] source)
		/// <summary>將布林值陣列轉成位元組</summary>
		/// <param name="source">布林值陣列</param>
		/// <returns></returns>
		public static byte ToByte(this bool[] source)
		{
			if (source.Length > 8)
				throw new ArgumentOutOfRangeException();
			bool[] tmp = new bool[source.Length];
			Array.Copy(source, tmp, tmp.Length);
			Array.Reverse(tmp);
			System.Collections.BitArray ba = new System.Collections.BitArray(tmp);
			byte[] res = new byte[1];
			ba.CopyTo(res, 0);
			return res[0];
		}
		#endregion

		#region Public Static Method : int ToInt32(this bool[] source)
		/// <summary>將布林值陣列轉成32位元整數</summary>
		/// <param name="source">布林值陣列</param>
		/// <returns></returns>
		public static int ToInt32(this bool[] source)
		{
			if (source.Length > 32)
				throw new ArgumentOutOfRangeException();
			bool[] tmp = new bool[source.Length];
			Array.Copy(source, tmp, tmp.Length);
			Array.Reverse(tmp);
			System.Collections.BitArray ba = new System.Collections.BitArray(tmp);
			int[] res = new int[1];
			ba.CopyTo(res, 0);
			return res[0];
		}
		#endregion

		#region Public Static Method : string ToBinaryString(this bool[] source)
		/// <summary>將布林值陣列轉成二進位制字串</summary>
		/// <param name="source">布林值陣列</param>
		/// <returns></returns>
		public static string ToBinaryString(this bool[] source)
		{
			string res = string.Empty;
			foreach (bool b in source)
				res += b ? "1" : "0";
			return res;
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

		#region Public Static Method : long ToInt64(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成含符號長整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static long ToInt64(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 8)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt64(bytes, 0);
		}
		#endregion

		#region Public Static Method : long ToInt64(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成含符號長整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 8 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		/// <returns>數值</returns>
		public static long ToInt64(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 8)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[8];
			Array.Copy(bytes, start, buffer, 0, 8);
			return buffer.ToInt64(getBigEndian);
		}
		#endregion

		#region Public Static Method : ulong ToUInt64(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成無符號長整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static ulong ToUInt64(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 8)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToUInt64(bytes, 0);
		}
		#endregion

		#region Public Static Method : ulong ToUInt64(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成無符號長整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 8 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static ulong ToUInt64(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 8)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[8];
			Array.Copy(bytes, start, buffer, 0, 8);
			return buffer.ToUInt64(getBigEndian);
		}
		#endregion

		#region Public Static Method : int ToInt32(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成含符號整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static int ToInt32(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 4)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt32(bytes, 0);
		}
		#endregion

		#region Public Static Method : int ToInt32(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成含符號整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 4 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static int ToInt32(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 4)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[4];
			Array.Copy(bytes, start, buffer, 0, 4);
			return buffer.ToInt32(getBigEndian);
		}
		#endregion

		#region Public Static Method : uint ToUInt32(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成無符號 32 位元整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static uint ToUInt32(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 4)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToUInt32(bytes, 0);
		}
		#endregion

		#region Public Static Method : uint ToUInt32(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成無符號 32 位元整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 4 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static uint ToUInt32(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 4)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[4];
			Array.Copy(bytes, start, buffer, 0, 4);
			return buffer.ToUInt32(getBigEndian);
		}
		#endregion

		#region Public Static Method : short ToInt16(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成含符號 16 位元整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static short ToInt16(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 2)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToInt16(bytes, 0);
		}
		#endregion

		#region Public Static Method : short ToInt16(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成含符號 16 位元整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 2 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static short ToInt16(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 2)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[2];
			Array.Copy(bytes, start, buffer, 0, 2);
			return buffer.ToInt16(getBigEndian);
		}
		#endregion

		#region Public Static Method : ushort ToUInt16(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成無符號 16 位元整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static ushort ToUInt16(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 2)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToUInt16(bytes, 0);
		}
		#endregion

		#region Public Static Method : ushort ToUInt16(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成無符號 16 位元整數</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 2 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static ushort ToUInt16(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 2)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[2];
			Array.Copy(bytes, start, buffer, 0, 2);
			return buffer.ToUInt16(getBigEndian);
		}
		#endregion

		#region Public Static Method : double ToDouble(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成倍精度數值</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static double ToDouble(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 8)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1d)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToDouble(bytes, 0);
		}
		#endregion

		#region Public Static Method : double ToDouble(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成倍精度數值</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 8 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static double ToDouble(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 8)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[8];
			Array.Copy(bytes, start, buffer, 0, 8);
			return buffer.ToDouble(getBigEndian);
		}
		#endregion

		#region Public Static Method : float ToSingle(this byte[] bytes, bool getBigEndian)
		/// <summary>將位元組陣列轉換成單精度數值</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static float ToSingle(this byte[] bytes, bool getBigEndian)
		{
			if (bytes.Length < 4)
				throw new IndexOutOfRangeException();
			bool sysIsLittleEndian = (BitConverter.GetBytes(1f)[0] == 0);
			if (sysIsLittleEndian != getBigEndian)
				Array.Reverse(bytes);
			return BitConverter.ToSingle(bytes, 0);
		}
		#endregion

		#region Public Static Method : float ToSingle(this byte[] bytes, int start, bool getBigEndian)
		/// <summary>將位元組陣列轉換成單精度數值</summary>
		/// <param name="bytes">位元組陣列</param>
		/// <param name="start">取用起始索引，共取 4 個Bytes</param>
		/// <param name="getBigEndian">傳入的陣列是否為BigEndian</param>
		/// <returns>數值</returns>
		/// <exception cref="System.IndexOutOfRangeException">陣列長度不足</exception>
		public static float ToSingle(this byte[] bytes, int start, bool getBigEndian)
		{
			if (bytes.Length < start + 4)
				throw new IndexOutOfRangeException();
			byte[] buffer = new byte[4];
			Array.Copy(bytes, start, buffer, 0, 4);
			return buffer.ToSingle(getBigEndian);
		}
		#endregion

		#region Public Static Method : byte[] Xor(this byte[] bytes, byte code)
		/// <summary>將位元組陣列以 code 做 XOR 運算</summary>
		/// <param name="bytes">原始位元組陣列</param>
		/// <param name="code">欲運算的位元組值</param>
		/// <returns>運算結果</returns>
		public static byte[] Xor(this byte[] bytes, byte code)
		{
			byte[] res = new byte[bytes.Length];
			for (int i = 0; i < res.Length; i++)
				res[i] = (byte)(bytes[i] ^ code);
			return res;
		}
		#endregion

		#region Public Static Method : byte[] Xor(this byte[] bytes, byte[] codes)
		/// <summary>將位元組陣列以 codes 做 XOR 運算。</summary>
		/// <param name="bytes">原始位元組陣列</param>
		/// <param name="codes">欲運算的位元組陣列值</param>
		/// <returns>運算結果</returns>
		public static byte[] Xor(this byte[] bytes, byte[] codes)
		{
			byte[] res = new byte[bytes.Length];
			for (int i = 0; i < res.Length; i += codes.Length)
			{
				for (int j = 0; j < codes.Length; j++)
				{
					if (i + j >= bytes.Length)
						break;
					res[i + j] = (byte)(bytes[i + j] ^ codes[j]);
				}
			}
			return res;
		}
		#endregion
	}
	#endregion

	#region Public Static Class : ObjectExtension
	/// <summary>物件資料類型擴充函示</summary>
	public static class ObjectExtension
	{
		#region Public Static Method : bool IsNumeric(this object o)
		/// <summary>檢查物件是否為數字型別</summary>
		/// <param name="o">欲檢查的物件</param>
		/// <returns></returns>
		public static bool IsNumeric(this object o)
		{
			if (!o.GetType().IsValueType || o.GetType().IsArray || o.GetType().IsClass)
				return false;
			else
			{
				ValueType vt = (ValueType)o;
				return (vt is Byte || vt is Int16 || vt is Int32 || vt is Int64 ||
					vt is SByte || vt is UInt16 || vt is UInt32 || vt is UInt64 ||
					vt is Decimal || vt is Double || vt is Single);
			}
		}
		#endregion

		#region Public Method : bool IsBoolean(this object o)
		/// <summary>檢查物件是否為 Boolean 型別</summary>
		/// <param name="o">欲檢查的物件</param>
		/// <returns></returns>
		public static bool IsBoolean(this object o)
		{
			return (o.GetType().Equals(typeof(bool)));
		}
		#endregion

		#region Public Static Method : bool IsDateTime(this object o)
		/// <summary>檢查物件是否為 DateTime 型別</summary>
		/// <param name="o">欲檢查的物件</param>
		/// <returns></returns>
		public static bool IsDateTime(this object o)
		{
			return (o.GetType().Equals(typeof(DateTime)));
		}
		#endregion
	}
	#endregion
}
