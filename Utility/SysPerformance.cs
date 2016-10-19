using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CJF.Utility
{
	/// <summary>
	/// 取得系統負載資料
	/// </summary>
	public class SysPerformance : IDisposable
	{
		Dictionary<string, PerformanceCounter> _AllCounter = null;
		string[] _HardDrivers = null;
		bool _IsDisposed = false;

		#region Construct Method : SysPerformance()
		/// <summary>建立 SysPerformance 類別</summary>
		public SysPerformance()
		{
			this.appName = Path.GetFileNameWithoutExtension(AppDomain.CurrentDomain.FriendlyName);
			_AllCounter = new Dictionary<string, PerformanceCounter>();
			_AllCounter.Add("SysCPU", new PerformanceCounter("Processor", "% Processor Time", "_Total"));
			_AllCounter.Add("MemoryUsed", new PerformanceCounter("Memory", "% Committed Bytes In Use"));
			_AllCounter.Add("FreeMemory", new PerformanceCounter("Memory", "Available KBytes"));
			_AllCounter.Add("CommitMemory", new PerformanceCounter("Memory", "Committed Bytes"));

			_AllCounter.Add("AppCPU", new PerformanceCounter("Process", "% Processor Time", this.appName));
			_AllCounter.Add("AppMemory", new PerformanceCounter("Process", "Private Bytes", this.appName));
			_AllCounter.Add("AppThread",new PerformanceCounter("Process", "Thread Count", this.appName));
			_AllCounter.Add("AppHandle", new PerformanceCounter("Process", "Handle Count", this.appName));

			DriveInfo[] drvs = DriveInfo.GetDrives();
			List<string> hds = new List<string>();
			string did = string.Empty;
			foreach (DriveInfo di in drvs)
			{
				if (di.DriveType == System.IO.DriveType.Fixed)
				{
					did = di.Name.TrimEnd('\\');
					hds.Add(did);
					_AllCounter.Add("HDD_" + did, new PerformanceCounter("LogicalDisk", "Free Megabytes", did));
				}
			}
			_HardDrivers = hds.ToArray();
		}
		/// <summary></summary>
		~SysPerformance() { Dispose(false); }
		#endregion

		#region IDisposable 成員
		/// <summary>釋放CJF.Utility.CustomPerformanceCounter所使用的所有資源</summary>
		/// <param name="disposing">確實釋放所有資源</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_IsDisposed) return;
			if (disposing)
			{
				PerformanceCounter[] pcs = new PerformanceCounter[_AllCounter.Values.Count];
				_AllCounter.Values.CopyTo(pcs, 0);
				foreach (PerformanceCounter pc in pcs)
				{
					pc.Close();
					pc.Dispose();
				}
				_AllCounter.Clear();
				_AllCounter = null;
			}
			_IsDisposed = true;
		}
		/// <summary>釋放CJF.Utility.CustomPerformanceCounter所使用的所有資源</summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Properties
		/// <summary>取得應用程式主檔名</summary>
		public string appName { get; private set; }
		/// <summary>取得整體CPU使用百分比</summary>
		public float SysCpuUsage { get { return _AllCounter["SysCPU"].NextValue(); } }
		/// <summary>取得整體記憶體使用百分比</summary>
		public float SysRamUsage { get { return _AllCounter["MemoryUsed"].NextValue(); } }
		/// <summary>取得整體記憶體剩餘大小, KBytes</summary>
		public float SysRamAvailable { get { return _AllCounter["FreeMemory"].NextValue(); } }
		/// <summary>取得整體記憶體已使用大小, KBytes</summary>
		public float SysRamCommitted { get { return _AllCounter["CommitMemory"].NextValue() / 1024; } }
		/// <summary>取得應用程式CPU使用百分比</summary>
		public float AppCpuUsage { get { return _AllCounter["AppCPU"].NextValue() / Environment.ProcessorCount; } }
		/// <summary>取得應用程式記憶體使用量, KBytes</summary>
		public float AppRamUsed { get { return _AllCounter["AppMemory"].NextValue() / 1024; } }
		/// <summary>取得應用程式執行緒使用量</summary>
		public float AppThreads { get { return _AllCounter["AppThread"].NextValue(); } }
		/// <summary>取得應用程式目前所開啟的控制總數</summary>
		public float AppHandles { get { return _AllCounter["AppHandle"].NextValue(); } }
		/// <summary>取得所有硬碟代碼</summary>
		public string[] HddDrivers { get { return _HardDrivers; } }
		#endregion

		#region Public Method : float HddFreeSpace(string drvName)
		/// <summary>取得硬碟剩餘空間(MBytes)</summary>
		/// <param name="drvName">磁碟機代碼</param>
		/// <returns>剩餘空間(MBytes)</returns>
		public float HddFreeSpace(string drvName)
		{
			try
			{
				if (_AllCounter.ContainsKey("HDD_" + drvName))
					return _AllCounter["HDD_" + drvName].NextValue();
				else
					return -1;
			}
			catch
			{
				return -1;
			}
		}
		#endregion
	}
}
