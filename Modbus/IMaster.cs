using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CJF.Modbus
{
	/// <summary>所有 Modbus Master 類的類別共用的界面</summary>
	public interface IMaster : IDisposable
	{
		/// <summary>使用 FunctionCode-1 讀取數位輸出(Coil)狀態</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>數位輸出(Coil)點位狀態陣列</returns>
		bool[] ReadCoils(ushort address, ushort numberOfPoints);
		/// <summary>使用 FunctionCode-2 讀取數位輸入(Input)狀態</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>數位輸入(Input)點位狀態陣列</returns>
		bool[] ReadInputs(ushort address, ushort numberOfPoints);
		/// <summary>使用 FunctionCode-3 讀取類比輸出暫存器(Holding Registers)數值</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>類比輸出暫存器(Holding Registers)數值陣列</returns>
		ushort[] ReadHoldingRegisters(ushort address, ushort numberOfPoints);
		/// <summary>使用 FunctionCode-4 讀取類比輸入暫存器(Input Registers)數值</summary>
		/// <param name="address">開始讀取的位址</param>
		/// <param name="numberOfPoints">讀取的點位數</param>
		/// <returns>類比輸入暫存器(Input Registers)數值陣列</returns>
		ushort[] ReadInputRegisters(ushort address, ushort numberOfPoints);
		/// <summary>使用 FunctionCode-5 寫入單一數位輸出(Coil)點位狀態</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="value">寫入值</param>
		void WriteSingleCoil(ushort address, bool value);
		/// <summary>使用 FunctionCode-6 寫入單一類比輸出暫存器(Holding Register)數值</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="value">寫入值</param>
		void WriteSingleRegister(ushort address, ushort value);
		/// <summary>使用 FunctionCode-15 寫入多個數位輸出(Coil)點位狀態</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="data">欲寫入的點位狀態陣列</param>
		void WriteMultipleCoils(ushort address, bool[] data);
		/// <summary>使用 FunctionCode-16 寫入多個類比輸出暫存器(Holding Register)數值</summary>
		/// <param name="address">寫入的位址</param>
		/// <param name="data">欲寫入的數值陣列</param>
		void WriteMultipleRegisters(ushort address, ushort[] data);
	}
}
