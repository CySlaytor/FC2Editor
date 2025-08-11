using System;
using System.Drawing;
using System.Runtime.InteropServices;
using FC2Editor.Properties;

namespace FC2Editor.Core.Nomad
{
    internal class ObjectInventory : Inventory
    {
        public new class Entry : Inventory.Entry
        {
            public override Image Icon => (Count <= 0) ? Resources.icon_object : Resources.icon_folder;
            public override string IconName => (Count <= 0) ? "icon_object" : "icon_folder";
            public uint Id => FCE_Inventory_Object_GetId(m_entryPtr);
            public override string DisplayName => Marshal.PtrToStringUni(FCE_Inventory_Object_GetDisplay(m_entryPtr));
            public override Inventory.Entry Parent => new Entry(FCE_Inventory_Object_GetParent(m_entryPtr));
            public override int Count => FCE_Inventory_Object_GetChildCount(m_entryPtr);
            public bool IsDirectory => Count > 0;

            public override Inventory.Entry[] Children
            {
                get
                {
                    int count = Count;
                    Inventory.Entry[] array = new Inventory.Entry[count];
                    for (int i = 0; i < count; i++)
                    {
                        array[i] = new Entry(FCE_Inventory_Object_GetChild(m_entryPtr, i));
                    }
                    return array;
                }
            }

            public bool AutoOrientation => FCE_Inventory_Object_IsAutoOrientation(m_entryPtr);

            public float ZOffset
            {
                get { return FCE_Inventory_Object_GetZOffset(m_entryPtr); }
                set { FCE_Inventory_Object_SetZOffset(m_entryPtr, value); }
            }

            public bool AutoPivot
            {
                get { return FCE_Inventory_Object_IsAutoPivot(m_entryPtr); }
                set { FCE_Inventory_Object_SetAutoPivot(m_entryPtr, value); }
            }

            public int PivotCount => FCE_Inventory_Object_GetPivotCount(m_entryPtr);

            public Entry(IntPtr ptr) : base(ptr) { }

            public void ClearPivots() => FCE_Inventory_Object_ClearPivots(m_entryPtr);
            public void AddPivot(EditorObjectPivot pivot) => FCE_Inventory_Object_AddPivot(m_entryPtr, pivot.position.X, pivot.position.Y, pivot.position.Z, pivot.normal.X, pivot.normal.Y, pivot.normal.Z, pivot.normalUp.X, pivot.normalUp.Y, pivot.normalUp.Z);
            public void SetPivot(int idx, EditorObjectPivot pivot) => FCE_Inventory_Object_SetPivot(m_entryPtr, idx, pivot.position.X, pivot.position.Y, pivot.position.Z, pivot.normal.X, pivot.normal.Y, pivot.normal.Z, pivot.normalUp.X, pivot.normalUp.Y, pivot.normalUp.Z);
            public void SetPivots(float minX, float maxX, float minY, float maxY) => FCE_Inventory_Object_SetPivots(m_entryPtr, minX, maxX, minY, maxY);
        }

        private static ObjectInventory s_instance = new ObjectInventory();
        public override Inventory.Entry Root => new Entry(FCE_Inventory_Object_GetRoot());
        public static ObjectInventory Instance => s_instance;

        public void SavePivots() => FCE_Inventory_Object_SavePivots();

        #region P/Invoke
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Object_GetRoot();
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Object_GetParent(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern int FCE_Inventory_Object_GetChildCount(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Object_GetChild(IntPtr entry, int index);
        [DllImport("Dunia.dll")] private static extern uint FCE_Inventory_Object_GetId(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_Inventory_Object_GetDisplay(IntPtr entry);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Inventory_Object_IsAutoOrientation(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern float FCE_Inventory_Object_GetZOffset(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_SetZOffset(IntPtr entry, float offset);
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_SavePivots();
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_ClearPivots(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_AddPivot(IntPtr entry, float posX, float posY, float posZ, float normX, float normY, float normZ, float normUpX, float normUpY, float normUpZ);
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_SetPivot(IntPtr entry, int idx, float posX, float posY, float posZ, float normX, float normY, float normZ, float normUpX, float normUpY, float normUpZ);
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_SetPivots(IntPtr entry, float minX, float maxX, float minY, float maxY);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_Inventory_Object_IsAutoPivot(IntPtr entry);
        [DllImport("Dunia.dll")] private static extern void FCE_Inventory_Object_SetAutoPivot(IntPtr entry, [MarshalAs(UnmanagedType.U1)] bool autoPivot);
        [DllImport("Dunia.dll")] private static extern int FCE_Inventory_Object_GetPivotCount(IntPtr entry);
        #endregion
    }
}