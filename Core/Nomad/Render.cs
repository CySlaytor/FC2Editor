using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class Render
    {
        public static void BeginGroup() => FCE_Draw_BeginGroup();
        public static void EndGroup() => FCE_Draw_EndGroup();

        public static void DrawScreenCircleOutlined(Vec2 center, float z, float radius, float penWidth, Color color)
        {
            FCE_Draw_ScreenCircleOutlined(center.X, center.Y, z, radius, penWidth, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static void DrawScreenRectangleOutlined(RectangleF rect, float z, float penWidth, Color color)
        {
            SizeF size = rect.Size;
            Vec2 center = new Vec2(rect.X + size.Width / 2f, rect.Y + size.Height / 2f);
            FCE_Draw_ScreenRectangleOutlined(center.X, center.Y, z, size.Width, size.Height, penWidth, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static void DrawTerrainCircle(Vec2 center, float radius, float penWidth, Color color, float zOrder)
        {
            FCE_Draw_Terrain_Circle(center.X, center.Y, radius, penWidth, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f, zOrder);
        }

        public static void DrawTerrainSquare(Vec2 center, float radius, float penWidth, Color color, float zOrder)
        {
            FCE_Draw_Terrain_Square(center.X, center.Y, radius, penWidth, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f, zOrder);
        }

        public static void DrawArrow(Vec3 center, Vec3 direction, float length, float radius, float headLength, float headRadius, Color color)
        {
            FCE_Draw_Arrow(center.X, center.Y, center.Z, direction.X, direction.Y, direction.Z, length, radius, headLength, headRadius, color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static void DrawDot(Vec3 center, float radius, Color color, bool back, bool startGroup)
        {
            FCE_Draw_Dot(center.X, center.Y, center.Z, radius, color.R / 255f, color.G / 255f, color.B / 255f, back, startGroup);
        }

        public static void DrawSegmentedLineSegment(Vec3 p1, Vec3 p2, float penRadius, float penRadius2, Color color, bool back)
        {
            FCE_Draw_SegmentedLineSegment(p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, penRadius, penRadius2, color.R / 255f, color.G / 255f, color.B / 255f, back);
        }

        public static void DrawWireBoxFromBottomZ(Vec3 pos, Vec3 size, float penWidth)
        {
            FCE_Draw_WireBoxFromBottomZ(pos.X, pos.Y, pos.Z, size.X, size.Y, size.Z, penWidth);
        }

        public static void DrawWireRegionFromTerrain(Points points, float penWidth, Color color)
        {
            FCE_Draw_WireRegionFromTerrain(points.Pointer, penWidth, color.R / 255f, color.G / 255f, color.B / 255f);
        }

        [DllImport("Dunia.dll")] private static extern void FCE_Draw_BeginGroup();
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_EndGroup();
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_ScreenCircleOutlined(float x, float y, float z, float radius, float penWidth, float r, float g, float b, float a);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_ScreenRectangleOutlined(float x, float y, float z, float width, float height, float penWidth, float r, float g, float b, float a);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_Terrain_Circle(float x, float y, float radius, float penWidth, float r, float g, float b, float a, float zOrder);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_Terrain_Square(float x, float y, float radius, float penWidth, float r, float g, float b, float a, float zOrder);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_Arrow(float x, float y, float z, float dirX, float dirY, float dirZ, float length, float radius, float headLength, float headRadius, float r, float g, float b, float a);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_Dot(float x, float y, float z, float radius, float r, float g, float b, [MarshalAs(UnmanagedType.U1)] bool back, [MarshalAs(UnmanagedType.U1)] bool startGroup);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_SegmentedLineSegment(float x1, float y1, float z1, float x2, float y2, float z2, float penRadius, float penRadius2, float r, float g, float b, [MarshalAs(UnmanagedType.U1)] bool back);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_WireBoxFromBottomZ(float x, float y, float z, float sizeX, float sizeY, float sizeZ, float penWidth);
        [DllImport("Dunia.dll")] private static extern void FCE_Draw_WireRegionFromTerrain(IntPtr points, float penWidth, float r, float g, float b);
    }
}