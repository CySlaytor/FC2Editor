using System;
using System.Drawing;
using System.Runtime.InteropServices;
using FC2Editor.Properties;

namespace FC2Editor.Core.Nomad
{
    internal class SplineInventory : Inventory
    {
        public new class Entry : Inventory.Entry
        {
            public static Entry Null = new Entry(IntPtr.Zero);

            public override Image Icon => (Count <= 0) ? Resources.icon_object : Resources.icon_folder;
            public override string IconName => (Count <= 0) ? "icon_object" : "icon_folder";
            public override string DisplayName => Marshal.PtrToStringUni(FCE_Inventory_Spline_GetDisplay(m_entryPtr));
            public override Inventory.Entry Parent => new Entry(FCE_Inventory_Spline_GetParent(m_entryPtr));
            public override int Count => FCE_Inventory_Spline_GetChildCount(m_entryPtr);
            public override Inventory.Entry[] Children
            {
                get
                {
                    int count = Count;
                    Inventory.Entry[] array = new Inventory.Entry[count];
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = new Entry(FCE_Inventory_Spline_GetChild(m_entryPtr, i));
                    }
                    return array;
                }
            }

            public Entry(IntPtr ptr) : base(ptr) { }
        }

        private static SplineInventory s_instance = new SplineInventory();
        public override Inventory.Entry Root => new Entry(FCE_Inventory_Spline_GetRoot());
        public static SplineInventory Instance => s_instance;

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Spline_GetRoot();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Spline_GetParent(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern int FCE_Inventory_Spline_GetChildCount(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Spline_GetChild(IntPtr entry, int index);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Spline_GetDisplay(IntPtr entry);
    }
}