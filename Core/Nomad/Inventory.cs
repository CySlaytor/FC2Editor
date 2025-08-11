using System;
using System.Drawing;

namespace FC2Editor.Core.Nomad
{
    internal abstract class Inventory
    {
        public abstract class Entry
        {
            protected IntPtr m_entryPtr;

            public abstract Image Icon { get; }
            public abstract string IconName { get; }
            public abstract string DisplayName { get; }
            public abstract Entry Parent { get; }
            public abstract int Count { get; }
            public abstract Entry[] Children { get; }

            public bool IsValid => m_entryPtr != IntPtr.Zero;
            public IntPtr Pointer => m_entryPtr;

            public Entry(IntPtr ptr)
            {
                m_entryPtr = ptr;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is Entry entry))
                {
                    return base.Equals(obj);
                }
                return Pointer == entry.Pointer;
            }

            public override int GetHashCode()
            {
                return Pointer.ToInt32();
            }
        }

        public abstract Entry Root { get; }
    }
}