using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace CJF.Utility
{
	/// <summary>自訂安全性驗證類別，使用資料加密標準 (System.Security.Cryptography.DES) 密碼編譯</summary>
	public class Security
	{
		#region Public Static Method : string Encrypt(string source, string key, string iv)
		/// <summary>
		/// 將字串資料以加密標準 (System.Security.Cryptography.DES) 密碼加以編譯並以Base64字串格式回傳
		/// </summary>
		/// <param name="source">欲加密的字串</param>
		/// <param name="key">對稱演算法所用的秘密金鑰。長度固定為 8Bytes。</param>
		/// <param name="iv">對稱演算法所用的初始化向量。長度固定為 8Bytes。</param>
		/// <exception cref="ArgumentOutOfRangeException">長度不足，各參數所需的長度為：
		/// <para>key、iv 固定為 8Bytes</para>
		/// </exception>
		/// <returns>加密後的Base64字串</returns>
		public static string Encrypt(string source, string key, string iv)
		{
			if (Encoding.UTF8.GetByteCount(key) != 8)
				throw new ArgumentOutOfRangeException("key 長度不足，至少需 8bytes");
			if (Encoding.UTF8.GetByteCount(iv) != 8)
				throw new ArgumentOutOfRangeException("iv 長度不足，至少需 8bytes");
			byte[] bytearrayinput = Encoding.Default.GetBytes(source);
			MemoryStream ms = new MemoryStream();

			DES des = new DESCryptoServiceProvider();

			CryptoStream cryptostream = new CryptoStream(ms, des.CreateEncryptor(GetRgb(key), GetRgb(iv)), CryptoStreamMode.Write);
			cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
			cryptostream.Close();
			return Convert.ToBase64String(ms.ToArray());
		}
		#endregion

		#region Public Static Method : string Decrypt(string source, string key, string iv)
		/// <summary>
		/// 將以Base64格式加密的字串資料以加密標準 (System.Security.Cryptography.DES) 密碼解密後回傳
		/// </summary>
		/// <param name="source">Base64加密字串</param>
		/// <param name="key">對稱演算法所用的秘密金鑰。長度固定為 8Bytes。</param>
		/// <param name="iv">對稱演算法所用的初始化向量。長度固定為 8Bytes。</param>
		/// <exception cref="ArgumentOutOfRangeException">長度不足，各參數所需的長度為：
		/// <para>source 最小為 12Bytes</para>
		/// <para>key、iv 固定為 8Bytes</para>
		/// </exception>
		/// <exception cref="ArgumentException">source 非加密字串</exception>
		/// <returns>解密後的字串</returns>
		public static string Decrypt(string source, string key, string iv)
		{
			if (source.Length < 12)
				throw new ArgumentOutOfRangeException("source 長度不足，至少需 12bytes");
			if (!source.EndsWith("="))
				throw new ArgumentException("source 非加密字串");
			if (Encoding.UTF8.GetByteCount(key) != 8)
				throw new ArgumentOutOfRangeException("key 長度不足，至少需 8bytes");
			if (Encoding.UTF8.GetByteCount(iv) != 8)
				throw new ArgumentOutOfRangeException("iv 長度不足，至少需 8bytes");
			if (source.Trim() != "")
			{
				try
				{
					byte[] bytearrayinput = Convert.FromBase64String(source);
					MemoryStream ms = new MemoryStream();

					DES des = new DESCryptoServiceProvider();

					CryptoStream cryptostream = new CryptoStream(ms, des.CreateDecryptor(GetRgb(key), GetRgb(iv)), CryptoStreamMode.Write);

					cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
					cryptostream.Close();
					return Encoding.Default.GetString(ms.ToArray());
				}
				catch (CryptographicException) { }
			}
			return "";
		}
		#endregion

		#region Private Static Method : byte[] GetRgb(string key)
		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		private static byte[] GetRgb(string key)
		{
			return Encoding.UTF8.GetBytes(key);
		}
		#endregion
	}
}
