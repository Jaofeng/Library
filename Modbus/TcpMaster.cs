using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using CJF.Net;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace CJF.Modbus
{
	public class TcpMaster : ModbusBase, IMaster
	{
		#region Public Events
		/// <summary>當與遠端連線時產生</summary>
		public event EventHandler<AsyncClientEventArgs> OnConnected;
		/// <summary>當與遠端斷線時產生</summary>
		public event EventHandler<AsyncClientEventArgs> OnDisconnect;
		#endregion

		#region Private Variables
		/// <summary>記錄器</summary>
		/// <summary>AsyncClient 連線類別</summary>
		AsyncClient _Client = null;
		/// <summary>MBAP 用的發送序號，Byte 0~1</summary>
		int _Identifier = 0;
		/// <summary>是否正在解構中</summary>
		bool _OnDisposing = false;
		#endregion

		/// <summary>建立一個新的 TcpMaster 類別</summary>
		/// <param name="ip"></param>
		/// <param name="port"></param>
		public TcpMaster(string ip, int port = 502)
			: base()
		{
			_SingleCommunication = false;
			this.ReadTimeout = 1500;
			this.WriteTimeout = 0;
			this.Retries = 3;
			InitinalConnection(new IPEndPoint(IPAddress.Parse(ip), port));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ipHost"></param>
		public TcpMaster(IPEndPoint ipHost)
			: base()
		{
			_SingleCommunication = false;
			this.ReadTimeout = 1500;
			this.WriteTimeout = 0;
			this.Retries = 3;
			InitinalConnection(ipHost);
		}
		/// <summary>卸載當前的 TcpMaster 類別</summary>
		~TcpMaster() { Dispose(false); }

		#region Public Peoperties
		#region Public Override Property : int Retries(R/W)
		/// <summary>設定或取得重試次數，預設值 3。</summary>
		public override int Retries
		{
			get { return base.Retries; }
			set { base.Retries = value; }
		}
		#endregion

		#region Public Override Property : int ReadTimeout(R/W)
		/// <summary>設定或取得讀取逾時時間，單位豪秒。預設值 1500ms</summary>
		public override int ReadTimeout
		{
			get { return base.ReadTimeout; }
			set
			{
				base.ReadTimeout = value;
				if (_Client != null && !_Client.IsDisposed && _Client.Socket != null)
					_Client.Socket.ReceiveTimeout = value;
			}
		}
		#endregion

		#region Public Override Property : int WriteTimeout(R/W)
		/// <summary>寫入逾時時間，單位豪秒。預設值 0ms</summary>
		public override int WriteTimeout
		{
			get { return base.WriteTimeout; }
			set
			{
				base.WriteTimeout = value;
				if (_Client != null && !_Client.IsDisposed && _Client.Socket != null)
					_Client.Socket.SendTimeout = value;
			}
		}
		#endregion

		#region Public Property : bool IsConnected(R)
		/// <summary>取得是否已與遠端連線</summary>
		public bool IsConnected { get { return _Client != null && !_Client.IsDisposed && _Client.Socket != null && _Client.IsConnected; } }
		#endregion
		#endregion

		#region Private Method : void InitinalConnection(IPEndPoint ipHost)
		/// <summary>初始化連線類別並連線至遠端</summary>
		/// <param name="ipHost"></param>
		private void InitinalConnection(IPEndPoint ipHost)
		{
			_Client = new AsyncClient(ipHost);
			_Client.Connected += new EventHandler<AsyncClientEventArgs>(Remote_OnConnected);
			_Client.Closed += new EventHandler<AsyncClientEventArgs>(Remote_OnClosed);
			_Client.DataSended += new EventHandler<AsyncClientEventArgs>(Remote_OnDataSended);
			_Client.DataReceived += new EventHandler<AsyncClientEventArgs>(Remote_OnDataReceived);
			_Client.SendFail += new EventHandler<AsyncClientEventArgs>(Remote_OnSendedFail);
			_Client.Connect();
		}
		#endregion

		private void Remote_OnDataReceived(object sender, AsyncClientEventArgs e)
		{
			if (_OnDisposing) return;
			byte[] tmp = new byte[e.Data.Length];
			Array.Copy(e.Data, tmp, tmp.Length);
            Debug.Print($"Received {tmp.Length} bytes.");
            Debug.Print($"Data:{tmp.ToHexString()}");
		}

		#region Private Method : void Remote_OnDataSended(object sender, AsyncClientEventArgs e)
		private void Remote_OnDataSended(object sender, AsyncClientEventArgs e)
		{
			if (_OnDisposing) return;
			byte[] tmp = new byte[e.Data.Length];
			Array.Copy(e.Data, tmp, tmp.Length);
            Debug.Print($"Sended {tmp.Length} bytes.");
            Debug.Print($"Data:{tmp.ToHexString()}");
		}
		#endregion

		#region Private Method : void Remote_OnSendedFail(object sender, AsyncClientEventArgs e)
		private void Remote_OnSendedFail(object sender, AsyncClientEventArgs e)
		{
			if (_OnDisposing) return;
			byte[] tmp = new byte[e.Data.Length];
			Array.Copy(e.Data, tmp, tmp.Length);
            Debug.Print($"Send fail {tmp.Length} bytes.");
            Debug.Print($"Data:{tmp.ToHexString()}");
        }
        #endregion

        #region Private Method : void Remote_OnClosed(object sender, AsyncClientEventArgs e)
        private void Remote_OnClosed(object sender, AsyncClientEventArgs e)
		{
			if (_OnDisposing) return;
			Debug.Print("Remote disconnected");

			#region 產生事件
			if (!_OnDisposing && this.OnDisconnect != null)
			{
				foreach (EventHandler<AsyncClientEventArgs> del in this.OnDisconnect.GetInvocationList())
				{
					if (_OnDisposing) return;
					try { del.BeginInvoke(this, e, new AsyncCallback(EventCallback), del); }
					catch (Exception ex) { Debug.Print(ex.Message); }
				}
			}
			#endregion
		}
		#endregion

		#region private Method : void Remote_OnConnected(object sender, AsyncClientEventArgs e)
		private void Remote_OnConnected(object sender, AsyncClientEventArgs e)
		{
			if (_OnDisposing) return;
			Debug.Print("Remote connected");

			_Client.Socket.SendTimeout = _WriteTimeout;
			_Client.Socket.ReceiveTimeout = _ReadTimeout;

			#region 產生事件
			if (!_OnDisposing && this.OnConnected != null)
			{
				foreach (EventHandler<AsyncClientEventArgs> del in this.OnConnected.GetInvocationList())
				{
					if (_OnDisposing) return;
					try { del.BeginInvoke(this, e, new AsyncCallback(EventCallback), del); }
					catch (Exception ex) { Debug.Print(ex.Message); }
				}
			}
			#endregion
		}
		#endregion

		#region Private Method : void EventCallback(IAsyncResult result)
		private void EventCallback(IAsyncResult result)
		{
			if (_OnDisposing) return;
			try
			{
				EventHandler del = result.AsyncState as EventHandler;
				del.EndInvoke(result);
			}
			catch (ObjectDisposedException) { }
			catch (Exception ex) { Debug.Print(ex.Message); }
		}
		#endregion

		#region Private Method : ushort GetIdentifier()
		private ushort GetIdentifier()
		{
			int v = Interlocked.Increment(ref _Identifier);
			if (v > ushort.MaxValue)
			{
				v = 1;
				Interlocked.Exchange(ref _Identifier, 1);
			}
			return (ushort)v;
		}
		#endregion

		#region IMaster 成員
		#region Public Override Method : void SetCommunicationMode(bool single)
		/// <summary>
		/// 設定是否使用單一執行緒進行讀寫。
		/// <para>如 Slave 為硬體設備，建議使用單一執行緒方式讀寫</para>
		/// </summary>
		/// <param name="single">true:使用單一執行緒；false:使用多執行緒</param>
		public override void SetCommunicationMode(bool single)
		{
			if (_SingleCommunication == single)
				return;
			if (single)
			{
			}
			else
			{
			}
			_SingleCommunication = single;
		}
		#endregion

		#region Public Method : bool[] ReadCoils(ushort address, ushort numberOfPoints)
		/// <summary>使用 FunctionCode-1 讀取數位輸出(Coil)狀態</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>數位輸出(Coil)點位狀態陣列</returns>
		public bool[] ReadCoils(ushort address, ushort numberOfPoints)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : bool[] ReadInputs(ushort address, ushort numberOfPoints)
		/// <summary>使用 FunctionCode-2 讀取數位輸入(Input)狀態</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>數位輸入(Input)點位狀態陣列</returns>
		public bool[] ReadInputs(ushort address, ushort numberOfPoints)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : ushort[] ReadHoldingRegisters(ushort address, ushort numberOfPoints)
		/// <summary>使用 FunctionCode-3 讀取類比輸出暫存器(Holding Registers)數值</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>類比輸出暫存器(Holding Registers)數值陣列</returns>
		public ushort[] ReadHoldingRegisters(ushort address, ushort numberOfPoints)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : ushort[] ReadInputRegisters(ushort address, ushort numberOfPoints)
		/// <summary>使用 FunctionCode-4 讀取類比輸入暫存器(Input Registers)數值</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>類比輸入暫存器(Input Registers)數值陣列</returns>
		public ushort[] ReadInputRegisters(ushort address, ushort numberOfPoints)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : void WriteSingleCoil(ushort address, bool value)
		/// <summary>使用 FunctionCode-5 寫入單一數位輸出(Coil)點位狀態</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="value">寫入值</param>
		public void WriteSingleCoil(ushort address, bool value)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : void WriteSingleRegister(ushort address, ushort value)
		/// <summary>使用 FunctionCode-6 寫入單一類比輸出暫存器(Holding Register)數值</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="value">寫入值</param>
		public void WriteSingleRegister(ushort address, ushort value)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : void WriteMultipleCoils(ushort address, bool[] data)
		/// <summary>使用 FunctionCode-15 寫入多個數位輸出(Coil)點位狀態</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="data">欲寫入的點位狀態陣列</param>
		public void WriteMultipleCoils(ushort address, bool[] data)
		{
			throw new NotImplementedException();
		}
		#endregion

		#region Public Method : void WriteMultipleRegisters(ushort address, ushort[] data)
		/// <summary>使用 FunctionCode-16 寫入多個類比輸出暫存器(Holding Register)數值</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="data">欲寫入的數值陣列</param>
		public void WriteMultipleRegisters(ushort address, ushort[] data)
		{
			throw new NotImplementedException();
		}
		#endregion
		#endregion

		#region Protected Virtual Method : void Dispose(bool disposing)
		/// <summary>釋放 TcpMaster 所使用的資源。 </summary>
		/// <param name="disposing">是否完全釋放</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_OnDisposing = true;
				if (_Client != null)
				{
					try
					{
						_Client.Close();
						_Client.Dispose();
					}
					finally
					{
						_Client = null;
					}
				}
				this.IsDisposed = true;
			}
		}
		#endregion

		#region IDisposable 成員
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
