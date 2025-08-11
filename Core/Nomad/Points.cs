using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct Points
    {
        public static Points Null = new Points(IntPtr.Zero);
        private IntPtr m_pointsPtr;

        public IntPtr Pointer => m_pointsPtr;

        public Points(IntPtr pointsPtr)
        {
            m_pointsPtr = pointsPtr;
        }

        public static Points Create()
        {
            return new Points(FCE_Core_Points_Create());
        }

        public void Destroy()
        {
            FCE_Core_Points_Destroy(m_pointsPtr);
            m_pointsPtr = IntPtr.Zero;
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Core_Points_Create();
        [DllImport("Dunia.dll")] private static extern void FCE_Core_Points_Destroy(IntPtr points);
    }
}