using System;
using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal static class SplineManager
    {
        public const int MaxRoads = 8;
        public static SplineRoad CreateRoad(int id) => new SplineRoad(FCE_SplineManager_CreateRoad(id));
        public static void DestroyRoad(int id) => FCE_SplineManager_DestroyRoad(id);
        public static SplineRoad GetRoadFromId(int id) => new SplineRoad(FCE_SplineManager_GetRoadFromId(id));
        public static SplineZone GetPlayableZone() => new SplineZone(FCE_SplineManager_GetPlayableZone());

        [DllImport("Dunia.dll")] private static extern IntPtr FCE_SplineManager_CreateRoad(int id);
        [DllImport("Dunia.dll")] private static extern void FCE_SplineManager_DestroyRoad(int id);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_SplineManager_GetRoadFromId(int id);
        [DllImport("Dunia.dll")] private static extern IntPtr FCE_SplineManager_GetPlayableZone();
    }
}