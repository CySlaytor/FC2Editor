using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal static class CollectionManager
    {
        public static int EmptyCollectionId = 8;

        public static CollectionInventory.Entry GetCollectionEntryFromId(int id)
        {
            return new CollectionInventory.Entry(FCE_CollectionManager_GetCollectionEntryFromId(id));
        }

        public static void AssignCollectionId(int id, CollectionInventory.Entry entry)
        {
            FCE_CollectionManager_AssignCollectionId(id, entry.Pointer);
        }

        public static void WriteMaskCircle(float x, float y, float radius, int id, bool update)
        {
            FCE_CollectionManager_WriteMaskCircle(x, y, radius, id, update);
        }

        public static void WriteMaskSquare(float x, float y, float radius, int id, bool update)
        {
            FCE_CollectionManager_WriteMaskSquare(x, y, radius, id, update);
        }

        public static void ClearMaskId(int id)
        {
            FCE_CollectionManager_ClearMaskId(id);
        }

        public static void UpdateCollections(Rectangle rect)
        {
            FCE_CollectionManager_UpdateCollections(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_CollectionManager_GetCollectionEntryFromId(int id);
        [DllImport("Dunia.dll")] private static extern void FCE_CollectionManager_AssignCollectionId(int id, IntPtr entry);
        [DllImport("Dunia.dll")] private static extern void FCE_CollectionManager_WriteMaskCircle(float cx, float cy, float radius, int id, [MarshalAs(UnmanagedType.U1)] bool update);
        [DllImport("Dunia.dll")] private static extern void FCE_CollectionManager_WriteMaskSquare(float cx, float cy, float radius, int id, [MarshalAs(UnmanagedType.U1)] bool update);
        [DllImport("Dunia.dll")] private static extern void FCE_CollectionManager_ClearMaskId(int id);
        [DllImport("Dunia.dll")] private static extern void FCE_CollectionManager_UpdateCollections(int x, int y, int w, int h);
    }
}