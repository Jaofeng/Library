using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Xml;

namespace CJF.Net.Http
{
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

	#region Interface : IWebAPI
	/// <summary>WebAPI基礎介面</summary>
	public interface IWebAPI
	{
		/// <summary>取得屬性值</summary>
		/// <param name="property">屬性名稱</param>
		/// <returns></returns>
		object GetPropertyValue(string property);
		/// <summary>取得回應的XML字串</summary>
		/// <param name="param">參數</param>
		/// <returns></returns>
		string GetResponseXml(params object[] param);
		/// <summary>產生回傳代碼的 XML 回傳給終端</summary>
		/// <param name="retCode">回傳代碼</param>
		/// <returns></returns>
		string GetResultXml(string retCode);
		/// <summary>產生例外錯誤的 XML 訊息內容回傳給終端</summary>
		/// <param name="ex">錯誤內容</param>
		/// <returns></returns>
		string GetExceptionXml(Exception ex);
	}
	#endregion

	#region Public Class : WebAPIBase
	/// <summary>WebAPI基礎類別</summary>
	public class WebAPIBase : IWebAPI, IDisposable
	{
		/// <summary>是否已釋放資源</summary>
		protected bool _IsDisposed = false;
		/// <summary>基礎HttpListenerContext類別</summary>
		protected HttpListenerContext HttpCtx;
		/// <summary>XML根節點名稱</summary>
		protected string RootTagRes;

		/// <summary>隱性釋放</summary>
		~WebAPIBase() { Dispose(false); }

		/// <summary>取得原始 XML 字串</summary>
		public string SourceXml { get; protected set; }

		#region Public Virtual Method : virtual string GetResponseXml(params object[] param)
		/// <summary>取得回應的XML字串</summary>
		/// <param name="param">參數</param>
		/// <returns></returns>
		public virtual string GetResponseXml(params object[] param) 
		{
			if (param[0].GetType().Equals(typeof(string)))
				return GetResultXml(RootTagRes, param[0].ToString());
			else
				return null;
		}
		#endregion

		#region Public Virtual Method : object GetPropertyValue(string property)
		/// <summary>取得屬性值</summary>
		/// <param name="property">屬性名稱</param>
		/// <returns></returns>
		public virtual object GetPropertyValue(string property)
		{
			List<PropertyInfo> list = new List<PropertyInfo>(this.GetType().GetProperties());
			Predicate<PropertyInfo> find = delegate(PropertyInfo pi) { return pi.Name.Equals(property); };
			PropertyInfo p = list.Find(find);
			if (p == null)
				return null;
			else
				return p.GetValue(this, null);
		}
		#endregion

		#region Public Virtual Method : string GetResultXml(string retCode)
		/// <summary>產生回傳代碼的 XML 回傳給終端</summary>
		/// <param name="retCode">回傳代碼</param>
		/// <returns></returns>
		public virtual string GetResultXml(string retCode)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><" + RootTagRes + "/>");
			XmlNode root = xml.DocumentElement;
			// ReturnCode
			XmlNode rtrnCodeNode = xml.CreateElement("ReturnCode");
			rtrnCodeNode.InnerText = retCode;
			root.AppendChild(rtrnCodeNode);
			return xml.OuterXml;
		}
		#endregion

		#region Public Virtual Method : string GetExceptionXml(Exception ex)
		/// <summary>產生例外錯誤的 XML 訊息內容回傳給終端</summary>
		/// <param name="ex">錯誤內容</param>
		/// <returns></returns>
		public virtual string GetExceptionXml(Exception ex)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><" + RootTagRes + "/>");
			XmlNode root = xml.DocumentElement;
			// ReturnCode
			XmlNode rtrnCodeNode = xml.CreateElement("ReturnCode");
			rtrnCodeNode.InnerText = "9999";
			root.AppendChild(rtrnCodeNode);
			XmlNode expNode = xml.CreateElement("Exception");
			expNode.InnerText = ex.Message;
			root.AppendChild(expNode);
			return xml.OuterXml;
		}
		#endregion

		#region Public Static Method : string GetResultXml(string retTag, string retCode)
		/// <summary>產生回傳代碼的 XML 回傳給終端</summary>
		/// <param name="retTag">回傳的API Tag Name</param>
		/// <param name="retCode">回傳代碼</param>
		/// <returns></returns>
		public static string GetResultXml(string retTag, string retCode)
		{
			XmlDocument xml = new XmlDocument();
			xml.LoadXml("<?xml version=\"1.0\" encoding=\"UTF-8\"?><" + retTag + "/>");
			XmlNode root = xml.DocumentElement;
			// ReturnCode
			XmlNode rtrnCodeNode = xml.CreateElement("ReturnCode");
			rtrnCodeNode.InnerText = retCode;
			root.AppendChild(rtrnCodeNode);
			return xml.OuterXml;
		}
		#endregion

		#region IDisposable
		/// <summary>釋放資源</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>釋放資源</summary>
		/// <param name="disposing">是否確實釋放</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_IsDisposed) return;
			if (disposing)
			{
				RootTagRes = string.Empty;
				RootTagRes = null;
				SourceXml = string.Empty;
				SourceXml = null;
				if (HttpCtx != null)
					HttpCtx = null;
			}
			_IsDisposed = true;
		}
		#endregion
	}
	#endregion
}
