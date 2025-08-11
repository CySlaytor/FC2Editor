using System;
using System.Drawing;
using System.Runtime.InteropServices;
using FC2Editor.Properties;

namespace FC2Editor.Core.Nomad
{
    internal class TextureInventory : Inventory
    {
        public new class Entry : Inventory.Entry
        {
            public static Entry Null = new Entry(IntPtr.Zero);

            public override Image Icon => (Count <= 0) ? Resources.icon_object : Resources.icon_folder;
            public override string IconName => (Count <= 0) ? "icon_object" : "icon_folder";
            public override string DisplayName => Marshal.PtrToStringUni(FCE_Inventory_Texture_GetDisplay(m_entryPtr));
            public override Inventory.Entry Parent => new Entry(FCE_Inventory_Texture_GetParent(m_entryPtr));
            public override int Count => FCE_Inventory_Texture_GetChildCount(m_entryPtr);
            public override Inventory.Entry[] Children
            {
                get
                {
                    int count = Count;
                    Inventory.Entry[] array = new Inventory.Entry[count];
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = new Entry(FCE_Inventory_Texture_GetChild(m_entryPtr, i));
                    }
                    return array;
                }
            }

            public Entry(IntPtr ptr) : base(ptr) { }
        }

        private static TextureInventory s_instance = new TextureInventory();
        public override Inventory.Entry Root => new Entry(FCE_Inventory_Texture_GetRoot());
        public static TextureInventory Instance => s_instance;

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Texture_GetRoot();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Texture_GetParent(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern int FCE_Inventory_Texture_GetChildCount(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Texture_GetChild(IntPtr entry, int index);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Texture_GetDisplay(IntPtr entry);
    }
}