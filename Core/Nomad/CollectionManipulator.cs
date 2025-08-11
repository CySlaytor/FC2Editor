using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class CollectionManipulator
    {
        public static void Paint(Vec2 center, int id, PaintBrush brush)
        {
            FCE_Collection_Paint(center.X, center.Y, id, brush.Pointer);
        }

        public static void Paint_End()
        {
            FCE_Collection_Paint_End();
        }

        [DllImport("Dunia.dll")] private static extern void FCE_Collection_Paint(float x, float y, int id, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Collection_Paint_End();
    }
}