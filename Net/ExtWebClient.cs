using System;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;

namespace CJF.Net
{
	#region Class : ExtWebClient
	/// <summary>自定義 WebClient 類別，繼承自 System.Net.WebClient。</summary>
	public class ExtWebClient : WebClient
	{
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
