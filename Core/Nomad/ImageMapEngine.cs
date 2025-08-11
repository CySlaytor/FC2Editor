using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FC2Editor.Core;

namespace FC2Editor.Core.Nomad
{
    internal struct ImageMapEngine
    {
        private IntPtr m_pointer;

        public Size Size
        {
            get { FCE_ImageMap_GetSize(m_pointer, out int width, out int height); return new Size(width, height); }
        }

        public ImageMapEngine(IntPtr ptr)
        {
            m_pointer = ptr;
        }

        public void Dispose()
        {
            FCE_ImageMap_Destroy(m_pointer);
            m_pointer = IntPtr.Zero;
        }

        public ImageMapEngine Clone()
        {
            return new ImageMapEngine(FCE_ImageMap_Clone(m_pointer));
        }

        public ImageMap GetImage()
        {
            Size size = Size;
            Bitmap bitmap = new Bitmap(size.Width, size.Height);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            FCE_ImageMap_ConvertTo24bit(m_pointer, bitmapData.Scan0, bitmapData.Stride, out float min, out float max);
            bitmap.UnlockBits(bitmapData);
            return new ImageMap(bitmap, min, max);
        }

        [DllImport("Dunia.dll")] private static extern void FCE_ImageMap_Destroy(IntPtr map);
        [DllImport("Dunia.dll")] private static extern void FCE_ImageMap_GetSize(IntPtr map, out int width, out int height);
        [DllImport("Dunia.dll")] private static extern void FCE_ImageMap_ConvertTo24bit(IntPtr map, IntPtr data, int stride, out float min, out float max);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_ImageMap_Clone(IntPtr map);
    }
}