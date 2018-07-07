using System.Drawing;
using System.Windows.Forms;

namespace CJF.Utility.WinKits
{
    #region Internal Static Class : Extensions
    static class Extensions
    {
        #region Public Static Method : Point LeftTop(this Padding padding)
        /// <summary>取得與邊框距離的左上角座標點。</summary>
        /// <param name="padding">邊界資訊。</param>
        /// <returns>此邊界資訊的左上角座標點。</returns>
        public static Point LeftTop(this Padding padding)
        {
            return new Point(padding.Left, padding.Top);
        }
        #endregion

        #region Public Static Method : SizeF Gain(this Size size, float gain)
        /// <summary>將此 System.Drawing.Size 以 gain 倍率放大或縮小進行計算。</summary>
        /// <param name="size">欲計算 System.Drawing.Size 結構。</param>
        /// <param name="gain">放大或縮小倍率。</param>
        /// <returns>計算完畢的 System.Drawing.SizeF 結構。</returns>
        public static SizeF Gain(this Size size, float gain)
        {
            return new SizeF(size.Width * gain, size.Height * gain);
        }
        #endregion

        #region Public Static Method : SizeF Gain(this SizeF size, float gain)
        /// <summary>將此 System.Drawing.SizeF 以 gain 倍率放大或縮小進行計算。</summary>
        /// <param name="size">欲計算 System.Drawing.SizeF 結構。</param>
        /// <param name="gain">放大或縮小倍率。</param>
        /// <returns>計算完畢的 System.Drawing.SizeF 結構。</returns>
        public static SizeF Gain(this SizeF size, float gain)
        {
            return new SizeF(size.Width * gain, size.Height * gain);
        }
        #endregion

        #region Public Static Method : Size Gain(this Size size, int gain)
        /// <summary>將此 System.Drawing.Size 以 gain 倍率放大或縮小進行計算。</summary>
        /// <param name="size">欲計算 System.Drawing.Size 結構。</param>
        /// <param name="gain">放大或縮小倍率。</param>
        /// <returns>計算完畢的 System.Drawing.Size 結構。</returns>
        public static Size Gain(this Size size, int gain)
        {
            return new Size(size.Width * gain, size.Height * gain);
        }
        #endregion

        #region Public Static Method : Padding Gain(this Padding padding, float gain)
        public static Padding Gain(this Padding padding, float gain)
        {
            if (padding.All == -1)
                return new Padding((int)(padding.Left * gain), (int)(padding.Top * gain), (int)(padding.Right * gain), (int)(padding.Bottom * gain));
            else
                return new Padding((int)(padding.All * gain));
        }
        #endregion
    }
    #endregion
}
