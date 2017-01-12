using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading;
using System.Text;
using CJF.Utility;

namespace CJF.Net
{
	/// <summary>處理 Ping 的類別元件</summary>
	public class PingTester : IDisposable
	{
		LogManager _log = new LogManager(typeof(PingTester));
		Timer _PingTimer = null;
		bool _IsDisposed = false;

		#region Public Events
		/// <summary>當錯誤發生時觸發</summary>
		public event EventHandler<PingResultEventArgs> OnException;
		/// <summary>當回應時觸發</summary>
		public event EventHandler<PingResultEventArgs> OnResult;
		/// <summary>當已達到發送次數時觸發</summary>
		public event EventHandler OnFinished;
		#endregion

		#region Construct Method : PingTester(...) + 2
		/// <summary>建立一個 Ping 類別元件。</summary>
		/// <param name="hostNameOrAddress">System.String，識別 ICMP 回應訊息的目標電腦。指定給這個參數的值可以是主機名稱或 IP 位址的字串表示。</param>
		public PingTester(string hostNameOrAddress) : this(hostNameOrAddress, 128, 3000, 1000, 32, 0) { }
		/// <summary>建立一個 Ping 類別元件。</summary>
		/// <param name="hostNameOrAddress">System.String，識別 ICMP 回應訊息的目標電腦。指定給這個參數的值可以是主機名稱或 IP 位址的字串表示。</param>
		/// <param name="ttl">資料轉送次數(Time to Live)</param>
		/// <param name="cycle">測試週期, 單位豪秒。</param>
		/// <param name="timeout">逾時時間, 單位豪秒。</param>
		/// <param name="dataLength">資料長度, 不得超過 65500。</param>
		/// <param name="times">重複次數, 傳入 0 則一直重複。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// 測試週期不得小於100豪秒。 -或-
		/// 逾時時間不得小於100豪秒。 -或-
		/// 資料長度必須大於 32 且小於 65,500。 -或-
		/// 測試次數不得小於 0。 -或-
		/// </exception>
		public PingTester(string hostNameOrAddress, int ttl, int cycle, int timeout, int dataLength, int times)
		{
			this.HostNameOrAddress = hostNameOrAddress;
			this.TimeToLive = ttl;
			this.Cycle = cycle;
			this.Timeout = timeout;
			this.DataLength = dataLength;
			this.Times = times;
		}
		/// <summary>隱式釋放</summary>
		~PingTester() { Dispose(false); }
		#endregion

		#region IDisposable
		/// <summary>將 CJF.Net.PingTester 目前的執行個體所使用的資源全部釋出。</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		/// <summary>將 CJF.Net.PingTester 目前的執行個體所使用的資源全部釋出。</summary>
		/// <param name="isDispose">是否確實釋放資源</param>
		protected virtual void Dispose(bool isDispose)
		{
			if (_IsDisposed) return;
			if (isDispose)
			{
				if (_PingTimer != null)
					_PingTimer.Dispose();
				_PingTimer = null;
			}
			_IsDisposed = true;
		}
		#endregion

		#region Properties
		private string _HostNameOrAddress = string.Empty;
		/// <summary></summary>
		public string HostNameOrAddress { get { return _HostNameOrAddress; } set { _HostNameOrAddress = value; } }

		#region Cycle : int
		private int _Cycle = 0;
		/// <summary>設定或取得測試週期，單位豪秒。</summary>
		/// <exception cref="System.ArgumentOutOfRangeException">測試週期不得小於100豪秒。</exception>
		public int Cycle
		{
			get { return _Cycle; }
			set
			{
				if (_Cycle != value)
				{
					if (value < 100)
						throw new ArgumentOutOfRangeException("Cycle", "測試週期不得小於100豪秒。");
					_Cycle = value;
					if (_PingTimer != null)
						_PingTimer.Change(0, _Cycle);
					else
						_PingTimer = new Timer(PingCallback, null, 0, _Cycle);
				}
			}
		}
		#endregion

		#region Timeout : int
		private int _Timeout = 0;
		/// <summary>設定或取得逾時時間，單位豪秒。</summary>
		/// <exception cref="System.ArgumentOutOfRangeException">逾時時間不得小於100豪秒。</exception>
		public int Timeout
		{
			get { return _Timeout; }
			set
			{
				if (value < 100)
					throw new ArgumentOutOfRangeException("Timeout", "逾時時間不得小於100豪秒。");
				_Timeout = value;
			}
		}
		#endregion

		#region DataLength : int
		private int _DataLength = 0;
		/// <summary>設定或取得測試的資料長度，單位豪秒。</summary>
		/// <exception cref="System.ArgumentOutOfRangeException">設定值必須大於 32 且小於 65,500。</exception>
		public int DataLength
		{
			get { return _DataLength; }
			set
			{
				if (value > 65500 || value < 32)
					throw new ArgumentOutOfRangeException("DataLength", "資料長度必須大於 32 且小於 65,500。");
				_DataLength = value;
			}
		}
		#endregion

		#region Times : int
		private int _Times = 0;
		/// <summary>設定或取得測試次數, 設定 0 時則無限循環。</summary>
		/// <exception cref="System.ArgumentOutOfRangeException">設定值必須大於 0。</exception>
		public int Times
		{
			get { return _Times; }
			set
			{
				if (value < 0)
					throw new ArgumentOutOfRangeException("Times", "測試次數不得小於 0。");
				_Times = value;
			}
		}
		#endregion

		/// <summary>取得或設定最大轉送次數</summary>
		public int TimeToLive { get; set; }
		long _DoneTimes = 0;
		/// <summary>取得已發送的次數。</summary>
		public long DoneTimes { get { return Interlocked.Read(ref _DoneTimes); } }
		/// <summary>設定或取得額外資訊</summary>
		public object ExtraInfo { get; set; }
		#endregion

		#region Delegate Callback Methods
		private void PingResultCallback(IAsyncResult result)
		{
			EventHandler<PingResultEventArgs> del = result.AsyncState as EventHandler<PingResultEventArgs>;
			del.EndInvoke(result);
		}
		private void EventCallback(IAsyncResult result)
		{
			EventHandler del = result.AsyncState as EventHandler;
			del.EndInvoke(result);
		}
		#endregion

		#region Private Timer callback Method : void PingCallback(object o)
		private void PingCallback(object o)
		{
			if (_Times != 0 && _DoneTimes >= _Times)
			{
				if (_PingTimer != null)
					_PingTimer.Dispose();
				_PingTimer = null;
				return;
			}
			Interlocked.Increment(ref _DoneTimes);
			if (_DoneTimes >= long.MaxValue)
				Interlocked.Exchange(ref _DoneTimes, 0);
			using (Ping pingSender = new Ping())
			{
				PingOptions options = new PingOptions(this.TimeToLive, true);
				string data = "a".PadLeft(this.DataLength, 'a');
				byte[] buffer = Encoding.ASCII.GetBytes(data);
				try
				{
					PingReply reply = pingSender.Send(this.HostNameOrAddress, this.Timeout, buffer, options);

					if (OnResult != null)
					{

						PingResultEventArgs arg = new PingResultEventArgs(reply);
						foreach (EventHandler<PingResultEventArgs> del in this.OnResult.GetInvocationList())
						{
							try { del.BeginInvoke(this, arg, new AsyncCallback(PingResultCallback), del); }
							catch (Exception exx) { _log.WriteException(exx); }
						}
					}
				}
				catch (Exception ex)
				{
					if (OnException != null)
					{
						PingResultEventArgs arg = new PingResultEventArgs(ex);
						foreach (EventHandler<PingResultEventArgs> del in this.OnException.GetInvocationList())
						{
							try { del.BeginInvoke(this, arg, new AsyncCallback(PingResultCallback), del); }
							catch (Exception exx) { _log.WriteException(exx); }
						}
					}
				}
			}
			if (_Times != 0 && _DoneTimes >= _Times)
			{
				if (_PingTimer != null)
					_PingTimer.Dispose();
				_PingTimer = null;
				if (OnFinished != null)
				{
					foreach (EventHandler del in this.OnFinished.GetInvocationList())
					{
						try { del.BeginInvoke(this, new EventArgs(), new AsyncCallback(EventCallback) , del); }
						catch (Exception exx) { _log.WriteException(exx); }
					}
				}
			}
		}
		#endregion

		#region Public Method : void Restart()
		/// <summary>重新啟動</summary>
		public void Restart()
		{
			_DoneTimes = 0;
			if (_PingTimer != null)
				_PingTimer.Change(0, _Cycle);
			else
				_PingTimer = new Timer(PingCallback, null, 0, _Cycle);
		}
		#endregion

		#region Public Method : void Restart(int cycle)
		/// <summary>重新啟動</summary>
		/// <param name="cycle">測試週期, 單位豪秒。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">測試週期不得小於100豪秒。</exception>
		public void Restart(int cycle)
		{
			_DoneTimes = 0;
			if (cycle != this.Cycle)
				this.Cycle = cycle;
			else
				Restart();
		}
		#endregion

		#region Public Static Method : PingResultEventArgs Ping(string hostNameOrAddress)
		/// <summary>直接對主機傳送網際網路控制訊息通訊協定 (ICMP)</summary>
		/// <param name="hostNameOrAddress">System.String，識別 ICMP 回應訊息的目標電腦。指定給這個參數的值可以是主機名稱或 IP 位址的字串表示。</param>
		/// <returns>PingResultEventArgs : 回應內容</returns>
		/// <remarks>本函示的TTL(Time to Live)預設為64</remarks>
		public static PingResultEventArgs Ping(string hostNameOrAddress)
		{
			PingResultEventArgs arg = null;
			using (Ping pingSender = new Ping())
			{
				PingOptions options = new PingOptions(64, true);
				string data = "a".PadLeft(32, 'a');
				byte[] buffer = Encoding.ASCII.GetBytes(data);
				try
				{
					PingReply reply = pingSender.Send(hostNameOrAddress, 120, buffer, options);
					arg = new PingResultEventArgs(reply);
				}
				catch (Exception ex)
				{
					arg = new PingResultEventArgs(ex);
				}
			}
			return arg;
		}
		#endregion
	}
}
