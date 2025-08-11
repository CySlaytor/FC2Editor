using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class TerrainManipulator
    {
        public enum NoiseType
        {
            Normal,
            Absolute,
            InverseAbsolute
        }

        public static void Bump(Vec2 center, float amount, PaintBrush brush) => FCE_Terrain_Bump(center.X, center.Y, amount, brush.Pointer);
        public static void Bump_End() => FCE_Terrain_Bump_End();
        public static void RaiseLower(Vec2 center, float amount, PaintBrush brush) => FCE_Terrain_RaiseLower(center.X, center.Y, amount, brush.Pointer);
        public static void RaiseLower_End() => FCE_Terrain_RaiseLower_End();
        public static void SetHeight(Vec2 center, float height, PaintBrush brush) => FCE_Terrain_SetHeight(center.X, center.Y, height, brush.Pointer);
        public static void SetHeight_End() => FCE_Terrain_SetHeight_End();
        public static void Grab_Begin(float x, float y, PaintBrush brush) => FCE_Terrain_Grab_Begin(x, y, brush.Pointer);
        public static void Grab(float ratio) => FCE_Terrain_Grab(ratio);
        public static void Grab_End() => FCE_Terrain_Grab_End();
        public static void Smooth(Vec2 center, PaintBrush brush) => FCE_Terrain_Smooth(center.X, center.Y, brush.Pointer);
        public static void Smooth_End() => FCE_Terrain_Smooth_End();
        public static void Ramp(Vec2 ptStart, Vec2 ptEnd, float radius, float hardness) => FCE_Terrain_Ramp(ptStart.X, ptStart.Y, ptEnd.X, ptEnd.Y, radius, hardness);
        public static void Terrace(Vec2 center, float height, float falloff, PaintBrush brush) => FCE_Terrain_Terrace(center.X, center.Y, height, falloff, brush.Pointer);
        public static void Terrace_End() => FCE_Terrain_Terrace_End();
        public static void Noise_Begin(int numOctaves, float noiseSize, float persistence, NoiseType noiseType) => FCE_Terrain_Noise_Begin(numOctaves, noiseSize, persistence, noiseType);
        public static void Noise(Vec2 center, float amount, PaintBrush brush) => FCE_Terrain_Noise(center.X, center.Y, amount, brush.Pointer);
        public static void Noise_End() => FCE_Terrain_Noise_End();
        public static void Erosion(Vec2 center, float radius, float density, float deformation, float channelDepth, float randomness) => FCE_Terrain_Erosion(center.X, center.Y, radius, density, deformation, channelDepth, randomness);
        public static void Erosion_End() => FCE_Terrain_Erosion_End();

        #region P/Invoke
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Bump(float x, float y, float amount, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Bump_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_RaiseLower(float x, float y, float amount, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_RaiseLower_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_SetHeight(float x, float y, float height, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_SetHeight_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Grab_Begin(float x, float y, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Grab(float ratio);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Grab_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Smooth(float x, float y, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Smooth_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Ramp(float x1, float y1, float x2, float y2, float radius, float hardness);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Terrace(float x, float y, float height, float falloff, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Terrace_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Noise_Begin(int numOctaves, float noiseSize, float persistence, NoiseType noiseType);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Noise(float x, float y, float amount, IntPtr brush);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Noise_End();
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Erosion(float x, float y, float radius, float density, float deformation, float channelDepth, float randomness);
        [DllImport("Dunia.dll")] private static extern void FCE_Terrain_Erosion_End();
        #endregion
    }
}