using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class BudgetManager
    {
        public static int MemoryUsage => FCE_BudgetManager_GetMemoryUsage();
        public static float ObjectUsage => FCE_BudgetManager_GetObjectUsage();

        [DllImport("Dunia.dll")] private static extern int FCE_BudgetManager_GetMemoryUsage();
        [DllImport("Dunia.dll")] private static extern float FCE_BudgetManager_GetObjectUsage();
    }
}