using System;

namespace CJF.Utility.CRC
{
	/*
	 * CRC 16 CCITT
	 * From : http://www.sanity-free.org/133/crc_16_ccitt_in_csharp.html
	 */
	#region Public Enum : InitialCrcValue(ushort)
	/// <summary>初始值列舉</summary>
	public enum InitialCrcValue : ushort
	{
		/// <summary>適用於一般</summary>
		Zeros = 0x0000,
		/// <summary>適用於 Modbus</summary>
		NonZero1 = 0xFFFF,
		/// <summary>未知用途</summary>
		NonZero2 = 0x1D0F
	}
	#endregion

	/// <summary>CRC 16 CCITT</summary>
	public class Crc16Ccitt
	{
		const ushort poly = 4129;
		ushort[] table = new ushort[256];
		ushort initialValue = 0;

		#region Public Method : ushort ComputeChecksum(tring fileName)
		/// <summary>計算CRC</summary>
		/// <param name="fileName">欲計算的檔案名稱</param>
		/// <returns>計算後的 CRC16 值</returns>
		public ushort ComputeChecksum(string fileName)
		{
			byte[] source = System.IO.File.ReadAllBytes(fileName);
			return ComputeChecksum(source);
		}
		#endregion

		#region Public Method : ushort ComputeChecksum(byte[] source)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <returns>計算後的 CRC16 值</returns>
		public ushort ComputeChecksum(byte[] source)
		{
			ushort crc = this.initialValue;
			for (int i = 0; i < source.Length; ++i)
				crc = (ushort)((crc << 8) ^ table[((crc >> 8) ^ (0xff & source[i]))]);
			return crc;
		}
		#endregion

		#region Public Method : ushort ComputeChecksum(byte[] source, ushort start, ushort length)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <param name="startIndex">開始計算的陣列索引值</param>
		/// <param name="length">欲計算的長度</param>
		/// <returns>計算後的 CRC16 值</returns>
		/// <exception cref="ArgumentNullException">source 為 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">startIndex 小於 source 第一個維度的下限。<br />
		/// - 或 -length 小於零。
		/// </exception>
		/// <exception cref="ArgumentException">length 大於從 startIndex 到 source 結尾的元素數目。</exception>
		public ushort ComputeChecksum(byte[] source, ushort startIndex, ushort length)
		{
			byte[] tmp = new byte[length];
			Array.Copy(source, startIndex, tmp, 0, length);
			return ComputeChecksum(tmp);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksum(tring fileName)
		/// <summary>計算CRC</summary>
		/// <param name="fileName">欲計算的檔案名稱</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		public byte[] ComputeChecksumBytes(string fileName)
		{
			byte[] source = System.IO.File.ReadAllBytes(fileName);
			return ComputeChecksumBytes(source);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksumBytes(byte[] source)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		public byte[] ComputeChecksumBytes(byte[] source)
		{
			ushort crc = ComputeChecksum(source);
			return BitConverter.GetBytes(crc);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksumBytes(byte[] source, ushort startIndex, ushort length)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <param name="startIndex">開始計算的陣列索引值</param>
		/// <param name="length">欲計算的長度</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		/// <exception cref="ArgumentNullException">source 為 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">startIndex 小於 source 第一個維度的下限。<br />
		/// - 或 -length 小於零。
		/// </exception>
		/// <exception cref="ArgumentException">length 大於從 startIndex 到 source 結尾的元素數目。</exception>
		public byte[] ComputeChecksumBytes(byte[] source, ushort startIndex, ushort length)
		{
			byte[] tmp = new byte[length];
			Array.Copy(source, startIndex, tmp, 0, length);
			return ComputeChecksumBytes(tmp);
		}
		#endregion

		#region Consture Method : Crc16Ccitt(InitialCrcValue initialValue)
		/// <summary>建立一個 Crc16Ccitt 類別</summary>
		/// <param name="initialValue">初始計算列舉值</param>
		public Crc16Ccitt(InitialCrcValue initialValue)
		{
			this.initialValue = (ushort)initialValue;
			ushort temp, a;
			for (int i = 0; i < table.Length; ++i)
			{
				temp = 0;
				a = (ushort)(i << 8);
				for (int j = 0; j < 8; ++j)
				{
					if (((temp ^ a) & 0x8000) != 0)
						temp = (ushort)((temp << 1) ^ poly);
					else
						temp <<= 1;
					a <<= 1;
				}
				table[i] = temp;
			}
		}
		#endregion
	}

	/*
	 * CRC 16
	 * From : http://www.sanity-free.org/134/standard_crc_16_in_csharp.html
	 */
	/// <summary>CRC 16</summary>
	public class Crc16
	{
		const ushort polynomial = 0xA001;
		ushort[] table = new ushort[256];

		#region Public Method : ushort ComputeChecksum(tring fileName)
		/// <summary>計算CRC</summary>
		/// <param name="fileName">欲計算的檔案名稱</param>
		/// <returns>計算後的 CRC16 值</returns>
		public ushort ComputeChecksum(string fileName)
		{
			byte[] source = System.IO.File.ReadAllBytes(fileName);
			return ComputeChecksum(source);
		}
		#endregion

		#region Public Method : ushort ComputeChecksum(byte[] source)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <returns>計算後的 CRC16 值</returns>
		public ushort ComputeChecksum(byte[] source)
		{
			ushort crc = 0;
			for (int i = 0; i < source.Length; ++i)
				crc = (ushort)((crc >> 8) ^ table[(byte)(crc ^ source[i])]);
			return crc;
		}
		#endregion

		#region Public Method : ushort ComputeChecksum(byte[] source, ushort start, ushort length)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <param name="startIndex">開始計算的陣列索引值</param>
		/// <param name="length">欲計算的長度</param>
		/// <returns>計算後的 CRC16 值</returns>
		/// <exception cref="ArgumentNullException">source 為 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">startIndex 小於 source 第一個維度的下限。<br />
		/// - 或 -length 小於零。
		/// </exception>
		/// <exception cref="ArgumentException">length 大於從 startIndex 到 source 結尾的元素數目。</exception>
		public ushort ComputeChecksum(byte[] source, ushort startIndex, ushort length)
		{
			byte[] tmp = new byte[length];
			Array.Copy(source, startIndex, tmp, 0, length);
			return ComputeChecksum(tmp);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksum(tring fileName)
		/// <summary>計算CRC</summary>
		/// <param name="fileName">欲計算的檔案名稱</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		public byte[] ComputeChecksumBytes(string fileName)
		{
			byte[] source = System.IO.File.ReadAllBytes(fileName);
			return ComputeChecksumBytes(source);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksumBytes(byte[] source)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		public byte[] ComputeChecksumBytes(byte[] source)
		{
			ushort crc = ComputeChecksum(source);
			return BitConverter.GetBytes(crc);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksumBytes(byte[] source, ushort startIndex, ushort length)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <param name="startIndex">開始計算的陣列索引值</param>
		/// <param name="length">欲計算的長度</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		/// <exception cref="ArgumentNullException">source 為 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">startIndex 小於 source 第一個維度的下限。<br />
		/// - 或 -length 小於零。
		/// </exception>
		/// <exception cref="ArgumentException">length 大於從 startIndex 到 source 結尾的元素數目。</exception>
		public byte[] ComputeChecksumBytes(byte[] source, ushort startIndex, ushort length)
		{
			byte[] tmp = new byte[length];
			Array.Copy(source, startIndex, tmp, 0, length);
			return ComputeChecksumBytes(tmp);
		}
		#endregion

		#region Consture Method : Crc16()
		/// <summary>建立一個 Crc16 類別</summary>
		public Crc16()
		{
			ushort value;
			ushort temp;
			for (ushort i = 0; i < table.Length; ++i)
			{
				value = 0;
				temp = i;
				for (byte j = 0; j < 8; ++j)
				{
					if (((value ^ temp) & 0x0001) != 0)
						value = (ushort)((value >> 1) ^ polynomial);
					else
						value >>= 1;
					temp >>= 1;
				}
				table[i] = value;
			}
		}
		#endregion
	}
	public class Crc16Table
	{
		#region Private Const : CRC16_TABLE for CRC16
		private UInt16[] CRC16_TABLE = {
			0x0000,0xC0C1,0xC181,0x0140,0xC301,0x03C0,0x0280,0xC241,
			0xC601,0x06C0,0x0780,0xC741,0x0500,0xC5C1,0xC481,0x0440,
			0xCC01,0x0CC0,0x0D80,0xCD41,0x0F00,0xCFC1,0xCE81,0x0E40,
			0x0A00,0xCAC1,0xCB81,0x0B40,0xC901,0x09C0,0x0880,0xC841,
			0xD801,0x18C0,0x1980,0xD941,0x1B00,0xDBC1,0xDA81,0x1A40,
			0x1E00,0xDEC1,0xDF81,0x1F40,0xDD01,0x1DC0,0x1C80,0xDC41,
			0x1400,0xD4C1,0xD581,0x1540,0xD701,0x17C0,0x1680,0xD641,
			0xD201,0x12C0,0x1380,0xD341,0x1100,0xD1C1,0xD081,0x1040,
			0xF001,0x30C0,0x3180,0xF141,0x3300,0xF3C1,0xF281,0x3240,
			0x3600,0xF6C1,0xF781,0x3740,0xF501,0x35C0,0x3480,0xF441,
			0x3C00,0xFCC1,0xFD81,0x3D40,0xFF01,0x3FC0,0x3E80,0xFE41,
			0xFA01,0x3AC0,0x3B80,0xFB41,0x3900,0xF9C1,0xF881,0x3840,
			0x2800,0xE8C1,0xE981,0x2940,0xEB01,0x2BC0,0x2A80,0xEA41,
			0xEE01,0x2EC0,0x2F80,0xEF41,0x2D00,0xEDC1,0xEC81,0x2C40,
			0xE401,0x24C0,0x2580,0xE541,0x2700,0xE7C1,0xE681,0x2640,
			0x2200,0xE2C1,0xE381,0x2340,0xE101,0x21C0,0x2080,0xE041,
			0xA001,0x60C0,0x6180,0xA141,0x6300,0xA3C1,0xA281,0x6240,
			0x6600,0xA6C1,0xA781,0x6740,0xA501,0x65C0,0x6480,0xA441,
			0x6C00,0xACC1,0xAD81,0x6D40,0xAF01,0x6FC0,0x6E80,0xAE41,
			0xAA01,0x6AC0,0x6B80,0xAB41,0x6900,0xA9C1,0xA881,0x6840,
			0x7800,0xB8C1,0xB981,0x7940,0xBB01,0x7BC0,0x7A80,0xBA41,
			0xBE01,0x7EC0,0x7F80,0xBF41,0x7D00,0xBDC1,0xBC81,0x7C40,
			0xB401,0x74C0,0x7580,0xB541,0x7700,0xB7C1,0xB681,0x7640,
			0x7200,0xB2C1,0xB381,0x7340,0xB101,0x71C0,0x7080,0xB041,
			0x5000,0x90C1,0x9181,0x5140,0x9301,0x53C0,0x5280,0x9241,
			0x9601,0x56C0,0x5780,0x9741,0x5500,0x95C1,0x9481,0x5440,
			0x9C01,0x5CC0,0x5D80,0x9D41,0x5F00,0x9FC1,0x9E81,0x5E40,
			0x5A00,0x9AC1,0x9B81,0x5B40,0x9901,0x59C0,0x5880,0x9841,
			0x8801,0x48C0,0x4980,0x8941,0x4B00,0x8BC1,0x8A81,0x4A40,
			0x4E00,0x8EC1,0x8F81,0x4F40,0x8D01,0x4DC0,0x4C80,0x8C41,
			0x4400,0x84C1,0x8581,0x4540,0x8701,0x47C0,0x4680,0x8641,
			0x8201,0x42C0,0x4380,0x8341,0x4100,0x81C1,0x8081,0x4040,
		};
		#endregion

		ushort initialValue = 0;

		#region Consture Method : Crc16Table(InitialCrcValue initValue)
		/// <summary>建立一個 Crc16Table 類別</summary>
		/// <param name="initValue">初始計算列舉值</param>
		public Crc16Table(InitialCrcValue initValue)
		{
			this.initialValue = (ushort)initValue;
		}
		#endregion

		#region Public Method : UInt16 ComputeChecksum(byte[] pDataIn)
		/// <summary>使用查表法取得 CRC16 檢查碼</summary>
		/// <param name="pDataIn">欲運算的位元組資料</param>
		public UInt16 ComputeChecksum(byte[] pDataIn)
		{
			// wResult 改成 0xFFFF 時，即可用於 Modbus
			UInt16 wResult = this.initialValue;
			UInt16 wTableNo = 0;
			UInt16 i = 0;

			for (i = 0; i < pDataIn.Length; i++)
			{
				wTableNo = (UInt16)((wResult & 0xff) ^ (pDataIn[i] & 0xff));
				wResult = (UInt16)(((wResult >> 8) & 0xff) ^ CRC16_TABLE[(UInt16)wTableNo]);
			}
			return wResult;
		}
		#endregion

		#region Public Method : UInt16 ComputeChecksumBytes(byte[] source)
		/// <summary>使用查表法取得 CRC16 檢查碼</summary>
		/// <param name="source">欲運算的位元組資料</param>
		public byte[] ComputeChecksumBytes(byte[] source)
		{
			ushort crc = ComputeChecksum(source);
			return BitConverter.GetBytes(crc);
		}
		#endregion

		#region Public Method : UInt16 ComputeChecksum(byte[] pDataIn, UInt16 start, UInt16 length)
		/// <summary>使用查表法取得 CRC16 檢查碼</summary>
		/// <param name="source">欲運算的位元組資料</param>
		/// <param name="startIndex">開始索引值</param>
		/// <param name="length">欲檢查的長度</param>
		/// <exception cref="ArgumentNullException">source 為 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">startIndex 小於 source 第一個維度的下限。<br />
		/// - 或 -length 小於零。
		/// </exception>
		/// <exception cref="ArgumentException">length 大於從 startIndex 到 source 結尾的元素數目。</exception>
		public UInt16 ComputeChecksum(byte[] source, UInt16 startIndex, UInt16 length)
		{
			byte[] tmp = new byte[length];
			Array.Copy(source, startIndex, tmp, 0, length);
			return ComputeChecksum(tmp);
		}
		#endregion

		#region Public Method : byte[] ComputeChecksumBytes(byte[] source, ushort startIndex, ushort length)
		/// <summary>計算CRC</summary>
		/// <param name="source">欲計算的位元組陣列</param>
		/// <param name="startIndex">開始計算的陣列索引值</param>
		/// <param name="length">欲計算的長度</param>
		/// <returns>計算後的 CRC16 位元組陣列值</returns>
		/// <exception cref="ArgumentNullException">source 為 null</exception>
		/// <exception cref="ArgumentOutOfRangeException">startIndex 小於 source 第一個維度的下限。<br />
		/// - 或 -length 小於零。
		/// </exception>
		/// <exception cref="ArgumentException">length 大於從 startIndex 到 source 結尾的元素數目。</exception>
		public byte[] ComputeChecksumBytes(byte[] source, ushort startIndex, ushort length)
		{
			byte[] tmp = new byte[length];
			Array.Copy(source, startIndex, tmp, 0, length);
			return ComputeChecksumBytes(tmp);
		}
		#endregion

	}
}
