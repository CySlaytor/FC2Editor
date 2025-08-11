using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class TextureManipulator
    {
        public static void Paint(Vec2 center, float amount, int id, PaintBrush brush) => FCE_Texture_Paint(center.X, center.Y, amount, id, brush.Pointer);
        public static void Paint_End() => FCE_Texture_Paint_End();
        public static void PaintConstraints_Begin(float minHeight, float maxHeight, float heightFuzziness, float minSlope, float maxSlope) => FCE_Texture_PaintConstraints_Begin(minHeight, maxHeight, heightFuzziness, minSlope, maxSlope);
        public static void PaintConstraints(Vec2 center, float amount, int id, PaintBrush brush) => FCE_Texture_PaintConstraints(center.X, center.Y, amount, id, brush.Pointer);
        public static void PaintConstraints_End() => FCE_Texture_PaintConstraints_End();

        [DllImport("Dunia.dll")] private static extern void FCE_Texture_Paint(float x, float y, float amount, int id, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Texture_Paint_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Texture_PaintConstraints_Begin(float minHeight, float maxHeight, float heightFuzziness, float minSlope, float maxSlope);
        [DllImport("Dunia.dll")] private static extern void FCE_Texture_PaintConstraints(float x, float y, float amount, int id, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Texture_PaintConstraints_End();
    }
}