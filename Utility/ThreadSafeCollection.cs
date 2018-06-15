using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CJF.Utility.ThreadSafe
{
	#region Public Class : ThreadSafeReaderWriterLock
	/// <summary>多執行緒鎖定機制類別</summary>
	public class ThreadSafeReaderWriterLock : IDisposable
	{
		// Fields
		private readonly ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
		private readonly CountdownEvent _countdownEvent = new CountdownEvent(1);

		// Constructor
		/// <summary></summary>
		public ThreadSafeReaderWriterLock() { }

		/// <summary></summary>
		public void Dispose()
		{
			_countdownEvent.Signal();
			_countdownEvent.Wait();
			_countdownEvent.Dispose();
			_readerWriterLock.Dispose();
		}

		// Methods
		/// <summary>進入讀取鎖定</summary>
		public void EnterReadLock()
		{
			_countdownEvent.AddCount();
			_readerWriterLock.EnterReadLock();
		}

		/// <summary>離開讀取鎖定</summary>
		public void ExitReadLock()
		{
			_readerWriterLock.ExitReadLock();
			_countdownEvent.Signal();
		}

		/// <summary>進入寫入鎖定</summary>
		public void EnterWriteLock()
		{
			_countdownEvent.AddCount();
			_readerWriterLock.EnterWriteLock();
		}

		/// <summary>離開寫入鎖定</summary>
		public void ExitWriteLock()
		{
			_readerWriterLock.ExitWriteLock();
			_countdownEvent.Signal();
		}
	}
	#endregion

	#region Public Sealed Class : ThreadSafeEnumerator<T>
	/// <summary></summary>
	/// <typeparam name="T"></typeparam>
	public sealed class ThreadSafeEnumerator<T> : IEnumerator<T>, IDisposable
	{
		// Fields         
		private readonly ThreadSafeReaderWriterLock _readerWriterLock = null;
		private readonly IEnumerator<T> _component = null;
		private readonly object _syncRoot = new object();
		private bool _disposed = false;

		// Constructor
		/// <summary></summary>
		/// <param name="getEnumeratorDelegate"></param>
		/// <param name="readerWriterLock"></param>
		public ThreadSafeEnumerator(Func<IEnumerator<T>> getEnumeratorDelegate, ThreadSafeReaderWriterLock readerWriterLock)
		{
			#region Require
			if (getEnumeratorDelegate == null) throw new ArgumentNullException();
			if (readerWriterLock == null) throw new ArgumentNullException();
			#endregion

			// ReaderWriterLock
			_readerWriterLock = readerWriterLock;
			_readerWriterLock.EnterReadLock();

			// Component
			_component = getEnumeratorDelegate();
		}

		/// <summary></summary>
		public void Dispose()
		{
			// Require
			lock (_syncRoot)
			{
				if (_disposed == true) return;
				_disposed = true;
			}

			// Component
			_component.Dispose();

			// ReaderWriterLock            
			_readerWriterLock.ExitReadLock();
		}


		// Properties
		/// <summary></summary>
		public T Current { get { return _component.Current; } }
		object System.Collections.IEnumerator.Current { get { return this.Current; } }

		// Methods
		/// <summary></summary>
		public bool MoveNext() { return _component.MoveNext(); }
		/// <summary></summary>
		public void Reset() { _component.Reset(); }
	}
	#endregion

	#region Public Class : ThreadSafeEnumerable<T>
	/// <summary></summary>
	/// <typeparam name="T"></typeparam>
	public class ThreadSafeEnumerable<T> : IEnumerable<T>, IDisposable
	{
		// Fields         
		private readonly ThreadSafeReaderWriterLock _readerWriterLock = null;
		private readonly IEnumerable<T> _component = null;
		private readonly object _syncRoot = new object();
		private bool _disposed = false;

		// Constructor
		/// <summary></summary>
		protected ThreadSafeEnumerable() : this(new List<T>()) { }
		/// <summary></summary>
		protected ThreadSafeEnumerable(IEnumerable<T> component)
		{
			#region Require
			if (component == null) throw new ArgumentNullException();
			#endregion

			// ReaderWriterLock
			_readerWriterLock = new ThreadSafeReaderWriterLock();
			// Component
			_component = component;
		}
		/// <summary></summary>
		public void Dispose()
		{
			// Require
			lock (_syncRoot)
			{
				if (_disposed == true) return;
				_disposed = true;
			}

			// ReaderWriterLock            
			_readerWriterLock.Dispose();
			// Component
			if (_component is IDisposable)
				((IDisposable)_component).Dispose();
		}

		// Methods
		/// <summary></summary>
		protected void EnterReadLock() { _readerWriterLock.EnterReadLock(); }
		/// <summary></summary>
		protected void ExitReadLock() { _readerWriterLock.ExitReadLock(); }
		/// <summary></summary>
		protected void EnterWriteLock() { _readerWriterLock.EnterWriteLock(); }
		/// <summary></summary>
		protected void ExitWriteLock() { _readerWriterLock.ExitWriteLock(); }
		/// <summary></summary>
		public IEnumerator<T> GetEnumerator() { return new ThreadSafeEnumerator<T>(_component.GetEnumerator, _readerWriterLock); }
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	}
	#endregion

	#region Public Class : ThreadSafeCollection<T>
	/// <summary>表示可以依照索引存取的強型別物件清單。本類別為執行緒安全類別。</summary>
	/// <typeparam name="T">集合清單中元素的型別。</typeparam>
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	public class ThreadSafeCollection<T> : ThreadSafeEnumerable<T>, ICollection<T>
	{
		// Fields         
		private readonly ICollection<T> _Items = null;

		#region Constructor
		/// <summary>初始化 CJF.Utility.ThreadSafe.ThreadSafeCollection&lt;T&gt; 類別的新執行個體，其為空白且具有預設的初始容量。</summary>
		public ThreadSafeCollection()
		{
			_Items = new List<T>();
		}
		#endregion

		#region Properties
		#region Property : int Count
		/// <summary>取得集合中實際包含的元素數目。</summary>
		public int Count
		{
			get
			{
				try
				{
					this.EnterReadLock();
					return _Items.Count;
				}
				finally { this.ExitReadLock(); }
			}
		}
		#endregion

		#region Property : bool IsReadOnly
		/// <summary>
		/// 取得值，指出此集合類別是否為唯讀。
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				try
				{
					this.EnterReadLock();
					return _Items.IsReadOnly;
				}
				finally { this.ExitReadLock(); }
			}
		}
		#endregion
		#endregion

		#region Public Method : void Add(T item)
		/// <summary>將物件加入至集合中的結尾。</summary>
		/// <param name="item">要加入至集合中結尾的物件。參考型別的值可以是 null。</param>
		public void Add(T item)
		{
			try
			{
				this.EnterWriteLock();
				_Items.Add(item);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : bool Remove(T item)
		/// <summary>
		/// 移除集合中特定物件的第一個相符項目。
		/// </summary>
		/// <param name="item">要從集合中移除的物件。參考型別的值可以是 null。</param>
		/// <returns>如果成功移除 item 則為 true，否則為 false。如果在集合中找不到 item，則這個方法也會傳回 false。</returns>
		public bool Remove(T item)
		{
			try
			{
				this.EnterWriteLock();
				return _Items.Remove(item);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void Clear()
		/// <summary>
		/// 將所有元素從集合中移除。
		/// </summary>
		public void Clear()
		{
			try
			{
				this.EnterWriteLock();
				_Items.Clear();
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : bool Contains(T item)
		/// <summary>
		/// 判斷集合中是否包含特定值。
		/// </summary>
		/// <param name="item">要在集合中尋找的物件。</param>
		/// <returns>如果在集合中找到 item，則為 true，否則為 false。</returns>
		public bool Contains(T item)
		{
			try
			{
				this.EnterReadLock();
				return _Items.Contains(item);
			}
			finally { this.ExitReadLock(); }
		}
		#endregion

		#region Public Method : void CopyTo(T[] array, int arrayIndex)
		/// <summary>
		/// 從特定的 System.Array 索引開始，複製集合中的項目至 System.Array。
		/// </summary>
		/// <param name="array">一維 System.Array，是從集合中複製過來的元素之目的端。System.Array 必須有以零起始的索引。</param>
		/// <param name="arrayIndex">中以零起始的索引，位於複製開始的位置。</param>
		/// <exception cref="System.ArgumentNullException">array 為 null。</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">arrayIndex 小於 0。</exception>
		/// <exception cref="System.ArgumentException">array 是多維的。- 或 -
		/// 來源集合中元素的數量大於從 arrayIndex 到目的 array 結尾的可用空間。- 或 -
		/// T 型別無法自動轉換成目的 array 的型別。</exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			try
			{
				this.EnterReadLock();
				if (array == null)
					throw new ArgumentNullException("array", "array 為 null。");
				if (arrayIndex < 0)
					throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex 小於 0。");
				if (array.Rank != 1 || (arrayIndex + array.Length) > _Items.Count)
					throw new ArgumentException();
				_Items.CopyTo(array, arrayIndex);
			}
			finally { this.ExitReadLock(); }
		}
		#endregion
	}
	#endregion

	#region Public Class : ThreadSafeList<T>
	/// <summary>
	/// 表示可以依照索引存取的強型別物件清單。提供搜尋、排序和管理清單的方法。本類別為執行緒安全類別。
	/// </summary>
	/// <typeparam name="T">集合清單中元素的型別。</typeparam>
	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	public class ThreadSafeList<T> : ThreadSafeCollection<T>, IList<T>
	{
		// Fields         
		private readonly List<T> _Items = null;

		#region Constructor
		/// <summary>初始化 CJF.Utility.ThreadSafe.ThreadSafeList&lt;T&gt; 類別的新執行個體，其為空白且具有預設的初始容量。</summary>
		public ThreadSafeList()
		{
			_Items = new List<T>();
		}

		/// <summary>
		/// 初始化 CJF.Utility.ThreadSafe.ThreadSafeList&lt;T&gt; 類別的新執行個體，其包含從指定之集合複製的元素，且具有容納複製之元素數目的足夠容量。
		/// </summary>
		/// <param name="collection">將其元素複製到新清單的來源集合。</param>
		public ThreadSafeList(IEnumerable<T> collection)
		{
			_Items = new List<T>();
			this.AddRange(collection);
		}
		/// <summary>
		/// 初始化 CJF.Utility.ThreadSafe.ThreadSafeList&lt;T&gt; 類別的新執行個體，這個執行個體是空白且可具有指定的初始容量。
		/// </summary>
		/// <param name="capacity">新清單一開始能夠儲存的項目個數。</param>
		public ThreadSafeList(int capacity)
		{
			_Items = new List<T>(capacity);
		}
		#endregion

		#region Properties
		#region Property : T this[int index]
		/// <summary>取得或設定指定之索引處的元素。</summary>
		/// <param name="index">要取得或設定之項目的以零為起始的索引。</param>
		/// <returns>指定之索引處的項目。</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -index 等於或大於集合中的物件數量。</exception>
		public T this[int index]
		{
			get
			{
				try
				{
					this.EnterReadLock();
					if (index < 0 || index >= _Items.Count)
						throw new ArgumentOutOfRangeException("index");
					return _Items[index];
				}
				finally { this.ExitReadLock(); }
			}
			set
			{
				try
				{
					this.EnterWriteLock();
					if (index < 0 || index >= _Items.Count)
						throw new ArgumentOutOfRangeException("index");
					_Items[index] = value;
				}
				finally { this.ExitWriteLock(); }
			}
		}
		#endregion

		#region Property : int Capacity
		/// <summary>
		/// 在不需要調整大小之下，取得或設定內部資料結構可以保存的元素總數。
		/// </summary>
		public int Capacity
		{
			get
			{
				try
				{
					this.EnterReadLock();
					return _Items.Capacity;
				}
				finally { this.ExitReadLock(); }
			}
			set
			{
				try
				{
					this.EnterWriteLock();
					_Items.Capacity = value;
				}
				finally { this.ExitWriteLock(); }
			}
		}
		#endregion
		#endregion

		#region Public Method : void AddRange(IEnumerable<T> collection)
		/// <summary>將特定集合的元素加入至集合中的結尾。</summary>
		/// <param name="collection">集合，其元素應加入至集合中的結尾。集合本身不能是 null，但它可以包含 null 的元素，如果型別 T 是參考型別。</param>
		/// <exception cref="System.ArgumentNullException">collection 為 null。</exception>
		public void AddRange(IEnumerable<T> collection)
		{
			try
			{
				this.EnterWriteLock();
				_Items.AddRange(collection);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : bool Exists(Predicate<T> match)
		/// <summary>判斷集合中是否包含符合指定之述詞 (Predicate) 所定義之條件的元素。</summary>
		/// <param name="match">定義要搜尋元素之條件的 System.Predicate&lt;T&gt; 委派。</param>
		/// <returns>如果集合中包含的一個或多個元素符合指定之述詞所定義的條件，則為 true，否則為 false。</returns>
		public bool Exists(Predicate<T> match)
		{
			if (match == null) throw new ArgumentNullException();
			try
			{
				this.EnterReadLock();
				return _Items.Exists(match);
			}
			finally { this.ExitReadLock(); }
		}
		#endregion

		#region Public Method : T Find(Predicate<T> match)
		/// <summary>
		/// 搜尋符合指定之述詞所定義的條件之元素，並傳回整個集合內第一個相符的元素。
		/// </summary>
		/// <exception cref="System.ArgumentNullException">match 為 null。</exception>
		/// <param name="match">定義要搜尋元素之條件的 System.Predicate&lt;T&gt; 委派。</param>
		/// <returns>第一個符合指定之述詞所定義的條件之元素 (如果找到的話)，否則為型別 T 的預設值。</returns>
		public T Find(Predicate<T> match)
		{
			if (match == null) throw new ArgumentNullException();
			try
			{
				this.EnterReadLock();
				return _Items.Find(match);
			}
			finally { this.ExitReadLock(); }
		}
		#endregion

		#region Public Method : ThreadSafeList<T> FindAll(Predicate<T> match)
		/// <summary>
		/// 擷取符合指定之述詞所定義的條件之所有元素。
		/// </summary>
		/// <param name="match">定義要搜尋元素之條件的 System.Predicate&lt;T&gt; 委派。</param>
		/// <returns>
		///     CJF.Utility.ThreadSafe.ThreadSafeList&lt;T&gt;，其中包含符合指定之述詞所定義的條件之所有元素 (如果有找到的話)，否則為空的
		///     CJF.Utility.ThreadSafe.ThreadSafeList&lt;T&gt;。
		/// </returns>
		public ThreadSafeList<T> FindAll(Predicate<T> match)
		{
			if (match == null) throw new ArgumentNullException();
			try
			{
				this.EnterReadLock();
				return new ThreadSafeList<T>(_Items.FindAll(match));
			}
			finally { this.ExitReadLock(); }
		}
		#endregion

		#region Public Method : void ForEach(Action<T> action)
		/// <summary>在集合中的每一個項目上執行指定之動作。</summary>
		/// <param name="action">要在集合中的每一個項目上執行的 System.Action&lt;T&gt; 委派。</param>
		public void ForEach(Action<T> action)
		{
			try
			{
				this.EnterWriteLock();
				_Items.ForEach(action);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void Insert(int index, T item)
		/// <summary>將項目插入集合中指定的索引處。</summary>
		/// <param name="index">應在該處插入 item 之以零起始的索引。</param>
		/// <param name="item">要插入的物件。參考型別的值可以是 null。</param>
		public void Insert(int index, T item)
		{
			try
			{
				this.EnterWriteLock();
				_Items.Insert(index, item);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : int IndexOf(T item)
		/// <summary>
		/// 搜尋指定的物件，並傳回整個 CJF.Utility.Collections.ThreadSafeList&lt;T&gt; 中第一個相符項目之以零起始的索引。
		/// </summary>
		/// <param name="item">要在 CJF.Utility.Collections.ThreadSafeList&lt;T&gt; 中尋找的物件。參考型別的值可以是 null。</param>
		/// <returns>如果有找到，則是在整個 CJF.Utility.Collections.ThreadSafeList&lt;T&gt; 內，item 之第一個相符項目的以零起始的索引，否則為 -1</returns>
		public int IndexOf(T item)
		{
			try
			{
				this.EnterReadLock();
				return _Items.IndexOf(item);
			}
			finally { this.ExitReadLock(); }
		}
		#endregion

		#region Public Method : int FindIndex(Predicate<T> match)
		/// <summary>搜尋符合指定之述詞所定義的條件之元素，並傳回整個集合內第一個相符元素的索引值。</summary>
		/// <exception cref="System.ArgumentNullException">match 為 null。</exception>
		/// <param name="match">定義要搜尋元素之條件的 System.Predicate&lt;T&gt; 委派。</param>
		/// <returns>第一個符合指定之述詞所定義的條件之元素 (如果找到的話)，否則為型別 T 的預設值。</returns>
		public int FindIndex(Predicate<T> match)
		{
			if (match == null) throw new ArgumentNullException();
			try
			{
				this.EnterReadLock();
				return _Items.FindIndex(match);
			}
			finally { this.ExitReadLock(); }
		}
		#endregion

		#region Public Method : void RemoveAt(int index)
		/// <summary>
		/// 移除 CJF.Utility.Collections.ThreadSafeList&lt;T&gt; 中指定之索引處的項目。
		/// </summary>
		/// <param name="index">要移除元素之以零起始的索引。</param>
		/// <exception cref="System.ArgumentOutOfRangeException">index 小於 0。- 或 -index 等於或大於 CJF.Utility.Collections.ThreadSafeList&lt;T&gt;.Count。</exception>
		public void RemoveAt(int index)
		{
			try
			{
				this.EnterWriteLock();
				if (index <= -1 || index >= _Items.Count)
					throw new ArgumentOutOfRangeException("index");
				_Items.RemoveAt(index);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : int RemoveAll(Predicate<T> match)
		/// <summary>移除符合指定之述詞所定義的條件之所有項目。</summary>
		/// <param name="match">定義要移除項目之條件的 System.Predicate&lt;T&gt; 委派。</param>
		/// <returns>CJF.Utility.Collections.ThreadSafeList&lt;T&gt; 中已移除的項目數。</returns>
		public int RemoveAll(Predicate<T> match)
		{
			if (match == null) throw new ArgumentNullException();
			try
			{
				this.EnterWriteLock();
				return _Items.RemoveAll(match);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void RemoveRange(int index, int count)
		/// <summary>
		/// 從集合中移除的元素範圍。
		/// </summary>
		/// <param name="index">要移除之元素範圍內之以零起始的起始索引。</param>
		/// <param name="count">要移除的元素數目。</param>
		public void RemoveRange(int index, int count)
		{
			try
			{
				this.EnterWriteLock();
				_Items.RemoveRange(index, count);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void Reverse()
		/// <summary>
		/// 反向整個集合中元素的順序。
		/// </summary>
		public void Reverse()
		{
			try
			{
				this.EnterWriteLock();
				_Items.Reverse();
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void Reverse(int index, int count)
		/// <summary>
		/// 反向指定範圍中元素的順序。
		/// </summary>
		/// <param name="index">要反向範圍內之以零起始的起始索引。</param>
		/// <param name="count">要反向範圍中的元素數。</param>
		public void Reverse(int index, int count)
		{
			try
			{
				this.EnterWriteLock();
				_Items.Reverse(index, count);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void Sort()
		/// <summary>
		/// 使用預設比較子來排序在整個集合中的項目。
		/// </summary>
		public void Sort()
		{
			try
			{
				this.EnterWriteLock();
				_Items.Sort();
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : void Sort(Comparison<T> comparison)
		/// <summary>使用指定的 System.Comparison&gt;T&lt; 來排序在整個集合中的項目。</summary>
		/// <param name="comparison">比較元素時所要使用的 System.Comparison&gt;T&lt;。</param>
		public void Sort(Comparison<T> comparison)
		{
			try
			{
				this.EnterWriteLock();
				_Items.Sort(comparison);
			}
			finally { this.ExitWriteLock(); }
		}
		#endregion

		#region Public Method : T[] ToArray()
		/// <summary>
		/// 將集合中的元素複製到新的陣列。
		/// </summary>
		/// <returns>陣列，包含集合中所有項目的複本。</returns>
		public T[] ToArray()
		{
			try
			{
				this.EnterReadLock();
				return _Items.ToArray();
			}
			finally { this.ExitReadLock(); }
		}
		#endregion
	}
	#endregion
}


