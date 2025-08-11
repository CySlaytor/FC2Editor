using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class ObjectViewer
    {
        private static bool m_active;
        private static EditorObject m_object;

        public static bool Active
        {
            get { return m_active; }
            set
            {
                m_active = value;
                FCE_ObjectViewer_SetActive(m_active);
            }
        }

        public static EditorObject Object
        {
            get { return m_object; }
            set
            {
                m_object = value;
                FCE_ObjectViewer_SetObject(m_object.Pointer);
            }
        }

        [DllImport("Dunia.dll")] private static extern void FCE_ObjectViewer_SetActive(bool active);
        [DllImport("Dunia.dll")] private static extern void FCE_ObjectViewer_SetObject(IntPtr obj);
    }
}