using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CJF.Modbus
{
	public abstract class ModbusBase
	{
		/// <summary>重試次數</summary>
		internal int _Retries = 0;
		/// <summary>讀取逾時時間，單位豪秒</summary>
		internal int _ReadTimeout = 1500;
		/// <summary>寫入逾時時間，單位豪秒</summary>
		internal int _WriteTimeout = 0;
		/// <summary>是否使用單一執行緒進行讀寫</summary>
		internal bool _SingleCommunication = true;

		internal ModbusBase()
		{
			this.IsDisposed = false;
		}

		/// <summary>設定或取得重試次數，預設值 0。</summary>
		public virtual int Retries
		{
			get { return _Retries; }
			set { _Retries = value; }
		}
		/// <summary>設定或取得讀取逾時時間，單位豪秒。預設值 1500ms</summary>
		public virtual int ReadTimeout
		{
			get { return _ReadTimeout; }
			set { _ReadTimeout = value; }
		}
		/// <summary>寫入逾時時間，單位豪秒。預設值 0ms</summary>
		public virtual int WriteTimeout
		{
			get { return _WriteTimeout; }
			set { _WriteTimeout = value; }
		}
		/// <summary>取得類別是否已被卸載</summary>
		public bool IsDisposed { get; internal set; }

		/// <summary>取得目前是否使用單一執行緒進行讀寫</summary>
		public bool SingleCommunication { get { return _SingleCommunication; } }
		/// <summary>
		/// 設定是否使用單一執行緒進行讀寫。
		/// <para>如 Slave 為硬體設備，建議使用單一執行緒方式讀寫</para>
		/// </summary>
		/// <param name="single">true:使用單一執行緒；false:使用多執行緒</param>
		public abstract void SetCommunicationMode(bool single);

	}
}
