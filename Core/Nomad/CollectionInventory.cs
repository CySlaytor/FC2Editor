using System;
using System.Drawing;
using System.Runtime.InteropServices;
using FC2Editor.Properties;

namespace FC2Editor.Core.Nomad
{
    internal class CollectionInventory : Inventory
    {
        public new class Entry : Inventory.Entry
        {
            public static Entry Null = new Entry(IntPtr.Zero);

            public override Image Icon => (Count <= 0) ? Resources.icon_object : Resources.icon_folder;
            public override string IconName => (Count <= 0) ? "icon_object" : "icon_folder";
            public override string DisplayName => Marshal.PtrToStringUni(FCE_Inventory_Collection_GetDisplay(m_entryPtr));
            public override Inventory.Entry Parent => new Entry(FCE_Inventory_Collection_GetParent(m_entryPtr));
            public override int Count => FCE_Inventory_Collection_GetChildCount(m_entryPtr);

            public override Inventory.Entry[] Children
            {
                get
                {
                    int count = Count;
                    Inventory.Entry[] array = new Inventory.Entry[count];
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = new Entry(FCE_Inventory_Collection_GetChild(m_entryPtr, i));
                    }
                    return array;
                }
            }

            public bool HasBurnProfile => FCE_Inventory_Collection_GetBurnProfile(m_entryPtr) != 0x8FFF0255;

            public Entry(IntPtr ptr) : base(ptr) { }
        }

        private static CollectionInventory s_instance = new CollectionInventory();
        public override Inventory.Entry Root => new Entry(FCE_Inventory_Collection_GetRoot());
        public static CollectionInventory Instance => s_instance;

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Collection_GetRoot();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Collection_GetParent(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern int FCE_Inventory_Collection_GetChildCount(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Collection_GetChild(IntPtr entry, int index);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Collection_GetDisplay(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern uint FCE_Inventory_Collection_GetBurnProfile(IntPtr entry);
    }
}