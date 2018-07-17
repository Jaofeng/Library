using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
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

    #region Public Static Class : PrimaryScreen
    /// <summary>取得系統解析度相關資訊。</summary>
    public static class PrimaryScreen
    {
        #region Win32 API
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr ptr);
        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(
            IntPtr hdc, // handle to DC
            int nIndex // index of capability
        );
        [DllImport("user32.dll", EntryPoint = "ReleaseDC")]
        static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);
        #endregion

        #region DeviceCaps 常量
        const int HORZRES = 8;
        const int VERTRES = 10;
        const int LOGPIXELSX = 88;
        const int LOGPIXELSY = 90;
        const int DESKTOPVERTRES = 117;
        const int DESKTOPHORZRES = 118;
        #endregion

        #region Public Static Property : int DefDpiX(R) & int DefDpiY(R)
        /// <summary>系統預設文字放大率 100% 時為 96 DPI。</summary>
        public static int DefDpiX { get => 96; }
        /// <summary>系統預設文字放大率 100% 時為 96 DPI。</summary>
        public static int DefDpiY { get => 96; }
        #endregion

        #region Public Static Property : WorkingArea(R)
        /// <summary>取得螢幕實際設定的解析度大小。</summary>
        public static Size WorkingArea
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                Size size = new Size();
                size.Width = GetDeviceCaps(hdc, HORZRES);
                size.Height = GetDeviceCaps(hdc, VERTRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return size;
            }
        }
        #endregion

        #region Public Static Property : int DpiX(R)
        /// <summary>取得目前系統 DPI_X 值，文字放大率 100% 時為 96 DPI。</summary>
        public static int DpiX
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int DpiX = GetDeviceCaps(hdc, LOGPIXELSX);
                ReleaseDC(IntPtr.Zero, hdc);
                return DpiX;
            }
        }
        #endregion

        #region Public Static Property : int DpiY(R)
        /// <summary>取得目前系統 DPI_Y 值，文字放大率 100% 時為 96 DPI。</summary>
        public static int DpiY
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                int DpiX = GetDeviceCaps(hdc, LOGPIXELSY);
                ReleaseDC(IntPtr.Zero, hdc);
                return DpiX;
            }
        }
        #endregion

        #region Public Static Property : Size DESKTOP(R)
        /// <summary>取得桌面實際設定的解析度大小。</summary>
        public static Size DESKTOP
        {
            get
            {
                IntPtr hdc = GetDC(IntPtr.Zero);
                Size size = new Size();
                size.Width = GetDeviceCaps(hdc, DESKTOPHORZRES);
                size.Height = GetDeviceCaps(hdc, DESKTOPVERTRES);
                ReleaseDC(IntPtr.Zero, hdc);
                return size;
            }
        }
        #endregion

        #region Public Static Property : float ScaleX(R) & float ScaleY(R)
        /// <summary>取得寬度縮放比。</summary>
        public static float ScaleX => (float)DpiX / DefDpiX;
        /// <summary>取得高度縮放比。</summary>
        public static float ScaleY => (float)DpiY / DefDpiY;
        #endregion
    }
    #endregion

    #region Public Static Class : DpiUtils
    /// <summary>DPI 工具函示集。</summary>
    public static class DpiUtils
    {
        #region Public Static Method : Image ScaleTo(this Image image, float scale)
        /// <summary>將 System.Drawing.Image 執行個體，以 scale 縮放倍率放大或縮小。</summary>
        /// <param name="image">欲放大縮小的 System.Drawing.Image 執行個體。</param>
        /// <param name="scale">縮放倍率。</param>
        /// <returns>縮放過後的 System.Drawing.Image 新執行個體。</returns>
        public static Image ScaleTo(this Image image, float scale)
        {
            if (image == null) return null;
            Bitmap bitmap = new Bitmap((int)(image.Width * scale), (int)(image.Height * scale));
            using (Graphics grp = Graphics.FromImage(bitmap))
            {
                grp.CompositingQuality = CompositingQuality.HighQuality;
                grp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grp.SmoothingMode = SmoothingMode.HighQuality;
                grp.DrawImage(image, 0, 0, bitmap.Width, bitmap.Height);
            }
            return bitmap;
        }
        #endregion

        #region Public Static Method : Bitmap ScaleTo(this Bitmap bitmap, float scale)
        /// <summary>將 System.Drawing.Bitmap 執行個體，以 scale 縮放倍率放大或縮小。</summary>
        /// <param name="bitmap">欲放大縮小的 System.Drawing.Bitmap 執行個體。</param>
        /// <param name="scale">縮放倍率。</param>
        /// <returns>縮放過後的 System.Drawing.Bitmap 新執行個體。</returns>
        public static Bitmap ScaleTo(this Bitmap bitmap, float scale)
        {
            if (bitmap == null) return null;
            Bitmap result = new Bitmap((int)(bitmap.Width * scale), (int)(bitmap.Height * scale));
            using (Graphics grp = Graphics.FromImage(result))
            {
                grp.CompositingQuality = CompositingQuality.HighQuality;
                grp.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grp.SmoothingMode = SmoothingMode.HighQuality;
                grp.DrawImage(bitmap, 0, 0, result.Width, result.Height);
            }
            return result;
        }
        #endregion

        #region Public Static Method : Size ScaleTo(this Size size, float scale)
        /// <summary>將 System.Drawing.Size 結構，以 scale 倍率放大或縮小。</summary>
        /// <param name="size">原尺寸大小。</param>
        /// <param name="scale">縮放倍率。</param>
        /// <returns></returns>
        public static Size ScaleTo(this Size size, float scale)
        {
            return new Size((int)(size.Width * scale), (int)(size.Height * scale));
        }
        #endregion

        #region Public Static Method : int FontPointSizeToDpi(float size)
		/*
		 * 1 pt = 0.03527cm = 1/72 Inch
		 * 1 Inch = 2.54cm = 96px（一般顯示器 96DPI 進行換算。像素不能出現小數點，一般是取整數）
		 * 1 Twip = 1/20 pt
		 * 1 Pixel = 1440 TPI / 96 DPI = 15 Twips
		 * 1 Twip = 96 DPI / 1440 TPI = 0.0666667 Pixels
		*/
        /// <summary>將以點(Point)為單位的字型大小值，配合系統字型放大率，轉換成螢幕解析度值(DPI)。</summary>
        /// <param name="size">以點(Point)為單位的字型大小值。</param>
        /// <returns>轉換成螢幕解析度的字型大小(DPI)。</returns>
        public static int FontPointSizeToDpi(float size) => (int)(1F / 72F * size * PrimaryScreen.DpiX);
        #endregion
    }
    #endregion
}
