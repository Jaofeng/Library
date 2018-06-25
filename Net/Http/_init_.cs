using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CJF.Net.Http
{
	#region Struct : ReceivedFileInfo
	/// <summary>
	/// 接收的檔案資料類別
	/// </summary>
	public struct ReceivedFileInfo
	{
		/// <summary>在 Form 中的鍵名</summary>
		public string FieldKey;
		/// <summary>原始檔案名稱</summary>
		public string FileName;
		/// <summary>包含完整路徑的暫存檔名</summary>
		public string FullPath;
		/// <summary>檔案種類</summary>
		public string ContentType;
		/// <summary>檔案長度</summary>
		public long Length;
	}
	#endregion

	#region Class : MyMemoryStream
	internal class MyMemoryStream : MemoryStream
	{
		public MyMemoryStream() : base() { }
		public MyMemoryStream(byte[] buffer) : base(buffer) { }
		public string ReadLine()
		{
			long oldPos = this.Position;
			while (this.Position < this.Length)
			{
				if (this.ReadByte() == 13 && this.ReadByte() == 10)
					break;
			}
			byte[] buf = new byte[(int)(this.Position - oldPos - 2)];
			this.Position = oldPos;
			this.Read(buf, 0, buf.Length);
			this.Position += 2;
			return Encoding.UTF8.GetString(buf);
		}
	}
	#endregion

	#region Public Class : APIFormatException
	/// <summary>WebAPI格式錯誤類別</summary>
	public class APIFormatException : Exception
	{
		/// <summary>建立新的WebAPI格式錯誤類別</summary>
		/// <param name="message">錯誤訊息</param>
		public APIFormatException(string message) : base(message) { }
		/// <summary>建立新的WebAPI格式錯誤類別</summary>
		/// <param name="message">錯誤訊息</param>
		/// <param name="innerException">子錯誤類別</param>
		public APIFormatException(string message, Exception innerException) : base(message, innerException) { }
	}
	#endregion

	#region Public Class : APIMissingFieldException
	/// <summary>缺少必要欄位的錯誤類別</summary>
	public class APIMissingFieldException : Exception
	{
		/// <summary>產生缺少必要欄位的錯誤</summary>
		public APIMissingFieldException(string field) : base("缺少必要欄位!") { this.FieldName = field; }
		/// <summary>必要欄位名稱</summary>
		public string FieldName { get; private set; }
	}
	#endregion

}
