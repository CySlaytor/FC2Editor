using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct PaintBrush
    {
        private IntPtr m_pointer;

        public bool IsValid => m_pointer != IntPtr.Zero;
        public IntPtr Pointer => m_pointer;

        public PaintBrush(IntPtr ptr)
        {
            m_pointer = ptr;
        }

        public static PaintBrush Create(bool circle, float radius, float hardness, float opacity, float distortion)
        {
            return new PaintBrush(FCE_Brush_Create(circle, radius, hardness, opacity, distortion));
        }

        public void Destroy()
        {
            if (IsValid)
            {
                FCE_Brush_Destroy(m_pointer);
                m_pointer = IntPtr.Zero;
            }
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Brush_Create(bool circle, float radius, float hardness, float opacity, float distortion);
        [DllImport("Dunia.dll")] private static extern void FCE_Brush_Destroy(IntPtr brush);
    }
}