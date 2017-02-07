using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CJF.Net
{
	#region Public Enum : FileUploaderCompletedResult
	/// <summary>檔案上傳完成回覆列舉</summary>
	public enum FileUploaderCompletedResult
	{
		/// <summary>失敗</summary>
		Failed = 0,
		/// <summary>成功</summary>
		Success = 1
	}
	#endregion

	#region Public Class : FileUploaderProgessChangedEventArgs : EventArgs
	/// <summary>FileUploaderProgessChangedEventArgs 事件參數類別</summary>
	public class FileUploaderProgessChangedEventArgs : EventArgs
	{
		/// <summary>取得已傳送的位元組數目。</summary>
		public long BytesSent { get; private set; }
		/// <summary>取得要傳送的位元組總數。</summary>
		public long TotalBytesToSend { get; private set; }
		/// <summary>取得使用者自訂資料</summary>
		public object UserState { get; private set; }
		/// <summary>建立 FileUploaderProgessChangedEventArgs 事件參數類別</summary>
		/// <param name="sent">已傳送位元組數</param>
		/// <param name="total">總位元組數</param>
		internal FileUploaderProgessChangedEventArgs(long sent, long total) : this(sent, total, null) { }
		/// <summary>建立 FileUploaderProgessChangedEventArgs 事件參數類別</summary>
		/// <param name="sent">已傳送位元組數</param>
		/// <param name="total">總位元組數</param>
		/// <param name="userState">使用者自訂資料</param>
		internal FileUploaderProgessChangedEventArgs(long sent, long total, object userState)
		{
			this.BytesSent = sent;
			this.TotalBytesToSend = total;
			this.UserState = userState;
		}
	}
	#endregion

	#region Publuc Class : FileUploaderCompletedEventArgs : EventArgs
	/// <summary>FileUploaderCompletedEventArgs 事件參數類別</summary>
	public class FileUploaderCompletedEventArgs : EventArgs
	{
		/// <summary>取得伺服器對資料上載作業的回應。</summary>
		public byte[] ServerResult { get; private set; }
		/// <summary>取得伺服器回應的狀態。</summary>
		public FileUploaderCompletedResult Status { get; private set; }
		/// <summary>取得使用者自訂資料</summary>
		public object UserState { get; private set; }
		/// <summary>建立 FileUploaderCompletedEventArgs 事件參數類別</summary>
		/// <param name="result">伺服器回覆的資料</param>
		/// <param name="status">檔案上傳成功與否回覆</param>
		internal FileUploaderCompletedEventArgs(byte[] result, FileUploaderCompletedResult status) : this(result, status, null) { }
		/// <summary>建立 FileUploaderCompletedEventArgs 事件參數類別</summary>
		/// <param name="result">伺服器回覆的資料</param>
		/// <param name="status">檔案上傳成功與否回覆</param>
		/// <param name="userState">使用者自訂資料</param>
		internal FileUploaderCompletedEventArgs(byte[] result, FileUploaderCompletedResult status, object userState)
		{
			this.ServerResult = result;
			this.Status = status;
			this.UserState = userState;
		}
	}
	#endregion

	#region Class : ExtWebClient
	/// <summary>自定義 WebClient 類別，繼承自 System.Net.WebClient。</summary>
	public class ExtWebClient : WebClient
	{
		#region Public Struct : FileData
		/// <summary>檔案上傳用的資料結構</summary>
		public struct FileData
		{
			/// <summary>鍵值</summary>
			public string KeyName;
			/// <summary>包含路徑的檔案名稱</summary>
			public string FileName;
			/// <summary>回傳結構是否為空值</summary>
			public bool IsEmpty { get { return string.IsNullOrEmpty(KeyName) && string.IsNullOrEmpty(FileName); } }
			/// <summary>檔案種類</summary>
			public string ContentType;
			/// <summary>建立一個新的 FileHttpUpload 結構</summary>
			/// <param name="key">上傳的鍵值</param>
			/// <param name="fileName">包含路徑的檔案名稱</param>
			/// <param name="type">檔案種類 ContentType</param>
			/// <returns></returns>
			public static FileData Create(string key, string fileName, string type)
			{
				return new FileData() { KeyName = key, FileName = fileName, ContentType = type };
			}
		}
		#endregion

		#region 類別公開事件
		/// <summary>檔案上傳進度事件，由 FileUploaderAsync(...) 函示產生</summary>
		public event EventHandler<FileUploaderProgessChangedEventArgs> OnFileUploaderProgressChanged;
		/// <summary>檔案上傳完成的事件，由 FileUploaderAsync(...) 函示產生</summary>
		public event EventHandler<FileUploaderCompletedEventArgs> OnFileUploaderCompleted;
		#endregion

		/// <summary>設定或取得請求逾時時間，單位豪秒</summary>
		public int RequestTimeout { get; set; }

		#region Construct Method : ExtWebClient(...)
		/// <summary>初始化 System.Net.WebClient 類別的新執行個體。</summary>
		public ExtWebClient()
		{
			this.RequestTimeout = 0;
		}
		/// <summary>初始化 System.Net.WebClient 類別的新執行個體。</summary>
		/// <param name="timeout">逾時時間，單位豪秒</param>
		public ExtWebClient(int timeout)
		{
			this.RequestTimeout = timeout;
		}
		#endregion

		#region Protected Override Method : WebRequest GetWebRequest(Uri address)
		/// <summary>傳回指定之資源的 System.Net.WebRequest 物件。</summary>
		/// <param name="address">識別所要求的資源。</param>
		/// <returns></returns>
		protected override WebRequest GetWebRequest(Uri address)
		{
			if (this.RequestTimeout != 0)
			{
				WebRequest w = base.GetWebRequest(address);
				w.Timeout = this.RequestTimeout;
				return w;
			}
			else
				return base.GetWebRequest(address);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(string urlAddress, string fileName)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="urlAddress">欲傳送的網址</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(string urlAddress, string fileName)
		{
			FileUploaderAsync(new Uri(urlAddress), null, fileName);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(string urlAddress, string fileName, object userState)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="urlAddress">欲傳送的網址</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <param name="userState">使用者自訂的附加資訊</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(string urlAddress, string fileName, object userState)
		{
			FileUploaderAsync(new Uri(urlAddress), null, fileName, userState);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(string urlAddress, NameValueCollection values, string fileName)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="urlAddress">欲傳送的網址</param>
		/// <param name="values">參數資料</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(string urlAddress, NameValueCollection values, string fileName)
		{
			FileUploaderAsync(new Uri(urlAddress), values, fileName);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(string urlAddress, NameValueCollection values, string fileName, object userState)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="urlAddress">欲傳送的網址</param>
		/// <param name="values">參數資料</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <param name="userState">使用者自訂的附加資訊</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(string urlAddress, NameValueCollection values, string fileName, object userState)
		{
			FileUploaderAsync(new Uri(urlAddress), values, fileName, userState);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(Uri uri, string fileName)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="uri">欲傳送的網址</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(Uri uri, string fileName)
		{
			FileUploaderAsync(uri, null, fileName);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(Uri uri, string fileName, object userState)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="uri">欲傳送的網址</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <param name="userState">使用者自訂的附加資訊</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(Uri uri, string fileName, object userState)
		{
			FileUploaderAsync(uri, null, fileName, userState);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(Uri uri, NameValueCollection values, string fileName)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="uri">欲傳送的網址</param>
		/// <param name="values">參數資料</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(Uri uri, NameValueCollection values, string fileName)
		{
			if (string.IsNullOrEmpty(fileName) && (values == null || values.Count == 0))
				throw new ArgumentException("values 與 fileName 不可同時為空值。");
			FileUploaderAsync(uri, values, new FileData[] { FileData.Create("file", fileName, CJF.Utility.ConvUtils.GetContentType(fileName)) }, null);
		}
		#endregion

		#region Public Method : void FileUploaderAsync(Uri uri, NameValueCollection values, string fileName, object userState)
		/// <summary>非同步方式上傳檔案與資料</summary>
		/// <param name="uri">欲傳送的網址</param>
		/// <param name="values">參數資料</param>
		/// <param name="fileName">包含路徑的檔案名稱</param>
		/// <param name="userState">使用者自訂的附加資訊</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 fileName 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(Uri uri, NameValueCollection values, string fileName, object userState)
		{
			if (string.IsNullOrEmpty(fileName) && (values == null || values.Count == 0))
				throw new ArgumentException("values 與 fileName 不可同時為空值。");
			FileUploaderAsync(uri, values, new FileData[] { FileData.Create("file", fileName, CJF.Utility.ConvUtils.GetContentType(fileName)) }, userState);
		}
		#endregion

		#region Public Mehod : void FileUploaderAsync(Uri uri, NameValueCollection values, FileData[] files, object userState)
		/// <summary>非同步方式上傳一個以上的檔案與資料</summary>
		/// <param name="uri">欲傳送的網址</param>
		/// <param name="values">參數資料</param>
		/// <param name="files">檔案清單</param>
		/// <param name="userState">使用者自訂的附加資訊</param>
		/// <exception cref="ArgumentNullException">uri 不得為空值。</exception>
		/// <exception cref="ArgumentException">values 與 files 不可同時為空值。</exception>
		/// <exception cref="FileNotFoundException">找不到檔案</exception>
		public void FileUploaderAsync(Uri uri, NameValueCollection values, FileData[] files, object userState)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");
			if ((values == null || values.Count == 0) && (files == null || files.Length == 0))
				throw new ArgumentException("values 與 files 不可同時為空值。");

			Task.Factory.StartNew(() =>
			{
				try
				{
					WebRequest req = WebRequest.Create(uri);
					req.Method = "POST";
					string boundary = string.Format("xxSTE@TGLxx{0:X}xxSTE@TGLxx", DateTime.Now.Ticks);
					req.ContentType = "multipart/form-data; boundary=" + boundary;
					boundary = "--" + boundary;

					MemoryStream msOut = new MemoryStream();
					byte[] buf;
					// Write the values into Output Stream
					if (values != null && values.Count != 0)
					{
						foreach (string name in values.Keys)
						{
							buf = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
							msOut.Write(buf, 0, buf.Length);
							buf = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
							msOut.Write(buf, 0, buf.Length);
							buf = Encoding.UTF8.GetBytes(values[name] + Environment.NewLine);
							msOut.Write(buf, 0, buf.Length);
						}
					}
					if (files != null && files.Length != 0)
					{
						const string format = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}";
						foreach (FileData fd in files)
						{
							if (!File.Exists(fd.FileName))
								throw new FileNotFoundException(fd.FileName);
							// Write the file into Output Stream
							buf = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
							msOut.Write(buf, 0, buf.Length);
							buf = Encoding.UTF8.GetBytes(string.Format(format, fd.KeyName, fd.FileName, Environment.NewLine));
							msOut.Write(buf, 0, buf.Length);
							buf = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", fd.ContentType, Environment.NewLine));
							msOut.Write(buf, 0, buf.Length);
							using (FileStream fs = File.OpenRead(fd.FileName))
							{
								fs.CopyTo(msOut);
								fs.Close();
							}
							buf = Encoding.ASCII.GetBytes(Environment.NewLine);
							msOut.Write(buf, 0, buf.Length);
						}
						buf = Encoding.ASCII.GetBytes(boundary + "--");
						msOut.Write(buf, 0, buf.Length);

						msOut.Position = 0;
						//IMPORTANT: set content length to directly write to network socket
						req.ContentLength = msOut.Length;
						var requestStream = req.GetRequestStream();

						//Write data in chunks and report progress
						var size = msOut.Length;
						const int chunkSize = 64 * 1024;
						buf = new byte[chunkSize];
						long bytesSent = 0;
						int readBytes;
						while ((readBytes = msOut.Read(buf, 0, buf.Length)) > 0)
						{
							requestStream.Write(buf, 0, readBytes);
							bytesSent += readBytes;

							//var status = "Uploading... " + bytesSent / 1024 + "KB of " + size / 1024 + "KB";
							var percentage = Convert.ToInt32(100 * bytesSent / size);
							if (OnFileUploaderProgressChanged != null)
								OnFileUploaderProgressChanged(this, new FileUploaderProgessChangedEventArgs(bytesSent, size, userState));
						}
					}
					//get response
					using (WebResponse response = req.GetResponse())
					using (Stream responseStream = response.GetResponseStream())
					using (MemoryStream stream = new MemoryStream())
					{
						// ReSharper disable once PossibleNullReferenceException - exception would get catched anyway
						responseStream.CopyTo(stream);
						if (OnFileUploaderCompleted != null)
						{
							if (stream == null || stream.Length == 0)
								OnFileUploaderCompleted(this, new FileUploaderCompletedEventArgs(null, FileUploaderCompletedResult.Failed, userState));
							else
								OnFileUploaderCompleted(this, new FileUploaderCompletedEventArgs(stream.ToArray(), FileUploaderCompletedResult.Success, userState));
						}
					}
				}
				catch (Exception ex)
				{
					if (OnFileUploaderCompleted != null)
						OnFileUploaderCompleted(this, new FileUploaderCompletedEventArgs(Encoding.UTF8.GetBytes(ex.Message), FileUploaderCompletedResult.Failed, userState));
				}
			}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
		}
		#endregion

		#region Public Static Method : string PostWebAPI(string url, string xml, int timeout = 5000)
		/// <summary>將 XML 資料以 WebAPI POST 傳輸方式傳送至遠端伺服器</summary>
		/// <param name="url">遠端伺服器網址</param>
		/// <param name="xml">欲傳送的資料</param>
		/// <param name="timeout">逾時時間，單位豪秒</param>
		/// <returns>自遠端伺服器回傳的資料(或網頁內容)</returns>
		/// <exception cref="ArgumentNullException">url 參數為 null。 -或- xml 參數為 null。</exception>
		/// <exception cref="ArgumentOutOfRangeException">逾時時間小於等於 0。</exception>
		/// <exception cref="WebException">傳輸錯誤</exception>
		public static string PostWebAPI(string url, string xml, int timeout = 5000)
		{
			string result = null;
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
			if (string.IsNullOrEmpty(xml)) throw new ArgumentNullException("xml");
			if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
			using (ExtWebClient wc = new ExtWebClient(timeout))
			{
				wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
				wc.Encoding = System.Text.Encoding.UTF8;
				NameValueCollection nc = new NameValueCollection();
				nc["xml"] = xml;
				byte[] bResult = wc.UploadValues(url, nc);
				result = System.Text.Encoding.UTF8.GetString(bResult);
			}
			return result;
		}
		#endregion

		#region Public Static Method : string PostWebAPI(string url, NameValueCollection nvc, int timeout = 5000)
		/// <summary>將 XML 資料以 WebAPI POST 傳輸方式傳送至遠端伺服器</summary>
		/// <param name="url">遠端伺服器網址</param>
		/// <param name="nvc">欲傳送的資料</param>
		/// <param name="timeout">逾時時間，單位豪秒</param>
		/// <returns>自遠端伺服器回傳的資料(或網頁內容)</returns>
		/// <exception cref="ArgumentNullException">url 參數為 null。 -或- nvc 參數為 null。</exception>
		/// <exception cref="ArgumentOutOfRangeException">逾時時間小於等於 0。</exception>
		/// <exception cref="WebException">傳輸錯誤</exception>
		public static string PostWebAPI(string url, NameValueCollection nvc, int timeout = 5000)
		{
			string result = null;
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
			if (nvc == null) throw new ArgumentNullException("nvc");
			if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
			using (ExtWebClient wc = new ExtWebClient(timeout))
			{
				wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
				wc.Encoding = System.Text.Encoding.UTF8;
				byte[] bResult = wc.UploadValues(url, nvc);
				result = System.Text.Encoding.UTF8.GetString(bResult);
			}
			return result;
		}
		#endregion

		#region Public Static Method : string GetWebAPI(string url, NameValueCollection queryString, int timeout = 5000)
		/// <summary>將 QueryString 資料以 WebAPI GET 傳輸方式傳送至遠端伺服器</summary>
		/// <param name="url">遠端伺服器網址</param>
		/// <param name="queryString">欲傳送的資料</param>
		/// <param name="timeout">逾時時間，單位豪秒</param>
		/// <returns>自遠端伺服器回傳的資料(或網頁內容)</returns>
		/// <exception cref="ArgumentNullException">url 參數為 null。</exception>
		/// <exception cref="ArgumentOutOfRangeException">逾時時間小於等於 0。</exception>
		/// <exception cref="WebException">傳輸錯誤</exception>
		public static string GetWebAPI(string url, NameValueCollection queryString, int timeout = 5000)
		{
			string result = null;
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
			if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
			using (ExtWebClient wc = new ExtWebClient(timeout))
			{
				wc.Encoding = System.Text.Encoding.UTF8;
				if (queryString != null)
					wc.QueryString = queryString;
				result = wc.DownloadString(url);
			}
			return result;
		}
		#endregion

		#region Public Static Method : string PostSOAP(string url, string xml, int timeout = 5000)
		/// <summary>將 SOAP 資料以 POST 傳輸方式傳送至遠端伺服器</summary>
		/// <param name="url">遠端伺服器網址</param>
		/// <param name="xml">欲傳送的資料</param>
		/// <param name="timeout">逾時時間，單位豪秒</param>
		/// <returns>自遠端伺服器回傳的資料(或網頁內容)</returns>
		/// <exception cref="ArgumentException">xml 參數不符合 SOAP 格式。</exception>
		/// <exception cref="ArgumentNullException">url 參數為 null。 -或- xml 參數為 null。</exception>
		/// <exception cref="ArgumentOutOfRangeException">逾時時間小於等於 0。</exception>
		/// <exception cref="WebException">傳輸錯誤</exception>
		public static string PostSOAP(string url, string xml, int timeout = 5000)
		{
			string result = null;
			if (string.IsNullOrEmpty(url)) throw new ArgumentNullException("url");
			if (string.IsNullOrEmpty(xml)) throw new ArgumentNullException("xml");
			if (timeout <= 0) throw new ArgumentOutOfRangeException("timeout");
			// 檢驗 SOAP 格式
			Regex reg = new Regex("<soap(12)?:Envelope[\\s\\S]*</soap(12)?:Envelope>$", RegexOptions.Multiline);
			Match m = reg.Match(xml);
			if (!m.Success) throw new ArgumentException("xml");
			System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
			try { doc.LoadXml(xml); }
			catch (System.Xml.XmlException) { throw new ArgumentException("xml"); }

			using (ExtWebClient wc = new ExtWebClient(timeout))
			{
				if (string.IsNullOrEmpty(m.Groups[1].Value))
					wc.Headers.Add("Content-Type", "text/xml; charset=utf-8");
				else if (m.Groups[1].Value.Equals("12"))
					wc.Headers.Add("Content-Type", "application/soap+xml; charset=utf-8");
				wc.Encoding = System.Text.Encoding.UTF8;
				result = wc.UploadString(url, xml);
			}
			return result;
		}
		#endregion
	}
	#endregion
}
