using System;

namespace FC2Editor.Core.Nomad
{
    internal class EditorEvent
    {
        protected uint m_typeID;
        protected IntPtr m_eventPtr;

        protected EditorEvent(uint typeID, IntPtr eventPtr)
        {
            m_typeID = typeID;
            m_eventPtr = eventPtr;
        }
    }
}