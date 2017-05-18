using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Text;
using log4net;

namespace CJF.Utility
{
	/// <summary>
	/// CJF.Utility.LogManager 記錄檔記錄類別。
	/// <para>請記得於 AssemblyInfo.cs 中加入 [assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]</para>
	/// </summary>
	[Serializable]
	public class LogManager
	{
		#region Public Enum : LogLevel
		/// <summary>紀錄層級列舉</summary>
		public enum LogLevel : uint
		{
			/// <summary>除錯用，所有訊息皆會紀錄</summary>
			Debug = 0,
			/// <summary>特別儲存資訊用，預設值</summary>
			Info = 1,
			/// <summary>警告訊息</summary>
			Warn = 2,
			/// <summary>錯誤訊息 Exception</summary>
			Error = 3,
			/// <summary>致命錯誤訊息</summary>
			Fatal = 4
			///// <summary>所有訊息</summary>
			//All = 5
		}
		#endregion

		#region LogManager Static Methods
		private static ILog _GlobalLogger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region Public Static Methods : void WriteLog(...)
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="msg">訊息</param>
		public static void WriteLog(string msg)
		{
			WriteLog(LogLevel.Info, msg);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="msg">訊息</param>
		public static void WriteLog(LogLevel lv, string msg)
		{
			switch (lv)
			{
				case LogLevel.Fatal:
					_GlobalLogger.Fatal(msg); break;
				case LogLevel.Error:
					_GlobalLogger.Error(msg); break;
				case LogLevel.Warn:
					_GlobalLogger.Warn(msg); break;
				case LogLevel.Debug:
					_GlobalLogger.Debug(msg); break;
				default:
					_GlobalLogger.Info(msg); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		public static void WriteLog(string format, object arg0)
		{
			WriteLog(LogLevel.Info, format, arg0);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		public static void WriteLog(LogLevel lv, string format, object arg0)
		{
			switch (lv)
			{
				case LogLevel.Fatal:
					_GlobalLogger.FatalFormat(format, arg0); break;
				case LogLevel.Error:
					_GlobalLogger.ErrorFormat(format, arg0); break;
				case LogLevel.Warn:
					_GlobalLogger.WarnFormat(format, arg0); break;
				case LogLevel.Debug:
					_GlobalLogger.DebugFormat(format, arg0); break;
				default:
					_GlobalLogger.InfoFormat(format, arg0); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		public static void WriteLog(string format, object arg0, object arg1)
		{
			WriteLog(LogLevel.Info, format, arg0, arg1);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		public static void WriteLog(LogLevel lv, string format, object arg0, object arg1)
		{
			switch (lv)
			{
				case LogLevel.Fatal:
					_GlobalLogger.FatalFormat(format, arg0, arg1); break;
				case LogLevel.Error:
					_GlobalLogger.ErrorFormat(format, arg0, arg1); break;
				case LogLevel.Warn:
					_GlobalLogger.WarnFormat(format, arg0, arg1); break;
				case LogLevel.Debug:
					_GlobalLogger.DebugFormat(format, arg0, arg1); break;
				default:
					_GlobalLogger.InfoFormat(format, arg0, arg1); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		/// <param name="arg2">要格式化的第三個物件</param>
		public static void WriteLog(string format, object arg0, object arg1, object arg2)
		{
			WriteLog(LogLevel.Info, format, arg0, arg1, arg2);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		/// <param name="arg2">要格式化的第三個物件</param>
		public static void WriteLog(LogLevel lv, string format, object arg0, object arg1, object arg2)
		{
			switch (lv)
			{
				case LogLevel.Fatal:
					_GlobalLogger.FatalFormat(format, arg0, arg1, arg2); break;
				case LogLevel.Error:
					_GlobalLogger.ErrorFormat(format, arg0, arg1, arg2); break;
				case LogLevel.Warn:
					_GlobalLogger.WarnFormat(format, arg0, arg1, arg2); break;
				case LogLevel.Debug:
					_GlobalLogger.DebugFormat(format, arg0, arg1, arg2); break;
				default:
					_GlobalLogger.InfoFormat(format, arg0, arg1, arg2); break;
			}
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
		public static void WriteLog(LogLevel lv, string format, params object[] args)
		{
			switch (lv)
			{
				case LogLevel.Fatal:
					_GlobalLogger.FatalFormat(format, args); break;
				case LogLevel.Error:
					_GlobalLogger.ErrorFormat(format, args); break;
				case LogLevel.Warn:
					_GlobalLogger.WarnFormat(format, args); break;
				case LogLevel.Debug:
					_GlobalLogger.DebugFormat(format, args); break;
				default:
					_GlobalLogger.InfoFormat(format, args); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="args"></param>
		public static void WriteLog(string format, params object[] args)
		{
			WriteLog(LogLevel.Info, format, args);
		}
		#endregion

		#region Private Static Method : void LogException(ILog logger, string sessionKey, Exception ex, bool sendMail)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="logger">log4Net.ILog</param>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="ex">錯誤類別</param>
		/// <param name="sendMail">是否寄發信件</param>
		private static void LogException(ILog logger, string sessionKey, Exception ex, bool sendMail)
		{
			try
			{
				logger.ErrorFormat("[{0}]EX:Type:{1}", sessionKey, ex.GetType());
				if (ex.GetType().Equals(typeof(NullReferenceException)))
				{
					NullReferenceException ne = (NullReferenceException)ex;
					logger.ErrorFormat("[{0}]EX:Source:{1}", sessionKey, ne.Source);
					logger.ErrorFormat("[{0}]EX:TargetSite:{1}", sessionKey, ne.TargetSite.Name);
				}
				else if (ex.GetType().Equals(typeof(System.Net.Sockets.SocketException)))
				{
					System.Net.Sockets.SocketException se = (System.Net.Sockets.SocketException)ex;
					logger.ErrorFormat("[{0}]EX:ErrorCode:{1}", sessionKey, se.ErrorCode);
					logger.ErrorFormat("[{0}]EX:SocketErrorCode:{1}:{2}", sessionKey, se.SocketErrorCode, (int)se.SocketErrorCode);
					logger.ErrorFormat("[{0}]EX:NativeErrorCode:{1}", sessionKey, se.NativeErrorCode);
				}
				else if (ex.GetType().Equals(typeof(System.Net.HttpListenerException)))
				{
					System.Net.HttpListenerException se = (System.Net.HttpListenerException)ex;
					logger.ErrorFormat("[{0}]EX:ErrorCode:{1}", sessionKey, se.ErrorCode);
					logger.ErrorFormat("[{0}]EX:NativeErrorCode:{1}", sessionKey, se.NativeErrorCode);
				}
				else if (ex.GetType().Equals(typeof(System.Data.SqlClient.SqlException)))
				{
					System.Data.SqlClient.SqlException se = (System.Data.SqlClient.SqlException)ex;
					logger.ErrorFormat("[{0}]EX:Class:{1:X2}", sessionKey, se.Class);
					logger.ErrorFormat("[{0}]EX:Number:{1}", sessionKey, se.Number);
					logger.ErrorFormat("[{0}]EX:LineNumber:{1}", sessionKey, se.LineNumber);
					logger.ErrorFormat("[{0}]EX:ErrorCode:{1}", sessionKey, se.ErrorCode);
					logger.ErrorFormat("[{0}]EX:Errors:{1}", sessionKey, se.Errors);
				}
				logger.ErrorFormat("[{0}]EX:Message:{1}", sessionKey, ex.Message);
				logger.ErrorFormat("[{0}]EX:Source:{1}", sessionKey, ex.Source);
				logger.ErrorFormat("[{0}]EX:StackTrace:{1}", sessionKey, ex.StackTrace);
				foreach (System.Collections.DictionaryEntry de in ex.Data)
					logger.ErrorFormat("[{0}]EX:Data:{1}:{2}", sessionKey, de.Key, de.Value);
				if (ex.InnerException != null)
				{
					logger.ErrorFormat("[{0}]EX:InnerException", sessionKey);
					LogException(logger, sessionKey, ex.InnerException, false);
				}
				if (sendMail)
				{
					string content = GetExceptionLogString(sessionKey, ex);
					System.Threading.Tasks.Task.Factory.StartNew(o => SendMail("Exception Notice", o.ToString(), null, false), content);
				}
			}
			catch (Exception nex)
			{
				logger.ErrorFormat("[LOG]EX:Type:{0}", nex.GetType());
				logger.ErrorFormat("[LOG]EX:Message:{0}", nex.Message);
				logger.ErrorFormat("[LOG]EX:Source:{0}", nex.Source);
				logger.ErrorFormat("[LOG]EX:StackTrace:{0}", nex.StackTrace);
			}
		}
		#endregion

		#region Private Static Method : void LogException(ILog logger, string sessionKey, Exception ex)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="logger">log4Net.ILog</param>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="ex">錯誤類別</param>
		private static void LogException(ILog logger, string sessionKey, Exception ex)
		{
			LogException(logger, sessionKey, ex, true);
		}
		#endregion

		#region Public Static Method : void LogException(string sessionKey, Exception ex, bool sendMail)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="ex">錯誤類別</param>
		/// <param name="sendMail">是否寄發信件</param>
		public static void LogException(string sessionKey, Exception ex, bool sendMail)
		{
			LogException(_GlobalLogger, sessionKey, ex, sendMail);
		}
		#endregion

		#region Public Static Method : void LogException(string sessionKey, Exception ex)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="ex">錯誤類別</param>
		public static void LogException(string sessionKey, Exception ex)
		{
			LogException(_GlobalLogger, sessionKey, ex, true);
		}
		#endregion

		#region Public Static Method : void LogException(string sessionKey, int index, Exception ex, bool sendMail)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="index">對應索引值</param>
		/// <param name="ex">錯誤類別</param>
		/// <param name="sendMail">是否寄發信件</param>
		public static void LogException(string sessionKey, int index, Exception ex, bool sendMail)
		{
			LogException(_GlobalLogger, string.Format("{0}][{1:X4}", sessionKey, index), ex, sendMail);
		}
		#endregion

		#region Public Static Method : void LogException(string sessionKey, int index, Exception ex)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="index">對應索引值</param>
		/// <param name="ex">錯誤類別</param>
		public static void LogException(string sessionKey, int index, Exception ex)
		{
			LogException(_GlobalLogger, string.Format("{0}][{1:X4}", sessionKey, index), ex, true);
		}
		#endregion

		#region Public Static Method : void LogException(Exception ex, bool sendMail)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="ex">錯誤類別</param>
		/// <param name="sendMail">是否寄發信件，預設寄發</param>
		public static void LogException(Exception ex, bool sendMail)
		{
			LogException(_GlobalLogger, "ERR", ex, sendMail);
		}
		#endregion

		#region Public Static Method : void LogException(Exception ex)
		/// <summary>記錄錯誤事件</summary>
		/// <param name="ex">錯誤類別</param>
		public static void LogException(Exception ex)
		{
			LogException(_GlobalLogger, "ERR", ex, true);
		}
		#endregion

		#region Public Static Method : void ReinitinalLogger()
		/// <summary>重新載入記錄器</summary>
		public static void ReinitinalLogger()
		{
			_GlobalLogger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		}
		#endregion
		#endregion

		#region Public Static Method : void SendMail(string subject, string content, Encoding enc, bool useHtml)
		/// <summary>寄發郵件，發送者、對象與伺服器設定直接來自App.Config</summary>
		/// <param name="subject">信件主旨</param>
		/// <param name="content">信件內容</param>
		/// <param name="enc">編碼</param>
		/// <param name="useHtml">是否使用HTML格式</param>
		public static void SendMail(string subject, string content, Encoding enc, bool useHtml)
		{
			NameValueCollection nvc = ConfigurationManager.AppSettings;
			string mailServer = nvc["MailServer"];
			string SvrID = nvc["ServerID"];
			string mailSubject = nvc["MailSubject"];
			int mailPort = 25;
			string[] mailTo = new string[] { };
			if (!string.IsNullOrEmpty(nvc["MailTo"]))
				mailTo = nvc["MailTo"].Split(';');
			string mailFrom = nvc["MailFrom"];
			if (string.IsNullOrEmpty(mailServer) || mailTo.Length == 0 || string.IsNullOrEmpty(mailFrom))
				return;
			string userName = nvc["FromUser"];
			string pwd = nvc["FromPWD"];
			if (enc == null) enc = Encoding.UTF8;
			try
			{
				MailMessage mail = new MailMessage();	// MailMessage(寄信者, 收信者)
				mail.From = new MailAddress(mailFrom);
				foreach (string s in mailTo)
					mail.To.Add(s);
				mail.IsBodyHtml = useHtml;
				mail.BodyEncoding = enc;		// E-mail編碼
				if (string.IsNullOrEmpty(mailSubject))
					mail.Subject = (string.IsNullOrEmpty(SvrID) ? subject : subject + " - " + SvrID);			// E-mail主旨
				else
					mail.Subject = mailSubject;
				mail.Body = content;			// E-mail內容
				if (mailServer.IndexOf(':') != -1)
				{
					mailPort = Convert.ToInt32(mailServer.Split(':')[1]);
					mailServer = mailServer.Split(':')[0];
				}
				SmtpClient smtpClient = new SmtpClient(mailServer, mailPort);	// 設定E-mail Server和port
				smtpClient.UseDefaultCredentials = false;
				smtpClient.Credentials = new System.Net.NetworkCredential(userName, pwd);
				smtpClient.Send(mail);
			}
			catch (Exception ex)
			{
				LogException("Mail", ex, false);
			}
		}
		#endregion

		#region Public Static Method : string GetFolder()
		/// <summary>取得記錄檔的存放目錄</summary>
		/// <returns></returns>
		public static string GetFolder()
		{
			log4net.Appender.IAppender[] ias = _GlobalLogger.Logger.Repository.GetAppenders();
			if (ias == null || ias.Length == 0)
				return null;
			foreach (log4net.Appender.IAppender ia in ias)
			{
				if (!ia.GetType().Equals(typeof(log4net.Appender.RollingFileAppender)))
					continue;
				log4net.Appender.FileAppender fa = (log4net.Appender.FileAppender)(log4net.Appender.RollingFileAppender)ia;
				return System.IO.Path.GetDirectoryName(fa.File);
			}
			return null;
		}
		#endregion

		#region Private Static Method : string GetExceptionLogString(string sessionKey, Exception ex)
		/// <summary>取得該錯誤的信件內容</summary>
		/// <param name="sessionKey"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		private static string GetExceptionLogString(string sessionKey, Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			try
			{
				sb.AppendLine(string.Format("DateTime:{0:yyyy/MM/dd HH\\:mm\\:ss.fff}", DateTime.Now));
				sb.AppendLine(string.Format("[{0}]EX:Type:{1}", sessionKey, ex.GetType()));
				if (ex.GetType().Equals(typeof(System.Net.Sockets.SocketException)))
				{
					System.Net.Sockets.SocketException se = (System.Net.Sockets.SocketException)ex;
					sb.AppendLine(string.Format("[{0}]EX:ErrorCode:{1}", sessionKey, se.ErrorCode));
					sb.AppendLine(string.Format("[{0}]EX:SocketErrorCode:{1}:{2}", sessionKey, se.SocketErrorCode, (int)se.SocketErrorCode));
					sb.AppendLine(string.Format("[{0}]EX:NativeErrorCode:{1}", sessionKey, se.NativeErrorCode));
				}
				sb.AppendLine(string.Format("[{0}]EX:Message:{1}", sessionKey, ex.Message));
				sb.AppendLine(string.Format("[{0}]EX:Source:{1}", sessionKey, ex.Source));
				sb.AppendLine(string.Format("[{0}]EX:StackTrace:{1}", sessionKey, ex.StackTrace));
				foreach (System.Collections.DictionaryEntry de in ex.Data)
					sb.AppendLine(string.Format("[{0}]EX:Data:{1}:{2}", sessionKey, de.Key, de.Value));
				if (ex.InnerException != null)
				{
					sb.AppendLine("InnerException:");
					sb.AppendLine(GetExceptionLogString(sessionKey, ex.InnerException));
				}
			}
			catch { }
			return sb.ToString();
		}
		#endregion

		#region LogManager Methods
		private ILog _PrivateLogger;
		/// <summary>建立 LogManager 類別</summary>
		/// <param name="source">類別型別</param>
		public LogManager(Type source)
		{
			_PrivateLogger = log4net.LogManager.GetLogger(source);
		}
		/// <summary>
		/// 建立含自訂欄位的 LogManager 類別
		/// </summary>
		/// <param name="source">類別型別</param>
		/// <param name="tokenValue">自訂欄位值</param>
		public LogManager(Type source, string tokenValue)
			: this(source)
		{
			_PrivateLogger = log4net.LogManager.GetLogger(source);
			this.TokenValue = tokenValue;
		}
		/// <summary>
		/// 建立私用的LogManager類別
		/// </summary>
		/// <param name="configFile">log4net 參數存放位置</param>
		/// <param name="appender">私用鍵值</param>
		public LogManager(string configFile, string appender)
		{
			log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo(configFile));
			_PrivateLogger = log4net.LogManager.GetLogger(appender);
		}
		/// <summary>設定或取得自訂欄位值</summary>
		public string TokenValue { get; set; }

		#region Write(...)
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="msg">訊息</param>
		public void Write(string msg)
		{
			Write(LogLevel.Info, msg);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="msg">訊息</param>
		public void Write(LogLevel lv, string msg)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			switch (lv)
			{
				case LogLevel.Fatal:
					_PrivateLogger.Fatal(msg); break;
				case LogLevel.Error:
					_PrivateLogger.Error(msg); break;
				case LogLevel.Warn:
					_PrivateLogger.Warn(msg); break;
				case LogLevel.Debug:
					_PrivateLogger.Debug(msg); break;
				default:
					_PrivateLogger.Info(msg); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		public void Write(string format, object arg0)
		{
			Write(LogLevel.Info, format, arg0);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		public void Write(LogLevel lv, string format, object arg0)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			switch (lv)
			{
				case LogLevel.Fatal:
					_PrivateLogger.FatalFormat(format, arg0); break;
				case LogLevel.Error:
					_PrivateLogger.ErrorFormat(format, arg0); break;
				case LogLevel.Warn:
					_PrivateLogger.WarnFormat(format, arg0); break;
				case LogLevel.Debug:
					_PrivateLogger.DebugFormat(format, arg0); break;
				default:
					_PrivateLogger.InfoFormat(format, arg0); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		public void Write(string format, object arg0, object arg1)
		{
			Write(LogLevel.Info, format, arg0, arg1);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		public void Write(LogLevel lv, string format, object arg0, object arg1)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			switch (lv)
			{
				case LogLevel.Fatal:
					_PrivateLogger.FatalFormat(format, arg0, arg1); break;
				case LogLevel.Error:
					_PrivateLogger.ErrorFormat(format, arg0, arg1); break;
				case LogLevel.Warn:
					_PrivateLogger.WarnFormat(format, arg0, arg1); break;
				case LogLevel.Debug:
					_PrivateLogger.DebugFormat(format, arg0, arg1); break;
				default:
					_PrivateLogger.InfoFormat(format, arg0, arg1); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		/// <param name="arg2">要格式化的第三個物件</param>
		public void Write(string format, object arg0, object arg1, object arg2)
		{
			Write(LogLevel.Info, format, arg0, arg1, arg2);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="arg0">要格式化的物件</param>
		/// <param name="arg1">要格式化的第二個物件</param>
		/// <param name="arg2">要格式化的第三個物件</param>
		public void Write(LogLevel lv, string format, object arg0, object arg1, object arg2)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			switch (lv)
			{
				case LogLevel.Fatal:
					_PrivateLogger.FatalFormat(format, arg0, arg1, arg2); break;
				case LogLevel.Error:
					_PrivateLogger.ErrorFormat(format, arg0, arg1, arg2); break;
				case LogLevel.Warn:
					_PrivateLogger.WarnFormat(format, arg0, arg1, arg2); break;
				case LogLevel.Debug:
					_PrivateLogger.DebugFormat(format, arg0, arg1, arg2); break;
				default:
					_PrivateLogger.InfoFormat(format, arg0, arg1, arg2); break;
			}
		}
		/// <summary>記錄事件，使用LogLevel.Info級別記錄訊息</summary>
		/// <param name="format">字串格式</param>
		/// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
		public void Write(string format, params object[] args)
		{
			Write(LogLevel.Info, format, args);
		}
		/// <summary>記錄事件</summary>
		/// <param name="lv">訊息紀錄級別</param>
		/// <param name="format">字串格式</param>
		/// <param name="args">物件陣列，包含零或多個要格式化的物件。</param>
		public void Write(LogLevel lv, string format, params object[] args)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			switch (lv)
			{
				case LogLevel.Fatal:
					_PrivateLogger.FatalFormat(format, args); break;
				case LogLevel.Error:
					_PrivateLogger.ErrorFormat(format, args); break;
				case LogLevel.Warn:
					_PrivateLogger.WarnFormat(format, args); break;
				case LogLevel.Debug:
					_PrivateLogger.DebugFormat(format, args); break;
				default:
					_PrivateLogger.InfoFormat(format, args); break;
			}
		}
		#endregion

		/// <summary>記錄錯誤事件</summary>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="ex">錯誤類別</param>
		/// <param name="sendMail">是否寄發信件</param>
		public void WriteException(string sessionKey, Exception ex, bool sendMail)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			LogException(_PrivateLogger, sessionKey, ex, sendMail);
		}
		/// <summary>記錄錯誤事件</summary>
		/// <param name="sessionKey">關鍵索引鍵</param>
		/// <param name="ex">錯誤類別</param>
		public void WriteException(string sessionKey, Exception ex)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			LogException(_PrivateLogger, sessionKey, ex);
		}
		/// <summary>記錄錯誤事件</summary>
		/// <param name="ex">錯誤類別</param>
		/// <param name="sendMail">是否寄發信件</param>
		public void WriteException(Exception ex, bool sendMail)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			LogException(_PrivateLogger, "ERR", ex, sendMail);
		}
		/// <summary>記錄錯誤事件</summary>
		/// <param name="ex">錯誤類別</param>
		public void WriteException(Exception ex)
		{
			log4net.LogicalThreadContext.Properties["TokenValue"] = this.TokenValue;
			LogException(_PrivateLogger, "ERR", ex);
		}
		#endregion
	}
}
