using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using CJF.Utility;

namespace CJF.Net.Http
{
	#region Struct : ReceivedFileInfo
	/// <summary>
	/// 接收的檔案資料類別
	/// </summary>
	public struct ReceivedFileInfo
	{
		/// <summary>原始檔案名稱</summary>
		public string FileName;
		/// <summary>暫存檔路徑</summary>
		public string TempFile;
		/// <summary>檔案種類</summary>
		public string ContentType;
		/// <summary>檔案長度</summary>
		public long Length;
	}
	#endregion

	#region Class : MyMemoryStream
	internal class MyMemoryStream : MemoryStream
	{
		public MyMemoryStream(byte[] buffer) : base(buffer) { }
		public string ReadLine()
		{
			long oldPos = this.Position;
			while (this.Position < this.Length)
			{
				if (this.ReadByte() == 13 && this.ReadByte() == 10)
					break;
			}
			string result = Encoding.UTF8.GetString(this.ToArray(), (int)oldPos, (int)(this.Position - oldPos));
			return result.Trim("\r\n".ToCharArray());
		}
	}
	#endregion

	/// <summary>HTTP 連線服務類別</summary>
	[Serializable]
	public class HttpServiceBase : IDisposable
	{
		LogManager _log = new LogManager(typeof(HttpServiceBase));
		bool isDisposed = false;
		bool _SendMail = false;
		Random rndKey = new Random(DateTime.Now.Millisecond);

		/// <summary>取得HttpListenerContext類別</summary>
		public HttpListenerContext Context { get; protected set; }
		/// <summary>取得遠端IP資訊</summary>
		public IPEndPoint ClientPoint { get { return this.Context.Request.RemoteEndPoint; } }
		/// <summary>取得接收的檔案資訊</summary>
		public List<ReceivedFileInfo> ReceivedFiles { get; protected set; }
		/// <summary>設定或取得。當錯誤發生時，除記錄至事件檔外，是否發送Mail</summary>
		public bool SendMailWhenException { get { return _SendMail; } set { _SendMail = value; } }

		#region Construct Method : HttpService(HttpListenerContext context)
		/// <summary></summary>
		public HttpServiceBase() { }
		/// <summary></summary>
		/// <param name="context"></param>
		public HttpServiceBase(HttpListenerContext context)
		{
			this.Context = context;
		}
		/// <summary></summary>
		~HttpServiceBase()
		{
			Dispose(false);
		}
		#endregion

		#region Public Virtual Method : void ProcessRequest()
		/// <summary>[覆寫] 接收到 Http Request 時的處理流程</summary>
		public virtual void ProcessRequest()
		{
			HttpListenerRequest request = this.Context.Request;
			string rawUrl = request.RawUrl;
			string pageUrl = rawUrl.Split('?')[0].TrimStart('/');
			// [Remote IP] - [Method] [RawUrl] [HTTP/Ver] - [User Agent]
			string format = "{0} - {1} {2} HTTP/{3}.{4} - {5}";
			_log.Write(LogManager.LogLevel.Debug, format, request.RemoteEndPoint, request.HttpMethod, rawUrl, request.ProtocolVersion.Major, request.ProtocolVersion.Minor, request.UserAgent);
			NameValueCollection queryString = request.QueryString;
			switch (request.HttpMethod.ToUpper())
			{
				case "HEAD":
					HttpHeadMethod(pageUrl, queryString); break;
				case "GET":
					HttpGetMethod(pageUrl, queryString); break;
				case "POST":
					HttpPostMethod(pageUrl, queryString); break;
				case "PUT":
					if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
						HttpPutMethod(pageUrl, queryString);
					else
						ResponseMethodNotAllowed();
					break;
				case "DELETE":
					if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
						HttpDeleteMethod(pageUrl, queryString);
					else
						ResponseMethodNotAllowed();
					break;
				case "PATCH":
					if (request.ProtocolVersion.Major >= 1 && request.ProtocolVersion.Minor >= 1)
						HttpPatchMethod(pageUrl, queryString);
					else
						ResponseMethodNotAllowed();
					break;
				default:
					ResponseServiceNotSupport(); break;
			}
		}
		#endregion

		#region Protected Virtual Method : void HttpHeadMethod(string path, NameValueCollection queryString)
		/// <summary>[覆寫]HEAD 模式</summary>
		/// <param name="path">網址路徑</param>
		/// <param name="queryString">要求中所包含的查詢字串。</param>
		protected virtual void HttpHeadMethod(string path, NameValueCollection queryString)
		{
			string file = path.Replace("/", "\\").ToLower();
			file = Path.Combine("Web", file);
			ResponseHead(file);
		}
		#endregion

		#region Protected Virtual Method : void HttpGetMethod(string path, NameValueCollection queryString)
		/// <summary>[覆寫]GET 模式</summary>
		/// <param name="path">網址路徑</param>
		/// <param name="queryString">要求中所包含的查詢字串。</param>
		protected virtual void HttpGetMethod(string path, NameValueCollection queryString)
		{
			string file = path.Replace("/", "\\").ToLower();
			file = Path.Combine("Web", file);
			ResponseFile(file);
		}
		#endregion

		#region Protected Virtual Method : void HttpPostMethod(string path, NameValueCollection queryString)
		/// <summary>[覆寫]POST 模式</summary>
		/// <param name="path">網址路徑</param>
		/// <param name="queryString">要求中所包含的查詢字串。</param>
		protected virtual void HttpPostMethod(string path, NameValueCollection queryString)
		{
			try
			{
				//string svc = path.Replace("/", "\\").ToLower().TrimEnd('\\').Split('\\')[0];
				HttpListenerRequest request = this.Context.Request;
				NameValueCollection nvc = null;
				this.ReceivedFiles = null;
				if (request.ContentType == "application/x-www-form-urlencoded")
				{
					StreamReader reader = new StreamReader(request.InputStream);
					string data = reader.ReadToEnd();
					//_log.Write(LogManager.LogLevel.Debug, "Service:{0}:{1}", svc, data);
					nvc = System.Web.HttpUtility.ParseQueryString(data);
					if (queryString != null && queryString.Count != 0)
					{
						foreach (KeyValuePair<string, string> kv in queryString)
						{
							if (nvc.AllKeys.Contains<string>(kv.Key))
								nvc[kv.Key] += ";" + kv.Value;
							else
								nvc.Add(kv.Key, kv.Value);
						}
					}
					ReceivedAPI(path, nvc);
				}
				else if (request.ContentType.StartsWith("multipart/form-data"))
				{
					PopulatePostMultiPart(request, out nvc);
					if (queryString != null && queryString.Count != 0)
					{
						foreach (KeyValuePair<string, string> kv in queryString)
						{
							if (nvc.AllKeys.Contains<string>(kv.Key))
								nvc[kv.Key] += ";" + kv.Value;
							else
								nvc.Add(kv.Key, kv.Value);
						}
					}
					List<string> arr = this.ReceivedFiles.ConvertAll<string>(rfi => rfi.FileName);
					//_log.Write(LogManager.LogLevel.Debug, "Service:{0},Files:{1}", svc, string.Join(",", arr.ToArray()));
					ReceivedAPI(path, nvc);
				}
			}
			catch (Exception ex)
			{
				_log.Write(LogManager.LogLevel.Debug, "From:HttpPostMethod:{0}", path);
				_log.WriteException(ex, _SendMail);
			}
		}
		#endregion

		#region Protected Virtual Method : void HttpPutMethod(string path, NameValueCollection queryString)
		/// <summary>[覆寫]PUT 模式</summary>
		/// <param name="path">網址路徑</param>
		/// <param name="queryString">要求中所包含的查詢字串。</param>
		protected virtual void HttpPutMethod(string path, NameValueCollection queryString)
		{
			_log.Write(LogManager.LogLevel.Debug, "PUT {0}", path);
			string msg = "No Support PUT method!!";
			byte[] buffer = this.Context.Request.ContentEncoding.GetBytes(msg);
			ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, msg);
		}
		#endregion

		#region Protected Virtual Method : void HttpDeleteMethod(string path, NameValueCollection queryString)
		/// <summary>[覆寫]DELETE 模式</summary>
		/// <param name="path">網址路徑</param>
		/// <param name="queryString">要求中所包含的查詢字串。</param>
		protected virtual void HttpDeleteMethod(string path, NameValueCollection queryString)
		{
			_log.Write(LogManager.LogLevel.Debug, "DELETE {0}", path);
			string msg = "No Support DELETE method!!";
			byte[] buffer = this.Context.Request.ContentEncoding.GetBytes(msg);
			ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, msg);
		}
		#endregion

		#region Protected Virtual Method : void HttpPatchMethod(string path, NameValueCollection queryString)
		/// <summary>[覆寫]PATCH 模式</summary>
		/// <param name="path">網址路徑</param>
		/// <param name="queryString">要求中所包含的查詢字串。</param>
		protected virtual void HttpPatchMethod(string path, NameValueCollection queryString)
		{
			_log.Write(LogManager.LogLevel.Debug, "PATCH {0}", path);
			string msg = "No Support PATCH method!!";
			byte[] buffer = this.Context.Request.ContentEncoding.GetBytes(msg);
			ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, msg);
		}
		#endregion

		#region Response Methods
		#region Public Virtual Method : bool ResponseHead(string fileName)
		/// <summary>傳送檔案資訊至終端</summary>
		/// <param name="fileName">檔案路徑</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseHead(string fileName)
		{
			bool result = false;
			if (!File.Exists(fileName))
			{
				this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
				this.Context.Response.OutputStream.Close();
				result = true;
			}
			else
			{
				try
				{
					using (FileStream fs = File.OpenRead(fileName))
					{
						this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
						this.Context.Response.ContentLength64 = fs.Length;
						string mime = ConvUtils.GetContentType(fileName);
						this.Context.Response.ContentType = mime;
						if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
							this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
					}
					this.Context.Response.OutputStream.Close();
					result = true;
				}
				catch (HttpListenerException ex)
				{
					if (ex.ErrorCode == 64)
						_log.Write(LogManager.LogLevel.Debug, "Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
					else
					{
						_log.Write(LogManager.LogLevel.Debug, "From:ResponseFile");
						_log.WriteException(ex, _SendMail);
					}
				}
				catch (Exception ex)
				{
					_log.Write(LogManager.LogLevel.Debug, "From:ResponseFile");
					_log.WriteException(ex, _SendMail);
				}
				finally
				{
					try
					{
						if (this.Context.Response != null)
							this.Context.Response.Close();
					}
					catch { }
				}
			}
			return result;
		}
		#endregion

		#region Public Virtual Method : bool ResponseFile(string fileName)
		/// <summary>傳送檔案至終端</summary>
		/// <param name="fileName">檔案路徑</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseFile(string fileName)
		{
			return ResponseFile(fileName, 0, 8192);
		}
		#endregion

		#region Public Virtual Method : bool ResponseFile(string fileName, int speed)
		/// <summary>傳送檔案至終端</summary>
		/// <param name="fileName">檔案路徑</param>
		/// <param name="speed">傳輸暫停時間，單位豪秒。本值越短，傳輸速度越快</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseFile(string fileName, int speed)
		{
			return ResponseFile(fileName, speed, 8192);
		}
		#endregion

		#region Public Virtual Method : bool ResponseFile(string fileName, int speed, int packageSize)
		/// <summary>傳送檔案至終端</summary>
		/// <param name="fileName">檔案路徑</param>
		/// <param name="speed">傳輸暫停時間，單位豪秒。本值越短，傳輸速度越快</param>
		/// <param name="packageSize">傳輸的封包大小，單位Bytes</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseFile(string fileName, int speed, int packageSize)
		{
			bool result = false;
			if (!File.Exists(fileName))
				result = ResponseNotFound();
			else
			{
				try
				{
					using (FileStream fs = File.OpenRead(fileName))
					{
						this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
						this.Context.Response.ContentLength64 = fs.Length;
						string mime = ConvUtils.GetContentType(fileName);
						this.Context.Response.ContentType = mime;

						if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
							this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
						byte[] buffer = new byte[packageSize];
						int read;
						while ((read = fs.Read(buffer, 0, packageSize)) > 0)
						{
							this.Context.Response.OutputStream.Write(buffer, 0, read);
							if (speed != 0)
								System.Threading.Thread.Sleep(speed);
						}
					}
					this.Context.Response.OutputStream.Close();
					result = true;
				}
				catch (HttpListenerException ex)
				{
					if (ex.ErrorCode == 64)
						_log.Write(LogManager.LogLevel.Debug, "Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
					else
					{
						_log.Write(LogManager.LogLevel.Debug, "From:ResponseFile");
						_log.WriteException(ex, _SendMail);
					}
				}
				catch (Exception ex)
				{
					_log.Write(LogManager.LogLevel.Debug, "From:ResponseFile");
					_log.WriteException(ex, _SendMail);
				}
				finally
				{
					try
					{
						if (this.Context.Response != null)
							this.Context.Response.Close();
					}
					catch { }
				}
			}
			return result;
		}
		#endregion

		#region Public Virtual Method : bool ResponseFile(MemoryStream stream, string fileName)
		/// <summary>傳送檔案至終端</summary>
		/// <param name="stream">檔案串流</param>
		/// <param name="fileName">檔案路徑</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseFile(MemoryStream stream, string fileName)
		{
			return ResponseFile(stream, fileName, 10, 8192);
		}
		#endregion

		#region Public Virtual Method : bool ResponseFile(MemoryStream stream, string fileName, int speed)
		/// <summary>傳送檔案至終端</summary>
		/// <param name="stream">檔案串流</param>
		/// <param name="fileName">檔案路徑</param>
		/// <param name="speed">傳送間隔，單位豪秒，值越大速度越慢</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseFile(MemoryStream stream, string fileName, int speed)
		{
			return ResponseFile(stream, fileName, speed, 8192);
		}
		#endregion

		#region Public Virtual Method : bool ResponseFile(MemoryStream stream, string fileName, int speed, int packageSize)
		/// <summary>傳送檔案至終端</summary>
		/// <param name="stream">檔案串流</param>
		/// <param name="fileName">檔案路徑</param>
		/// <param name="speed">傳送間隔，單位豪秒，值越大速度越慢</param>
		/// <param name="packageSize">傳輸的封包大小，單位Bytes，預設為8192</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseFile(MemoryStream stream, string fileName, int speed, int packageSize)
		{
			bool result = false;
			if (stream == null || stream.Length == 0)
				result = ResponseNotFound();
			else
			{
				try
				{
					this.Context.Response.StatusCode = (int)System.Net.HttpStatusCode.OK;
					this.Context.Response.ContentLength64 = stream.Length;
					string mime = ConvUtils.GetContentType(fileName);
					this.Context.Response.ContentType = mime;
					if (mime.Equals("application/octetstream", StringComparison.OrdinalIgnoreCase))
						this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + HttpUtility.UrlEncode(Path.GetFileName(fileName)));
					byte[] buffer = new byte[packageSize];
					int read;
					if (speed != 0)
					{
						while ((read = stream.Read(buffer, 0, packageSize)) > 0)
						{
							this.Context.Response.OutputStream.Write(buffer, 0, read);
							System.Threading.Thread.Sleep(speed);
						}
					}
					else
					{
						while ((read = stream.Read(buffer, 0, packageSize)) > 0)
							this.Context.Response.OutputStream.Write(buffer, 0, read);
					}
					this.Context.Response.OutputStream.Close();
					result = true;
				}
				catch (HttpListenerException ex)
				{
					if (ex.ErrorCode == 64)
						_log.Write(LogManager.LogLevel.Debug, "Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
					else
					{
						_log.Write(LogManager.LogLevel.Debug, "From:ResponseFile");
						_log.WriteException(ex, _SendMail);
					}
				}
				catch (Exception ex)
				{
					_log.Write(LogManager.LogLevel.Debug, "From:ResponseFile");
					_log.WriteException(ex, _SendMail);
				}
				finally
				{
					try
					{
						if (this.Context.Response != null)
							this.Context.Response.Close();
					}
					catch { }
				}
			}
			return result;
		}
		#endregion

		#region Public Virtual Method : bool ResponseString(string msg, int statusCode)
		/// <summary>傳送文字訊息給終端</summary>
		/// <param name="msg">訊息內容</param>
		/// <param name="statusCode">HTTP 狀態碼</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseString(string msg, int statusCode)
		{
			this.Context.Response.ContentEncoding = Encoding.UTF8;
			return ResponseBinary(Encoding.UTF8.GetBytes(msg), statusCode);
		}
		#endregion

		#region Public Virtual Method : bool ResponseString(string msg)
		/// <summary>傳送文字訊息給終端</summary>
		/// <param name="msg">訊息內容</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseString(string msg)
		{
			this.Context.Response.ContentEncoding = Encoding.UTF8;
			return ResponseBinary(Encoding.UTF8.GetBytes(msg), (int)System.Net.HttpStatusCode.OK);
		}
		#endregion

		#region Public Virtual Method : bool ResponseString(string msg, Encoding encoding)
		/// <summary>傳送文字訊息給終端</summary>
		/// <param name="msg">訊息內容</param>
		/// <param name="encoding">字元編碼方式</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseString(string msg, Encoding encoding)
		{
			this.Context.Response.ContentEncoding = encoding;
			return ResponseBinary(encoding.GetBytes(msg), (int)System.Net.HttpStatusCode.OK);
		}
		#endregion

		#region Public Virtual Method : bool ResponseBitmap(Bitmap bitmap)
		/// <summary>繪製 Bitmap 資料至終端</summary>
		/// <param name="bitmap">Bitmap 圖像資料</param>
		/// <returns></returns>
		public virtual bool ResponseBitmap(Bitmap bitmap)
		{
			return ResponseBitmap(bitmap, ImageFormat.Jpeg);
		}
		#endregion

		#region Public Virtual Method : bool ResponseBitmap(Bitmap bitmap, ImageFormat format)
		/// <summary>繪製 Bitmap 資料至終端</summary>
		/// <param name="bitmap">Bitmap 圖像資料</param>
		/// <param name="format">圖檔格式</param>
		/// <returns></returns>
		public virtual bool ResponseBitmap(Bitmap bitmap, ImageFormat format)
		{
			bool result = false;
			try
			{
				if (bitmap != null)
				{
					if (format == ImageFormat.Jpeg)
					{
						ImageCodecInfo info = GetImageEncoder(format);
						EncoderParameters eps = new EncoderParameters(1);
						EncoderParameter ep = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 100L);
						eps.Param[0] = ep;
						bitmap.Save(this.Context.Response.OutputStream, info, eps);
						this.Context.Response.OutputStream.Close();
					}
					else
						bitmap.Save(this.Context.Response.OutputStream, format);
				}
				else
				{
					ResponseNotFound();
				}
				result = true;
			}
			catch (HttpListenerException ex)
			{
				if (ex.ErrorCode == 64)
				{
					_log.Write(LogManager.LogLevel.Debug, "Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
					result = false;
				}
				else
				{
					_log.Write(LogManager.LogLevel.Debug, "From:ResponseBitmap");
					_log.WriteException(ex, _SendMail);
				}
			}
			catch (Exception ex)
			{
				_log.Write(LogManager.LogLevel.Debug, "From:ResponseBitmap");
				_log.WriteException(ex, _SendMail);
			}
			finally
			{
				if (this.Context != null && this.Context.Response != null)
					this.Context.Response.Close();
			}
			return result;
		}
		#endregion

		#region Public Virtual Method : bool ResponseBitmap(Bitmap bitmap, string fileName, ImageFormat format)
		/// <summary>繪製 Bitmap 資料至終端</summary>
		/// <param name="bitmap">Bitmap 圖像資料</param>
		/// <param name="fileName">檔案名稱</param>
		/// <param name="format">圖檔格式</param>
		/// <returns></returns>
		public virtual bool ResponseBitmap(Bitmap bitmap, string fileName, ImageFormat format)
		{
			this.Context.Response.AddHeader("content-disposition", "attachment;filename=" + System.Web.HttpUtility.UrlEncode(fileName));
			return ResponseBitmap(bitmap, format);
		}
		#endregion

		#region Public Virtual Method : bool ResponseBinary(byte[] buffer, int statusCode)
		/// <summary>傳送位元組資料給終端</summary>
		/// <param name="buffer">位元組陣列資料內容</param>
		/// <param name="statusCode">HTTP 狀態碼</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseBinary(byte[] buffer, int statusCode)
		{
			return ResponseBinary(buffer, statusCode, null);
		}
		#endregion

		#region Public Virtual Method : bool ResponseBinary(byte[] buffer, int statusCode, string description)
		/// <summary>傳送位元組資料給終端</summary>
		/// <param name="buffer">位元組陣列資料內容</param>
		/// <param name="statusCode">HTTP 狀態碼</param>
		/// <param name="description">狀態碼說明</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseBinary(byte[] buffer, int statusCode, string description)
		{
			bool result = false;
			try
			{
				this.Context.Response.StatusCode = statusCode;
				if (!string.IsNullOrEmpty(description))
					this.Context.Response.StatusDescription = description;
				if (buffer != null)
				{
					this.Context.Response.ContentLength64 = buffer.Length;
					this.Context.Response.OutputStream.Write(buffer, 0, buffer.Length);
				}
				else
					this.Context.Response.ContentLength64 = 0;
				this.Context.Response.OutputStream.Close();
				result = true;
			}
			catch (HttpListenerException ex)
			{
				if (ex.ErrorCode == 64)
				{
					_log.Write(LogManager.LogLevel.Debug, "Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
					result = false;
				}
				else
				{
					_log.Write(LogManager.LogLevel.Debug, "From:ResponseBinary");
					_log.WriteException(ex, _SendMail);
				}
			}
			catch (Exception ex)
			{
				_log.Write(LogManager.LogLevel.Debug, "From:ResponseBinary");
				_log.WriteException(ex, _SendMail);
			}
			finally
			{
				if (this.Context.Response != null)
				{
					try { this.Context.Response.Close(); }
					catch { }
				}
			}
			return result;
		}
		#endregion

		#region Public Virtual Method : bool ResponseException(Exception ex)
		/// <summary>將錯誤訊息送給終端</summary>
		/// <param name="ex">例外處理類別</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseException(Exception ex)
		{
			StringBuilder sb = new StringBuilder(ex.Message.Replace("\r", "").Replace("\n", "<br />"));
			sb.AppendFormat("<br />{0}<br />", "=".PadLeft(40, '='));
			if (ex.StackTrace != null)
				sb.AppendFormat("{0}<br />", ex.StackTrace.Replace("\r", "").Replace("\n", "<br />"));
			if (ex.InnerException != null)
			{
				sb.AppendFormat("{0}<br />", "-".PadLeft(40, '-'));
				sb.AppendFormat("InnerException<br />");
				sb.AppendFormat("{0}<br />", ex.InnerException.Message.Replace("\r", "").Replace("\n", "<br />"));
				if (ex.InnerException.StackTrace != null)
				{
					sb.AppendFormat("StackTrace<br />");
					sb.AppendFormat("{0}<br />", ex.InnerException.StackTrace.Replace("\r", "").Replace("\n", "<br />"));
				}
			}
			if (File.Exists("Web\\Exception.htm"))
			{
				string content = ReadHtmlFile("Web\\Exception.htm");
				content = content.Replace("<!-- #ERROR_MSG# -->", sb.ToString());
				return ResponseString(content);
			}
			else
				return ResponseString(sb.ToString(), (int)System.Net.HttpStatusCode.InternalServerError);
		}
		#endregion

		#region Public Virtual Method : bool ResponseXML(string xml)
		/// <summary>傳送 XML 內容給終端，如ContentType未設定，則將會指定為application/xhtml+xml</summary>
		/// <param name="xml">XML 資料內容</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseXML(string xml)
		{
			try
			{
				// XML 強制使用 UTF-8 轉碼，不可再異動 By JF @ 2012/04/18
				this.Context.Response.ContentEncoding = Encoding.UTF8;
				if (this.Context.Response.ContentType == null)
					this.Context.Response.ContentType = "application/xhtml+xml";
				return ResponseBinary(Encoding.UTF8.GetBytes(xml), (int)System.Net.HttpStatusCode.OK);
			}
			catch (Exception ex)
			{
				_log.Write(LogManager.LogLevel.Debug, "From:ResponseXML");
				_log.WriteException(ex, _SendMail);
				return false;
			}
		}
		#endregion

		#region Public Virtual Method : bool ResponseNotFound()
		/// <summary>傳送「頁面不存在訊息」給終端, HTTP Error Code:404</summary>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseNotFound()
		{
			return ResponseString("File Not Found !", (int)System.Net.HttpStatusCode.NotFound);
		}
		#endregion

		#region Public Virtual Method : bool ResponseServiceNotSupport()
		/// <summary>傳送「不支援此服務」給終端, HTTP Error Code:501</summary>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseServiceNotSupport()
		{
			byte[] buffer = this.Context.Request.ContentEncoding.GetBytes("Service Not Support!!");
			return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.NotImplemented, "Service Not Support!!");
		}
		#endregion

		#region Public Virtual Method : bool ResponseMethodNotAllowed()
		/// <summary>傳送「不支援此 Method」給終端, HTTP Error Code:405</summary>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseMethodNotAllowed()
		{
			return ResponseBinary(null, (int)System.Net.HttpStatusCode.MethodNotAllowed);
		}
		#endregion

		#region Public Virtual Method : bool ResponseUnauthorized()
		/// <summary>傳送「未獲得授權或未認證」給終端, HTTP Error Code:401</summary>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseUnauthorized()
		{
			byte[] buffer = this.Context.Request.ContentEncoding.GetBytes("Unauthorized!!");
			return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Unauthorized, "Unauthorized!!");
		}
		#endregion

		#region Public Virtual Method : bool ResponseNotAllowIP()
		/// <summary>傳送「不允許的 IP 位址連線」給終端, HTTP Error Code:403</summary>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseNotAllowIP()
		{
			byte[] buffer = this.Context.Request.ContentEncoding.GetBytes("Not Allow IP!!");
			return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.Forbidden, "Not Allow IP!!");
		}
		#endregion

		#region Public Virtual Method : bool ResponseAPIError()
		/// <summary>傳送「API錯誤」給終端, HTTP Error Code:400</summary>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseAPIError()
		{
			return ResponseAPIError("API Error!!");
		}
		#endregion

		#region Public Virtual Method : bool ResponseAPIError(string msg)
		/// <summary>傳送「API錯誤」給終端, HTTP Error Code:400</summary>
		/// <param name="msg">錯誤訊息</param>
		/// <returns>是否正確傳送</returns>
		public virtual bool ResponseAPIError(string msg)
		{
			byte[] buffer = Encoding.UTF8.GetBytes(msg);
			return ResponseBinary(buffer, (int)System.Net.HttpStatusCode.BadRequest, "API Error!!");
		}
		#endregion
		#endregion

		#region Protected Virtual Method : void ReceivedAPI(string svc, NameValueCollection nvc)
		/// <summary>[覆寫]收到 API 時的處理函示</summary>
		/// <param name="svc">服務代碼</param>
		/// <param name="nvc">自 QueryString 取得的資料</param>
		protected internal virtual void ReceivedAPI(string svc, NameValueCollection nvc)
		{
			ResponseException(new NotImplementedException());
		}
		#endregion

		#region Protected Virtual Method : string ToQueryString(NameValueCollection nvc)
		/// <summary>[覆寫]將 NameValueCollection 類別中的值轉成 QueryString 格式</summary>
		/// <param name="nvc">Key-Value對應的類別</param>
		/// <returns></returns>
		protected virtual string ToQueryString(NameValueCollection nvc)
		{
			if (nvc == null || nvc.AllKeys.Length == 0)
				return string.Empty;
			var array = (from key in nvc.AllKeys
						 from value in nvc.GetValues(key)
						 select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
				.ToArray();
			return "?" + string.Join("&", array);
		}
		#endregion

		#region IDisposable 成員
		/// <summary>清除並釋放 HttpServiceBase 所使用的資源。</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>[覆寫] 清除並釋放 HttpServiceBase 所使用的資源。</summary>
		/// <param name="disposing">是否確實清除</param>
		protected virtual void Dispose(bool disposing)
		{
			if (isDisposed) return;
			if (disposing)
			{
				this.Context = null;
			}
			isDisposed = true;
		}
		#endregion

		#region Protected Method : void PopulatePostMultiPart(HttpListenerRequest request, out NameValueCollection nvc)
		/// <summary>拆解 Request 內容</summary>
		/// <param name="request">欲拆解的 HttpListenerRequest 類別</param>
		/// <param name="nvc">輸出成 NameValueCollection 類別</param>
		protected void PopulatePostMultiPart(HttpListenerRequest request, out NameValueCollection nvc)
		{
			this.ReceivedFiles = new List<ReceivedFileInfo>();
			nvc = new NameValueCollection();
			int boundary_index = request.ContentType.IndexOf("boundary=") + 9;
			string split = request.ContentType.Substring(boundary_index, request.ContentType.Length - boundary_index);
			byte[] splitBytes = Encoding.UTF8.GetBytes(split);
			string line = string.Empty;
			int no = 0;
			string[] arr1 = null;
			string[] arr2 = null;
			byte[] buffer = new byte[request.ContentLength64];
			request.InputStream.Read(buffer, 0, buffer.Length);
			MyMemoryStream ms = new MyMemoryStream(buffer);
			StreamReader sr = new StreamReader(new MemoryStream(buffer));
			string data = sr.ReadToEnd();
			while (ms.Position < ms.Length)
			{
				line = ms.ReadLine();
				if (line.IndexOf(split) != -1)
				{
					no = 0;
					continue;
				}
				if (no == 0)
				{
					if (line.StartsWith("Content-Disposition"))
					{
						arr1 = line.Split(';');
						if (arr1[0].Split(':')[1].Trim() != "form-data")
							continue;
						arr2 = arr1[1].Trim().Split('=');
						if (arr2[0].ToLower() != "name")
							continue;
						string key = arr2[1].Trim('"');
						if (line.IndexOf("filename=", StringComparison.OrdinalIgnoreCase) != -1)
						{
							line = ms.ReadLine();	// ContentType
							if (!line.StartsWith("Content-Type"))
								continue;
							string ct = line.Split(':')[1].Trim();
							string fn = arr1[2].Trim().Split('=')[1].Trim('"');
							if (nvc.AllKeys.Contains<string>(key))
								nvc[key] += ';' + fn;
							else
								nvc.Add(key, fn);
							line = ms.ReadLine();	// 捨棄空行

							int length = 0;
							int idx = ConvUtils.IndexOfBytes(buffer, splitBytes, (int)ms.Position);
							byte[] buf = null;
							if (idx != -1)
							{
								buf = new byte[idx - ms.Position - 4];
								length = ms.Read(buf, 0, buf.Length);
								string tmp = Path.GetTempFileName();
								File.WriteAllBytes(tmp, buf);
								this.ReceivedFiles.Add(new ReceivedFileInfo()
								{
									FileName = fn,
									ContentType = ct,
									Length = buf.Length,
									TempFile = tmp
								});
								ms.Position += splitBytes.Length + 6;
								no = 0;
								continue;
							}
						}
						else
						{
							StringBuilder sb = new StringBuilder();
							line = ms.ReadLine();	// 捨棄空行
							line = ms.ReadLine();
							while (ms.Position < ms.Length && line.IndexOf(split) == -1)
							{
								sb.AppendLine(line);
								line = ms.ReadLine();
							}
							if (Array.IndexOf<string>(nvc.AllKeys, key) == -1)
								nvc.Add(key, sb.ToString());
							else
								nvc[key] += ';' + sb.ToString();
							if (line.IndexOf(split) != -1)
							{
								no = 0;
								continue;
							}
						}
					}
				}
				no++;
			}
		}
		#endregion

		#region Public Virtual Method : void RedirectURL(string url)
		/// <summary>將回應設定為重新導向用戶端至指定的 URL。</summary>
		/// <param name="url">用戶端應用來尋找所要求之資源的 URL</param>
		public virtual void RedirectURL(string url)
		{
			try
			{
				this.Context.Response.Redirect(url);
			}
			catch (HttpListenerException ex)
			{
				if (ex.ErrorCode == 64)
				{
					_log.Write(LogManager.LogLevel.Debug, "Remote Disconnected:{0}", this.Context.Request.RemoteEndPoint);
				}
				else
				{
					_log.Write(LogManager.LogLevel.Debug, "From:RedirectURL");
					_log.WriteException(ex, _SendMail);
				}
			}
			catch (Exception ex)
			{
				_log.Write(LogManager.LogLevel.Debug, "From:RedirectURL");
				_log.WriteException(ex, _SendMail);
			}
			finally
			{
				if (this.Context.Response != null)
				{
					try { this.Context.Response.Close(); }
					catch { }
				}
			}
		}
		#endregion

		#region Public Static Method : NameValueCollection ViewStateToNameValueCollection(string viewState)
		/// <summary>將頁面中的VIEWSTATE資料轉為鍵值索引集合</summary>
		/// <param name="viewState">VIEWSTATE字串</param>
		/// <returns>鍵值索引集合</returns>
		public static NameValueCollection ViewStateToNameValueCollection(string viewState)
		{
			if (viewState == "##VIEWSTATE##")
				return new NameValueCollection();
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			byte[] buffer = Convert.FromBase64String(viewState);
			System.IO.MemoryStream _memory = new System.IO.MemoryStream(buffer);
			return (NameValueCollection)formatter.Deserialize(_memory);
		}
		#endregion

		#region Public Static Method : string NameValueCollectionToViewState(NameValueCollection nvc)
		/// <summary>
		/// 將鍵值索引集合轉為VIEWSTATE資料
		/// </summary>
		/// <param name="nvc">鍵值索引集合</param>
		/// <returns>VIEWSTATE字串資料</returns>
		public static string NameValueCollectionToViewState(NameValueCollection nvc)
		{
			System.IO.MemoryStream _memory = new System.IO.MemoryStream();
			System.Runtime.Serialization.Formatters.Binary.BinaryFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
			formatter.Serialize(_memory, nvc);
			_memory.Position = 0;
			byte[] read = new byte[_memory.Length];
			_memory.Read(read, 0, read.Length);
			_memory.Close();
			return Convert.ToBase64String(read);
		}
		#endregion

		#region Public Static Method : string ReadHtmlFile(string fileName)
		/// <summary>依檔案編碼方式讀取 Html 檔</summary>
		/// <param name="fileName">檔案名稱</param>
		/// <returns></returns>
		public static string ReadHtmlFile(string fileName)
		{
			System.Text.Encoding enc = ConvUtils.GetFileEncoding(fileName);
			if (enc != null)
				return File.ReadAllText(fileName, enc);
			else
				return File.ReadAllText(fileName);
		}
		#endregion

		#region Private Method : ImageCodecInfo GetImageEncoder(ImageFormat format)
		private ImageCodecInfo GetImageEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codes = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo ici in codes)
				if (ici.FormatID == format.Guid)
					return ici;
			return null;
		}
		#endregion

	}
}
