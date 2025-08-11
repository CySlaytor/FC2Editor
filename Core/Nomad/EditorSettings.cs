using System.Runtime.InteropServices;

namespace FC2Editor.Core.Nomad
{
    internal class EditorSettings
    {
        public enum QualityLevel
        {
            Low,
            Medium,
            High,
            VeryHigh,
            UltraHigh,
            Optimal,
            Custom
        }

        private static float m_viewportQuality = 1f;
        private static bool m_invertMouseView;
        private static bool m_invertMousePan;

        public static bool ShowCollections
        {
            get { return FCE_EditorSettings_IsCollectionVisible(); }
            set { FCE_EditorSettings_ShowCollections(value); }
        }

        public static bool ShowFog
        {
            get { return FCE_EditorSettings_IsFogVisible(); }
            set { FCE_EditorSettings_ShowFog(value); }
        }

        public static bool ShowShadow
        {
            get { return FCE_EditorSettings_IsShadowVisible(); }
            set { FCE_EditorSettings_ShowShadow(value); }
        }

        public static bool ShowWater
        {
            get { return FCE_EditorSettings_IsWaterVisible(); }
            set { FCE_EditorSettings_ShowWater(value); }
        }

        public static bool ShowIcons
        {
            get { return FCE_EditorSettings_IsIconsVisible(); }
            set { FCE_EditorSettings_ShowIcons(value); }
        }

        public static bool SoundEnabled
        {
            get { return FCE_EditorSettings_IsSoundEnabled(); }
            set { FCE_EditorSettings_SetSoundEnabled(value); }
        }

        public static bool ShowGrid
        {
            get { return FCE_EditorSettings_IsGridVisible(); }
            set { FCE_EditorSettings_ShowGrid(value); }
        }

        public static int GridResolution
        {
            get { return FCE_EditorSettings_GetGridResolution(); }
            set { FCE_EditorSettings_SetGridResolution(value); }
        }

        public static bool Invincible
        {
            get { return FCE_EditorSettings_IsInvincible(); }
            set { FCE_EditorSettings_SetInvincible(value); }
        }

        public static bool SnapObjectsToTerrain
        {
            get { return FCE_EditorSettings_IsSnappingObjectsToTerrain(); }
            set { FCE_EditorSettings_SetSnapObjectsToTerrain(value); }
        }

        public static bool AutoSnappingObjects
        {
            get { return FCE_EditorSettings_IsAutoSnappingObjects(); }
            set { FCE_EditorSettings_SetAutoSnappingObjects(value); }
        }

        public static bool AutoSnappingObjectsRotation
        {
            get { return FCE_EditorSettings_IsAutoSnappingObjectsRotation(); }
            set { FCE_EditorSettings_SetAutoSnappingObjectsRotation(value); }
        }

        public static bool AutoSnappingObjectsTerrain
        {
            get { return FCE_EditorSettings_IsAutoSnappingObjectsTerrain(); }
            set { FCE_EditorSettings_SetAutoSnappingObjectsTerrain(value); }
        }

        public static bool CameraClipTerrain
        {
            get { return FCE_EditorSettings_IsCameraClippedTerrain(); }
            set { FCE_EditorSettings_SetCameraClipTerrain(value); }
        }

        public static QualityLevel EngineQuality
        {
            get { return FCE_EditorSettings_GetEngineQuality(); }
            set { FCE_EditorSettings_SetEngineQuality(value); }
        }

        public static float ViewportQuality
        {
            get { return m_viewportQuality; }
            set { m_viewportQuality = value; }
        }

        public static bool KillDistanceOverride
        {
            get { return FCE_EditorSettings_IsKillDistanceOverride(); }
            set { FCE_EditorSettings_SetKillDistanceOverride(value); }
        }

        public static bool InvertMouseView
        {
            get { return m_invertMouseView; }
            set { m_invertMouseView = value; }
        }

        public static bool InvertMousePan
        {
            get { return m_invertMousePan; }
            set { m_invertMousePan = value; }
        }

        #region P/Invoke
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsCollectionVisible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_ShowCollections([MarshalAs(UnmanagedType.U1)] bool show);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsFogVisible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_ShowFog([MarshalAs(UnmanagedType.U1)] bool show);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsShadowVisible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_ShowShadow([MarshalAs(UnmanagedType.U1)] bool show);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsWaterVisible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_ShowWater([MarshalAs(UnmanagedType.U1)] bool show);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsIconsVisible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_ShowIcons([MarshalAs(UnmanagedType.U1)] bool show);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsSoundEnabled();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetSoundEnabled([MarshalAs(UnmanagedType.U1)] bool enabled);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsGridVisible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_ShowGrid([MarshalAs(UnmanagedType.U1)] bool show);
        [DllImport("Dunia.dll")] private static extern int FCE_EditorSettings_GetGridResolution();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetGridResolution(int resolution);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsInvincible();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetInvincible([MarshalAs(UnmanagedType.U1)] bool invincible);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsSnappingObjectsToTerrain();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetSnapObjectsToTerrain([MarshalAs(UnmanagedType.U1)] bool snap);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsAutoSnappingObjects();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetAutoSnappingObjects([MarshalAs(UnmanagedType.U1)] bool snap);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsAutoSnappingObjectsRotation();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetAutoSnappingObjectsRotation([MarshalAs(UnmanagedType.U1)] bool snap);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsAutoSnappingObjectsTerrain();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetAutoSnappingObjectsTerrain([MarshalAs(UnmanagedType.U1)] bool snap);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsCameraClippedTerrain();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetCameraClipTerrain([MarshalAs(UnmanagedType.U1)] bool clip);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U4)] private static extern QualityLevel FCE_EditorSettings_GetEngineQuality();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetEngineQuality([MarshalAs(UnmanagedType.U4)] QualityLevel engineQuality);
        [DllImport("Dunia.dll")][return: MarshalAs(UnmanagedType.U1)] private static extern bool FCE_EditorSettings_IsKillDistanceOverride();
        [DllImport("Dunia.dll")] private static extern void FCE_EditorSettings_SetKillDistanceOverride([MarshalAs(UnmanagedType.U1)] bool value);
        #endregion
    }
}