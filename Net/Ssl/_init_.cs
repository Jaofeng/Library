using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace CJF.Net.Ssl
{
    #region Public Static Class : SslTcpUtils
    /// <summary>SSL 憑證相關工具。</summary>
    public static class SslTcpUtils
    {
        #region Public Static Method : bool[] CertificateExistsInStore(string subName, StoreLocation location, StoreName storeName)
        /// <summary>於本機憑證存放區中尋找指定的憑證是否存在。</summary>
        /// <param name="subName">憑證的主旨辨別名稱。</param>
        /// <param name="location">指定 X.509 憑證存放區的位置。</param>
        /// <param name="storeName">指定要開啟之 X.509 憑證存放區的名稱。</param>
        /// <returns>bool[] 型別，0:exists, 1:valid。</returns>
        public static bool[] CertificateExistsInStore(string subName, StoreLocation location, StoreName storeName)
        {
            X509Store store = new X509Store(storeName, location);
            store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
            X509Certificate2Collection x2c = store.Certificates.Find(X509FindType.FindBySubjectName, subName, false);
            store.Close();
            if (x2c.Count != 0)
                return new bool[] { true, x2c[0].Verify() };
            else
                return new bool[] { false, false };
        }
        #endregion

        #region Public Static Method : bool[] CertificateExistsInStore(string subName)
        /// <summary>於本機憑證存放區中尋找指定的憑證是否存在。</summary>
        /// <param name="subName">憑證的主旨辨別名稱。</param>
        /// <returns>bool[] 型別，0:exists, 1:valid。</returns>
        public static bool[] CertificateExistsInStore(string subName)
        {
            X509Store store = null;
            X509Certificate2Collection x2c = null;
            foreach (StoreName storeName in (StoreName[])Enum.GetValues(typeof(StoreName)))
            {
                foreach (StoreLocation location in (StoreLocation[])Enum.GetValues(typeof(StoreLocation)))
                {
                    store = new X509Store(storeName, location);
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    x2c = store.Certificates.Find(X509FindType.FindBySubjectName, subName, false);
                    store.Close();
                    if (x2c.Count != 0)
                    {
                        return new bool[] { true, x2c[0].Verify() };
                    }
                }
            }
            return new bool[] { false, false };
        }
        #endregion

        #region Public Static Method : bool SearchCertificateInStore(string subName, out StoreLocation location, out StoreName storeName)
        /// <summary>於本機憑證存放區中尋找指定的憑證，並回傳該憑證是否有效與存放位置。</summary>
        /// <param name="subName">憑證的主旨辨別名稱。</param>
        /// <param name="location"></param>
        /// <param name="storeName"></param>
        /// <returns>憑證存在時，則回傳 true；否則傳回 false。</returns>
        public static bool SearchCertificateInStore(string subName, out StoreLocation location, out StoreName storeName)
        {
            X509Store store = null;
            X509Certificate2Collection x2c = null;
            foreach (StoreName sn in (StoreName[])Enum.GetValues(typeof(StoreName)))
            {
                foreach (StoreLocation loc in (StoreLocation[])Enum.GetValues(typeof(StoreLocation)))
                {
                    store = new X509Store(sn, loc);
                    store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
                    x2c = store.Certificates.Find(X509FindType.FindBySubjectName, subName, false);
                    store.Close();
                    if (x2c.Count != 0)
                    {
                        location = loc;
                        storeName = sn;
                        return x2c[0].Verify();
                    }
                }
            }
            location = StoreLocation.CurrentUser;
            storeName = StoreName.CertificateAuthority;
            return false;
        }
        #endregion
    }
    #endregion

    #region Public Sealed Class : SslClientInfo
    /// <summary>SSL 連線端點資訊類別。</summary>
    public sealed class SslClientInfo
    {

        /// <summary>建立新的 CJF.Net.CslClientInfo 執行個體。</summary>
        /// <param name="client">連線端點的執行個體。</param>
        /// <exception cref="ArgumentNullException">client 不得為 null。</exception>
        public SslClientInfo(TcpClient client)
        {
            Client = client ?? throw new ArgumentNullException();
            Stream = new SslStream(client.GetStream(), false);
        }
        /// <summary>取得端點的 System.Net.Sockets.TcpClient 執行個體。</summary>
		public TcpClient Client { get; } = null;

        /// <summary>取得端點的 System.Net.Security.SslStream 執行個體。</summary>
        public SslStream Stream { get; } = null;

        /// <summary>取得端點是否已通過驗證。</summary>
        public bool IsAuthenticated { get => Stream?.IsAuthenticated ?? false; }
        /// <summary>取得端點是否已連線。</summary>
        public bool Connected { get => Client?.Connected ?? false; }
        /// <summary>取得這個執行個體的遠端端點資訊。</summary>
        public EndPoint RemoteEndPoint { get => Client?.Client?.RemoteEndPoint; }
        /// <summary>取得這個執行個體的本地端點資訊。</summary>
        public EndPoint LocalEndPoint { get => Client?.Client?.LocalEndPoint; }

        /// <summary>與端點結束連線。</summary>
        public void Close()
        {
            Stream?.Close();
            Client?.Close();
        }
    }
    #endregion

    #region Public Sealed Class : SslClientInfoCollection(ICollection)
    /// <summary>端點集合。</summary>
    public sealed class SslClientInfoCollection : ICollection<SslClientInfo>
    {
        ConcurrentDictionary<EndPoint, SslClientInfo> _Clients = null;

        /// <summary>建立新的端點集合的執行個體。</summary>
        public SslClientInfoCollection()
        {
            _Clients = new ConcurrentDictionary<EndPoint, SslClientInfo>();
        }

        #region Public Properties
        /// <summary>依使用者端點資訊取得 SSL 連線資訊類別。</summary>
        /// <param name="ep">使用者端點資訊。</param>
        /// <returns>SSL 連線資訊類別</returns>
        public SslClientInfo this[EndPoint ep]
        {
            get
            {
                if (_Clients.TryGetValue(ep, out SslClientInfo ssl))
                    return ssl;
                else
                    return null;
            }
        }
        /// <summary>取得在這個執行個體中所包含的元素數。</summary>
        public int Count { get => _Clients.Count; }
        /// <summary>取得這個執行個體是否為唯讀狀態。</summary>
        public bool IsReadOnly { get => false; }
        /// <summary>取得這個集合個體的使用者連線端點資訊。</summary>
        public IEnumerable<EndPoint> EndPoints
        {
            get
            {
                foreach (EndPoint ep in _Clients.Keys)
                    yield return ep;
            }
        }
        /// <summary>取得這個集合個體的使用者連線資訊。</summary>
        public IEnumerable<SslClientInfo> Clients
        {
            get
            {
                foreach (SslClientInfo sci in _Clients.Values)
                    yield return sci;
            }
        }
        #endregion

        #region Public Method : void Add(SslClientInfo item)
        /// <summary>新增使用者連線資訊。</summary>
        /// <param name="item">SSL 端點執行個體。</param>
        /// <exception cref="ArgumentNullException">item 不得為 null。</exception>
        /// <exception cref="ArgumentException">item 的連線端點個體不得為 null。</exception>
        public void Add(SslClientInfo item)
        {
            if (item == null)
                throw new ArgumentNullException();
            if (item.Client == null)
                throw new ArgumentException();
            _Clients.TryAdd(item.RemoteEndPoint, item);
        }
        #endregion

        #region Public Method : SslClientInfo Add(EndPoint ep, TcpClient client)
        /// <summary>新增使用者連線資訊。</summary>
        /// <param name="ep">使用者端點資訊。</param>
        /// <param name="client">用來連線的 System.Net.Sockets.TcpClient 類別。</param>
        /// <returns>回傳依附在 System.Net.Sockets.TcpClient 的 System.Net.Security.SslStream 資料加密串流類別。</returns>
        public SslClientInfo Add(EndPoint ep, TcpClient client)
        {
            SslClientInfo ssl = new SslClientInfo(client);
            _Clients.TryAdd(ep, ssl);
            return ssl;
        }
        #endregion

        #region Public Method : bool Remove(SslClientInfo item)
        /// <summary>移除使用者端點資料。</summary>
        /// <param name="item">欲移除的連線端點資訊。</param>
        /// <returns>如果已成功移除物件，則為 true，否則為 false。</returns>
        public bool Remove(SslClientInfo item)
        {
            if (item == null || item.Client == null)
                return false;
            return _Clients.TryRemove(item.RemoteEndPoint, out SslClientInfo ssl);
        }
        #endregion

        #region Public Method : void Remove(EndPoint ep)
        /// <summary>移除使用者端點資料。</summary>
        /// <param name="ep">欲移除的端點資訊。</param>
        public void Remove(EndPoint ep)
        {
            _Clients.TryRemove(ep, out SslClientInfo client);
        }
        #endregion

        #region Public Method : bool Contains(SslClientInfo item)
        /// <summary>判斷這個執行個體是否包含指定的使用者連線資訊。</summary>
        /// <param name="item">要在這個執行個體中尋找的使用者連線資訊。</param>
        /// <returns>如果這個執行個體包含具有指定索引鍵的項目則為 true，否則為 false。</returns>
        public bool Contains(SslClientInfo item)
        {
            return _Clients.ContainsKey(item.RemoteEndPoint);
        }
        #endregion

        #region Public Method : bool ContainsKey(EndPoint ep)
        /// <summary>判斷這個執行個體是否包含指定的索引鍵(使用者端點資訊)。</summary>
        /// <param name="ep">要在這個執行個體中尋找的索引鍵。</param>
        /// <returns>如果這個執行個體包含具有指定索引鍵的項目則為 true，否則為 false。</returns>
        public bool ContainsKey(EndPoint ep)
        {
            return _Clients.ContainsKey(ep);
        }
        #endregion

        #region Public Method : void CopyTo(SslClientInfo[] array, int index)
        /// <summary>從特定的一微陣列索引開始，複製這個執行個體的使用者連線資訊至 SslClientInfo[]。</summary>
        /// <param name="array">一維 SslClientInfo[] 陣列，是從這個執行個體複製過來的使用者端點資訊之目的端。array 必須有以零起始的索引。</param>
        /// <param name="index">array 中以零起始的索引，是開始複製的位置。</param>
        /// <exception cref="System.ArgumentNullException">array 為 null。</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">index 小於零。</exception>
        /// <exception cref="System.ArgumentException">array 是多維的。-或-這個執行個體元素的數量大於從 index 到目的 array 結尾的可用空間。</exception>
        public void CopyTo(SslClientInfo[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException();
            else if (index < 0)
                throw new ArgumentOutOfRangeException();
            else if (array.Rank != 1 || index + array.Length < _Clients.Count)
                throw new ArgumentException();
            _Clients.Values.CopyTo(array, index);
        }
        #endregion

        #region Public Method : void CopyEndPointTo(EndPoint[] array, int index)
        /// <summary>從特定的索引開始，複製這個執行個體的使用者端點資訊至 System.Net.EndPoint[]。</summary>
        /// <param name="array">一維 System.Net.EndPoint[]，是從這個執行個體複製過來的使用者端點資訊之目的端。</param>
        /// <param name="index">array 中以零起始的索引，是開始複製的位置。</param>
        /// <exception cref="System.ArgumentNullException">array 為 null。</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">index 小於零。</exception>
        /// <exception cref="System.ArgumentException">這個執行個體元素的數量大於從 index 到目的 array 結尾的可用空間。</exception>
        public void CopyEndPointTo(EndPoint[] array, int index)
        {
            if (array == null)
                throw new ArgumentNullException();
            else if (index < 0)
                throw new ArgumentOutOfRangeException();
            else if ((index + array.Length) < _Clients.Count)
                throw new ArgumentException();
            _Clients.Keys.CopyTo(array, index);
        }
        #endregion

        #region Public Method : void Clean()
        /// <summary>將這個執行個體的所有元素移除。</summary>
        public void Clear()
        {
            _Clients.Clear();
        }
        #endregion

        #region Public Method : IEnumerator<SslClientInfo> GetEnumerator()
        /// <summary>傳回會逐一查看集合的列舉程式。</summary>
        /// <returns>列舉值</returns>
        public IEnumerator GetEnumerator()
        {
            return _Clients.Values.GetEnumerator();
        }
        #endregion

        #region IEnumerator<SslClientInfo> IEnumerable<SslClientInfo>.GetEnumerator()
        /// <summary>傳回會逐一查看集合的列舉程式。</summary>
        /// <returns>列舉值</returns>
        IEnumerator<SslClientInfo> IEnumerable<SslClientInfo>.GetEnumerator()
        {
            return _Clients.Values.GetEnumerator();
        }
        #endregion
    }
    #endregion
}
