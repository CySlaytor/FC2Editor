using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal struct PhysEntityVector : IDisposable
    {
        public static PhysEntityVector Null = default(PhysEntityVector);
        private IntPtr m_pointer;

        public bool IsValid => m_pointer != IntPtr.Zero;
        public IntPtr Pointer => m_pointer;

        public PhysEntityVector(IntPtr ptr)
        {
            m_pointer = ptr;
        }

        public static PhysEntityVector Create()
        {
            return new PhysEntityVector(FCE_PhysEntityVector_Create());
        }

        public void Dispose()
        {
            if (IsValid)
            {
                FCE_PhysEntityVector_Destroy(m_pointer);
                m_pointer = IntPtr.Zero;
            }
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_PhysEntityVector_Create();
        [DllImport("Dunia.dll")] private static extern void FCE_PhysEntityVector_Destroy(IntPtr ptr);
    }
}