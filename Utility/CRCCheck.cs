using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using CJF.Utility.Extensions;

namespace CJF.Utility.CRC
{
	#region Public Sealed Class : Crc16
	/*
	 * CRC16
	 * Modified From : https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs
	 */
	/// <summary>CRC-16 類別</summary>
	public sealed class Crc16 : HashAlgorithm
	{
		/// <summary>預設多項式</summary>
		public const ushort DefaultPolynomial = 0xA001;
		/// <summary>預設初始值。For CRC-16/IBM</summary>
		public const ushort DefaultSeed = 0x0000;
		/// <summary>Modbus 專用初始值。</summary>
		public const ushort ModbusSeed = 0xFFFF;

		static ushort[] defaultTable;
		readonly ushort[] table;
		readonly ushort seed;
		ushort hash;

		#region Construct Methods
		/// <summary>初始化 CJF.Utility.CRC.Crc16 類別的新執行個體。</summary>
		public Crc16() : this(DefaultPolynomial, DefaultSeed) { }
		/// <summary>初始化 CJF.Utility.CRC.Crc16 類別的新執行個體。</summary>
		/// <param name="polynomial">多項式</param>
		/// <param name="seed">初始值</param>
		public Crc16(ushort polynomial, ushort seed)
		{
			table = InitializeTable(polynomial);
			this.seed = seed;
		}
		#endregion

		#region Public Override Method : void Initialize()
		/// <summary>初始化 CJF.Utility.CRC.Crc16 類別的實作。</summary>
		public override void Initialize() { }
		#endregion

		#region Protected Override Method : void HashCore(byte[] array, int ibStart, int cbSize)
		/// <summary>計算 CRC 值</summary>
		/// <param name="array">要用來計算雜湊程式碼的輸入。</param>
		/// <param name="ibStart">位元組陣列中的座標，從此處開始使用資料。</param>
		/// <param name="cbSize">位元組陣列中要用作資料的位元組數目。</param>
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			hash = CalculateHash(table, this.seed, array, ibStart, cbSize);
		}
		#endregion

		#region Protected Override Method : byte[] HashFinal()
		/// <summary>在衍生類別中覆寫時，當密碼編譯資料流物件處理最後的資料後，會對雜湊計算做最後處理。</summary>
		/// <returns>計算出來的雜湊程式碼。</returns>
		protected override byte[] HashFinal()
		{
			var hashBuffer = ToBigEndianBytes(hash);
			HashValue = hashBuffer;
			return hashBuffer;
		}
		#endregion

		#region Public Override Property : int HashSize(R)
		/// <summary>取得計算出來的雜湊程式碼的大小，以位元為單位。</summary>
		public override int HashSize { get { return 16; } }
		#endregion

		#region Public Static Method : ushort Compute(byte[] buffer)
		/// <summary>計算 CRC 值</summary>
		/// <param name="buffer">欲計算的位元組陣列</param>
		/// <returns></returns>
		public static ushort Compute(byte[] buffer)
		{
			return CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
		}
		#endregion

		#region Public Static Method : ushort Compute(byte[] buffer, ushort polynomial)
		/// <summary>計算 CRC 值</summary>
		/// <param name="buffer">欲計算的位元組陣列</param>
		/// <param name="polynomial">多項式</param>
		/// <returns></returns>
		public static ushort Compute(byte[] buffer, ushort polynomial)
		{
			return CalculateHash(InitializeTable(polynomial), DefaultSeed, buffer, 0, buffer.Length);
		}
		#endregion

		#region Public Static Method : ushort Compute(byte[] buffer, ushort polynomial, ushort seed)
		/// <summary>計算 CRC 值</summary>
		/// <param name="buffer">欲計算的位元組陣列</param>
		/// <param name="polynomial">多項式</param>
		/// <param name="seed">初始值</param>
		/// <returns></returns>
		public static ushort Compute(byte[] buffer, ushort polynomial, ushort seed)
		{
			return CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
		}
		#endregion

		#region Private Static Method : ushort[] InitializeTable(ushort polynomial)
		/// <summary>初始化雜湊表</summary>
		/// <param name="polynomial">多項式</param>
		/// <returns></returns>
		private static ushort[] InitializeTable(ushort polynomial)
		{
			if (polynomial == DefaultPolynomial && defaultTable != null)
				return defaultTable;
			ushort value;
			ushort temp;
			ushort[] createTable = new ushort[256];
			for (ushort i = 0; i < createTable.Length; ++i)
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
				createTable[i] = value;
			}
			if (polynomial == DefaultPolynomial)
				defaultTable = createTable;

			return createTable;
		}
		#endregion

		#region Private Static Method : ushort CalculateHash(ushort[] table, ushort seed, IList<byte> buffer, int start, int size)
		private static ushort CalculateHash(ushort[] table, ushort seed, IList<byte> buffer, int start, int size)
		{
			ushort hash = seed;
			for (int i = start; i < start + size; ++i)
				hash = (ushort)((hash >> 8) ^ table[buffer[i] ^ hash & 0xff]);
			return hash;
		}
		#endregion

		#region Private Static Method : byte[] ToBigEndianBytes(ushort val)
		private static byte[] ToBigEndianBytes(ushort val)
		{
			var result = BitConverter.GetBytes(val);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(result);
			return result;
		}
		#endregion
	}
	#endregion

	#region Public Sealed Class : Crc32
	/*
	 * CRC32
	 * Modified From : https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs
	 */
	/// <summary>CRC-32 類別</summary>
	public sealed class Crc32 : HashAlgorithm
	{
		/// <summary>預設多項式</summary>
		public const uint DefaultPolynomial = 0xEDB88320u;
		/// <summary>預設初始值</summary>
		public const uint DefaultSeed = 0xFFFFFFFFu;

		static uint[] defaultTable;
		readonly uint seed;
		readonly uint[] table;
		uint hash;

		#region Construct Methods
		/// <summary>初始化 CJF.Utility.CRC.Crc32 類別的新執行個體。</summary>
		public Crc32() : this(DefaultPolynomial, DefaultSeed) { }
		/// <summary>初始化 CJF.Utility.CRC.Crc32 類別的新執行個體。</summary>
		/// <param name="polynomial">多項式</param>
		/// <param name="seed">初始值</param>
		public Crc32(uint polynomial, uint seed)
		{
			table = InitializeTable(polynomial);
			this.seed = hash = seed;
		}
		#endregion

		#region Public Override Method : void Initialize()
		/// <summary>初始化 CJF.Utility.CRC.Crc16 類別的實作。</summary>
		public override void Initialize()
		{
			hash = seed;
		}
		#endregion

		#region Protected Override Method : void HashCore(byte[] array, int ibStart, int cbSize)
		/// <summary>計算 CRC 值</summary>
		/// <param name="array">要用來計算雜湊程式碼的輸入。</param>
		/// <param name="ibStart">位元組陣列中的座標，從此處開始使用資料。</param>
		/// <param name="cbSize">位元組陣列中要用作資料的位元組數目。</param>
		protected override void HashCore(byte[] array, int ibStart, int cbSize)
		{
			hash = CalculateHash(table, hash, array, ibStart, cbSize);
		}
		#endregion

		#region Protected Override Method : byte[] HashFinal()
		/// <summary>在衍生類別中覆寫時，當密碼編譯資料流物件處理最後的資料後，會對雜湊計算做最後處理。</summary>
		/// <returns>計算出來的雜湊程式碼。</returns>
		protected override byte[] HashFinal()
		{
			var hashBuffer = ToBigEndianBytes(~hash);
			HashValue = hashBuffer;
			return hashBuffer;
		}
		#endregion

		#region Public Override Property : int HashSize(R)
		/// <summary>取得計算出來的雜湊程式碼的大小，以位元為單位。</summary>
		public override int HashSize { get { return 32; } }
		#endregion

		#region Public Static Method : uint Compute(byte[] buffer)
		/// <summary>計算 CRC 值</summary>
		/// <param name="buffer">欲計算的位元組陣列</param>
		/// <returns></returns>
		public static uint Compute(byte[] buffer)
		{
			return ~CalculateHash(InitializeTable(DefaultPolynomial), DefaultSeed, buffer, 0, buffer.Length);
		}
		#endregion

		#region Public Static Method : uint Compute(byte[] buffer, uint polynomial)
		/// <summary>計算 CRC 值</summary>
		/// <param name="buffer">欲計算的位元組陣列</param>
		/// <param name="polynomial">多項式</param>
		/// <returns></returns>
		public static uint Compute(byte[] buffer, uint polynomial)
		{
			return ~CalculateHash(InitializeTable(polynomial), DefaultSeed, buffer, 0, buffer.Length);
		}
		#endregion

		#region Public Static Method : uint Compute(byte[] buffer, uint polynomial, uint seed)
		/// <summary>計算 CRC 值</summary>
		/// <param name="buffer">欲計算的位元組陣列</param>
		/// <param name="polynomial">多項式</param>
		/// <param name="seed">初始值</param>
		/// <returns></returns>
		public static uint Compute(byte[] buffer, uint polynomial, uint seed)
		{
			return ~CalculateHash(InitializeTable(polynomial), seed, buffer, 0, buffer.Length);
		}
		#endregion

		#region Private Static Method : uint[] InitializeTable(uint polynomial)
		/// <summary>初始化雜湊表</summary>
		/// <param name="polynomial">多項式</param>
		/// <returns></returns>
		private static uint[] InitializeTable(uint polynomial)
		{
			if (polynomial == DefaultPolynomial && defaultTable != null)
				return defaultTable;

			var createTable = new uint[256];
			for (var i = 0; i < 256; i++)
			{
				var entry = (uint)i;
				for (var j = 0; j < 8; j++)
					if ((entry & 1) == 1)
						entry = (entry >> 1) ^ polynomial;
					else
						entry = entry >> 1;
				createTable[i] = entry;
			}

			if (polynomial == DefaultPolynomial)
				defaultTable = createTable;

			return createTable;
		}
		#endregion

		#region Private Method : uint CalculateHash(uint[] table, uint seed, IList<byte> buffer, int start, int size)
		private static uint CalculateHash(uint[] table, uint seed, IList<byte> buffer, int start, int size)
		{
			var hash = seed;
			for (var i = start; i < start + size; i++)
				hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];
			return hash;
		}
		#endregion

		#region Private Method : byte[] ToBigEndianBytes(uint uint32)
		private static byte[] ToBigEndianBytes(uint uint32)
		{
			var result = BitConverter.GetBytes(uint32);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(result);
			return result;
		}
		#endregion
	}
	#endregion
}
