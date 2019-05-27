using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.ComponentModel;
using System.Collections;

namespace CJF.Utility.WinKits
{
    public class ExtTreeView : TreeView
    {

        /// <summary>[已隱藏]取得或設定值，指出反白顯示的選取範圍是否跨過樹狀檢視控制項寬度。</summary>
        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FullRowSelect { get; set; } = true;

        /// <summary>設定或取得欄位分隔空間寬度大小，單位像素。</summary>
        [Browsable(true), DefaultValue(5)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Category("Appearance"), Description("設定或取得欄位分隔空間寬度大小，單位像素。")]
        public int SpareWidth { get; set; } = 5;



        #region Public Construct Method : ColumnTreeView()
        /// <summary>初始化 CJF.Utility.WinKits.ColumnTreeView 類別的新執行個體。</summary>
        public ExtTreeView() : base()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }
        #endregion

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            base.OnDrawNode(e);
        }

        protected override void OnLeave(EventArgs e)
        {
            base.OnLeave(e);
        }
    }

    public class ExtTreeColumnCollection : ICollection<ExtTreeColumn>
    {

        private List<ExtTreeColumn> _Columns = null;

        public ExtTreeColumnCollection()
        {
            _Columns = new List<ExtTreeColumn>();
        }

        public ExtTreeColumn this[string name] => _Columns.Find(c => c.Name.Equals(name));
        public ExtTreeColumn this[int index] => _Columns[index];

        public int Count => _Columns.Count;

        public bool IsReadOnly => false;

        public void Add(ExtTreeColumn item)
        {
            //if (_Columns.Exists(c => c.Name.Equals(item.Name)))
            //    throw new exception
            _Columns.Add(item);
        }
        public void Clear() { _Columns.Clear(); }
        public bool Contains(ExtTreeColumn item) { return _Columns.Contains(item); }
        public void CopyTo(ExtTreeColumn[] array, int arrayIndex) { _Columns.CopyTo(array, arrayIndex); }
        public IEnumerator<ExtTreeColumn> GetEnumerator() { return _Columns.GetEnumerator(); }
        public bool Remove(ExtTreeColumn item) { return _Columns.Remove(item); }
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
    }

    public class ExtTreeColumn
    {
        public string Name { get; private set; } = null;
        public ExtTreeViewColumnStyles ColumeStyle { get; private set; } = ExtTreeViewColumnStyles.CheckBox;

        public ExtTreeColumn(string name, ExtTreeViewColumnStyles style)
        {
            Name = name;
            ColumeStyle = style;
        }
    }

    public class ExtTreeNodeCell
    {
        public string ColumnName { get; private set; } = string.Empty;
        public CheckState CheckState { get; set; } = CheckState.Unchecked;

        public ExtTreeNodeCell(string columnName)
        {
            ColumnName = columnName;
        }

        public ExtTreeNodeCell(string columnName, CheckState state) : this(columnName)
        {
            CheckState = state;
        }
    }

    public class ExtTreeNode : TreeNode
    {

    }

    #region Public Enum : ExtTreeViewColumnStyles
    /// <summary>指定欄位種類。</summary>
    public enum ExtTreeViewColumnStyles
    {
        /// <summary>分隔空間。</summary>
        Spare = 0,
        /// <summary>多選核選欄。</summary>
        CheckBox = 1,
        /// <summary>單選核選欄。</summary>
        OptionRadio = 2,
    }
    #endregion
}
