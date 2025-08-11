using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using FC2Editor.Core;

namespace FC2Editor.Core.Nomad
{
    internal struct Snapshot
    {
        private IntPtr m_pointer;

        public IntPtr Pointer => m_pointer;

        public Snapshot(IntPtr ptr)
        {
            m_pointer = ptr;
        }

        public static Snapshot Create(int width, int height)
        {
            return new Snapshot(FCE_Snapshot_Create(width, height));
        }

        public void Destroy()
        {
            FCE_Snapshot_Destroy(m_pointer);
            m_pointer = IntPtr.Zero;
        }

        public Image GetImage()
        {
            FCE_Snapshot_GetData(m_pointer, out IntPtr data, out int width, out int height, out int pitch);
            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            for (int i = 0; i < bitmap.Height; i++)
            {
                Win32.RtlMoveMemory(new IntPtr(bitmapData.Scan0.ToInt64() + i * bitmapData.Stride), new IntPtr(data.ToInt64() + i * pitch), bitmap.Width * 4);
            }

            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Snapshot_Create(int width, int height);
        [DllImport("Dunia.dll")] private static extern void FCE_Snapshot_Destroy(IntPtr snapshot);
        [DllImport("Dunia.dll")] private static extern void FCE_Snapshot_GetData(IntPtr snapshot, out IntPtr data, out int width, out int height, out int pitch);
    }
}