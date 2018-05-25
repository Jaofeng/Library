using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CJF.Utility;
using CJF.Utility.Extensions;

namespace CJF.Modbus
{
	public class ModbusException : Exception
	{
		/// <summary>錯誤代碼</summary>
		public enum ErrorCodes
		{
			/// <summary>未知的錯誤</summary>
			Unknow = 0,
			/// <summary>錯誤的 FunctionCode。</summary>
			IllegalFunctionCode = 1,
			/// <summary>錯誤的資料位址。</summary>
			IllegalDataAddress = 2,
			/// <summary>錯誤的資料。如回傳的資料長度欄位值與實際長度不符。</summary>
			IllegalDataValue = 3,
			/// <summary>Slave裝置無法執行或執行錯誤。</summary>
			SlaveDeviceFailure = 4,
			/// <summary>長度錯誤</summary>
			LengthError = 5
		}

		public ModbusException(ErrorCodes code)
			: base()
		{
			this.ErrorCode = code;
		}

		/// <summary>取得錯誤碼</summary>
		public ErrorCodes ErrorCode { get; private set; }
	}

	#region Interna; Enum : FunctionCodes(byte)
	/// <summary>Modbus Function Code 列舉</summary>
	internal enum FunctionCodes : byte
	{
		/// <summary>讀取 DO 點位狀態</summary>
		ReadCoil = 1,
		/// <summary>讀取 DI 點位狀態</summary>
		ReadInput = 2,
		/// <summary>讀取類比輸出暫存器的值</summary>
		ReadHoldingRegisters = 3,
		/// <summary>讀取類比輸入暫存器的值</summary>
		ReadInputRegisters = 4,
		/// <summary>設定/寫入單一 DO 點位的狀態</summary>
		ForceSingleCoil = 5,
		/// <summary>設定/寫入單一類比輸出暫存器的值</summary>
		PresetSingleRegister = 6,
		/// <summary>設定/寫入多個 DO 點位的狀態</summary>
		ForceMultiCoils = 15,
		/// <summary>設定/寫入多個類比輸出暫存器的值</summary>
		PresetMultiRegisters = 16
	}
	#endregion

	/*
	 * |-- Transcation ID (2 Bytes) --|	=> MBAP(6Bytes)
	 * |-- Protocol(2 Bytes)        --|
	 * |-- Message Length (2 Bytes) --|
	 * |-- Unit ID (1 Byte)         --| => Message
	 */
	/// <summary>用於 ModbusTCP 的封包格式類別 DataPackage</summary>
	internal class DataPackage
	{
		MessageFormat _Message = null;

		/// <summary>建立一個新的 DataPackage 類別</summary>
		public DataPackage()
		{
			this.Identifier = 0;
		}

		/// <summary>以傳入的封包內容解析成 DataPackage 類別</summary>
		/// <param name="data">欲解析的封包</param>
		private DataPackage(byte[] data)
		{
			if (data.Length > 260)
				throw new ArgumentOutOfRangeException("data", "傳入的資料陣列長度不得大於 260Bytes。");
			if (data.ToUInt16(4, true) != data.Length - 6)
				throw new ModbusException(ModbusException.ErrorCodes.LengthError);
			this.TransactionID = data.ToUInt16(0, true);
			this.Identifier = data.ToUInt16(2, true);
			byte[] msg = new byte[data.Length - 6];
			_Message = new MessageFormat(msg);
		}

		#region Public Properties
		/// <summary>設定或取得「資料識別號碼」，此號碼由 Master 產生，由 Slave 復返。</summary>
		public ushort TransactionID { get; set; }
		/// <summary>設定或取得「通信規約識別號碼」</summary>
		public ushort Identifier { get; set; }
		/// <summary>取得 FunctionCode</summary>
		public FunctionCodes FunctionCode { get { return (_Message != null) ? _Message.FunctionCode : 0; } }
		/// <summary>取得此封包的資料陣列</summary>
		public byte[] Rawdata { get { return (_Message != null) ? _Message.Rawdata : null; } }
		#endregion

		#region Public Method : byte[] ToArray()
		/// <summary>以位元組陣列方式輸出</summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			List<byte> buf = new List<byte>();
			byte[] msg = _Message.ToArray();
			buf.AddRange(TransactionID.GetBytes(true));
			buf.AddRange(Identifier.GetBytes(true));
			buf.AddRange(msg.Length.GetBytes(true));
			buf.AddRange(msg);
			return buf.ToArray();
		}
		#endregion

		public static DataPackage[] AnalysisPackage(byte[] data)
		{

			return null;
		}

	}

	/// <summary>
	/// 
	/// </summary>
	class MessageFormat
	{
		/// <summary>以傳入的位元組陣列建立一個新的 ProtocolDataUnit 類別</summary>
		/// <param name="data">傳入的 Message 資料陣列</param>
		/// <exception cref="ArgumentNullException">pdu 不得為 null 值。</exception>
		/// <exception cref="ArgumentOutOfRangeException">傳入的資料陣列長度不得大於 254Bytes。</exception>
		/// <exception cref="ModbusException">參閱 ErrorCode 屬性</exception>
		public MessageFormat(byte[] data)
		{
			if (data == null)
				throw new ArgumentNullException();
			if (data.Length > 254)
				throw new ArgumentOutOfRangeException("pdu", "傳入的資料陣列長度不得大於 254Bytes。");
			if (!Enum.IsDefined(typeof(FunctionCodes), data[0]))
				throw new ModbusException(ModbusException.ErrorCodes.IllegalFunctionCode);
			this.FunctionCode = (FunctionCodes)data[0];
			this.Rawdata = new byte[data.Length - 1];
			Array.Copy(data, 1, this.Rawdata, 0, this.Rawdata.Length);
		}

		public MessageFormat(byte unitId, FunctionCodes code, byte[] data)
		{
			this.UnitID = unitId;
			this.FunctionCode = code;
			this.Rawdata = new byte[data.Length];
			Array.Copy(data, 0, this.Rawdata, 0, this.Rawdata.Length);
		}

		/// <summary>設定或取得設備單位代碼</summary>
		public byte UnitID { get; set; }
		/// <summary>取得此 PDU 的 FunctionCode 列舉值</summary>
		public FunctionCodes FunctionCode { get; private set; }
		/// <summary>取得此 PDU 的資料</summary>
		public byte[] Rawdata { get; private set; }

		/// <summary>以位元組陣列方式輸出</summary>
		/// <returns></returns>
		public byte[] ToArray()
		{
			byte[] arr = new byte[this.Rawdata.Length + 2];
			arr[0] = this.UnitID;
			arr[1] = (byte)this.FunctionCode;
			Array.Copy(arr, 2, this.Rawdata, 0, this.Rawdata.Length);
			return arr;
		}
	}
}
