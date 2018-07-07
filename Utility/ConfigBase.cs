using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Text;

namespace CJF.Utility
{
	/// <summary>基礎參數類別</summary>
	[Serializable]
    [Obsolete("結構變更，不要使用。", true)]
	public class ConfigBase : IDisposable
	{
		#region Static Properties
		static string _ServerID = ConfigurationManager.AppSettings["ServerID"];
		/// <summary>取得伺服器代碼</summary>
		public static string ServerID { get { return _ServerID; } }
		#endregion

		#region Public Properties
		/// <summary>取得伺服器參數資料</summary>
		public Hashtable Settings { get; protected set; }
		/// <summary>取得共用參數資料</summary>
		public Hashtable Commons { get; protected set; }
		#endregion

		/// <summary>是否已釋放資源</summary>
		protected bool _IsDisposed = false;
		/// <summary>隱性函示，釋放資源</summary>
		~ConfigBase() { Dispose(false); }

		#region Public Virtual Method : void Reload()
		/// <summary>重新載入參數資料，直接覆寫</summary>
		public virtual void Reload()
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Virtual Method : int IntSetting(string key)
		/// <summary>傳回型態為 int 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual int IntSetting(string key)
		{
			if (this.Settings == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			int result = 0;
			object o = this.Settings[key];
			if (o != null)
				int.TryParse(o.ToString(), out result);
			return result;
		}
		#endregion

		#region Public Virtual Method : short ShortSetting(string key)
		/// <summary>傳回型態為 short 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual short ShortSetting(string key)
		{
			if (this.Settings == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			short result = 0;
			object o = this.Settings[key];
			if (o != null)
				short.TryParse(o.ToString(), out result);
			return result;
		}
		#endregion

		#region Public Virtual Method : string StringSetting(string key)
		/// <summary>傳回型態為 string 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual string StringSetting(string key)
		{
			if (this.Settings == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			object o = this.Settings[key];
			if (o != null)
				return o.ToString();
			else
				return string.Empty;
		}
		#endregion

		#region Public Virtual Method : float SingleSetting(string key)
		/// <summary>傳回型態為 float 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual float SingleSetting(string key)
		{
			if (this.Settings == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			float result = 0;
			object o = this.Settings[key];
			if (o != null)
				float.TryParse(o.ToString(), out result);
			return result;
		}
		#endregion

		#region Public Virtual Method : bool BooleanSetting(string key, bool defValue)
		/// <summary>傳回型態為 bool 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <param name="defValue">預設值</param>
		/// <returns></returns>
		public virtual bool BooleanSetting(string key, bool defValue)
		{
			if (this.Settings == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			object o = this.Settings[key];
			if (o == null)
				return defValue;
			else
			{
				switch (o.ToString().ToUpper())
				{
					case "TRUE":
					case "YES":
					case "T":
					case "Y":
					case "1":
						return true;
					default:
						return false;
				}
			}
		}
		#endregion

		#region Public Virtual Method : int CommonInt(string key)
		/// <summary>傳回型態為 int 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual int CommonInt(string key)
		{
			if (this.Commons == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			if (!this.Commons.ContainsKey(key))
				return 0;
			else
				return Convert.ToInt32(this.Commons[key]);
		}
		#endregion

		#region Public Virtual Method : short CommonShort(string key)
		/// <summary>傳回型態為 short 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual short CommonShort(string key)
		{
			if (this.Commons == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			short result = 0;
			object o = this.Commons[key];
			if (o != null)
				short.TryParse(o.ToString(), out result);
			return result;
		}
		#endregion

		#region Public Virtual Method : string CommonString(string key)
		/// <summary>傳回型態為 string 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual string CommonString(string key)
		{
			if (this.Commons == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			object o = this.Commons[key];
			if (o != null)
				return o.ToString();
			else
				return string.Empty;
		}
		#endregion

		#region Public Virtual Method : float CommonSingle(string key)
		/// <summary>傳回型態為 float 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <returns></returns>
		public virtual float CommonSingle(string key)
		{
			if (this.Commons == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			float result = 0;
			object o = this.Commons[key];
			if (o != null)
				float.TryParse(o.ToString(), out result);
			return result;
		}
		#endregion

		#region Public Virtual Method : bool CommonBoolean(string key, bool defValue)
		/// <summary>傳回型態為 bool 的參數值</summary>
		/// <param name="key">參數鍵名</param>
		/// <param name="defValue">預設值</param>
		/// <returns></returns>
		public virtual bool CommonBoolean(string key, bool defValue)
		{
			if (this.Commons == null)
				throw new TypeInitializationException(this.GetType().ToString(), new NotImplementedException());
			object o = this.Commons[key];
			if (o == null)
				return defValue;
			else
			{
				switch (o.ToString().ToUpper())
				{
					case "TRUE":
					case "YES":
					case "T":
					case "Y":
					case "1":
						return true;
					default:
						return false;
				}
			}
		}
		#endregion

		#region IDisposable 成員
		/// <summary>卸載本類別</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>卸載本類別</summary>
		/// <param name="disposing">是否確實卸載</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_IsDisposed) return;
			if (disposing)
			{
				try
				{
					this.Settings.Clear();
					this.Settings = null;
					this.Commons.Clear();
					this.Commons = null;
				}
				catch { }
			}
			_IsDisposed = true;
		}
		#endregion
	}
}
