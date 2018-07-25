using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CJF.Utility
{
    #region Public Enum : Tristate(short)
    /// <summary>三態列舉</summary>
    public enum Tristate : short
    {
        /// <summary>否</summary>
        No_False = -1,
        /// <summary>未定義</summary>
        None = 0,
        /// <summary>是</summary>
        Yes_True = 1
    }
    #endregion
}
