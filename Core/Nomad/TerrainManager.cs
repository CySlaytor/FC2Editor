using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal static class TerrainManager
    {
        public static float WaterLevel
        {
            get { return FCE_TerrainManager_GetWaterLevel(); }
            set { FCE_TerrainManager_SetWaterLevel(value); }
        }

        public static float GetHeightAt(Vec2 point) => FCE_TerrainManager_GetHeightAt(point.X, point.Y);
        public static TextureInventory.Entry GetTextureEntryFromId(int id) => new TextureInventory.Entry(FCE_TerrainManager_GetTextureEntryFromId(id));
        public static void AssignTextureId(int id, TextureInventory.Entry entry) => FCE_TerrainManager_AssignTextureId(id, entry.Pointer);
        public static void ClearTextureId(int id) => FCE_TerrainManager_ClearTextureId(id);

        [DllImport("Dunia.dll")] private static extern float FCE_TerrainManager_GetHeightAt(float x, float y);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_TerrainManager_GetTextureEntryFromId(int id);
        [DllImport("Dunia.dll")] private static extern void FCE_TerrainManager_AssignTextureId(int id, IntPtr entry);
        [DllImport("Dunia.dll")] private static extern void FCE_TerrainManager_ClearTextureId(int id);
        [DllImport("Dunia.dll")] private static extern float FCE_TerrainManager_GetWaterLevel();
        [DllImport("Dunia.dll")] private static extern void FCE_TerrainManager_SetWaterLevel(float waterLevel);
    }
}