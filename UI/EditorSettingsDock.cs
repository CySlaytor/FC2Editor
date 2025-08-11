using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using TD.SandDock;

namespace FC2Editor.UI
{
    internal class EditorSettingsDock : UserDockableWindow, IParameterProvider
    {
        private IContainer components = null;
        private ParametersList parametersList;

        private ParamBool m_paramShowFog = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SHOW_FOG"), true);
        private ParamBool m_paramShowShadow = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SHOW_SHADOWS"), true);
        private ParamBool m_paramShowWater = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SHOW_WATER"), true);
        private ParamBool m_paramShowCollections = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SHOW_COLLECTIONS"), true);
        private ParamBool m_paramShowIcons = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SHOW_ICONS"), true);
        private ParamBool m_paramEnableSound = new ParamBool(Localizer.LocalizeCommon("SETTINGS_ENABLE_SOUND"), false);
        private ParamBool m_paramShowGrid = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SHOW_GRID"), false);
        private ParamEnumBase<float> m_paramGridResolution = new ParamEnumBase<float>(Localizer.LocalizeCommon("SETTINGS_SHOW_GRID_RESOLUTION"), 64f, ParamEnumUIType.Buttons);
        private ParamBool m_paramInvincible = new ParamBool(Localizer.LocalizeCommon("SETTINGS_INVINCIBILITY"), true);
        private ParamBool m_paramSnapObjects = new ParamBool(Localizer.LocalizeCommon("SETTINGS_SNAP_TO_TERRAIN"), true);
        private ParamBool m_paramAutoSnappingObjects = new ParamBool(Localizer.LocalizeCommon("SETTINGS_AUTO_SNAP_OBJECTS"), true);
        private ParamBool m_paramAutoSnappingObjectsRotation = new ParamBool(Localizer.LocalizeCommon("SETTINGS_AUTO_SNAP_OBJECTS_ROTATION"), true);
        private ParamBool m_paramAutoSnappingObjectsTerrain = new ParamBool(Localizer.LocalizeCommon("SETTINGS_AUTO_SNAP_OBJECTS_TERRAIN"), true);
        private ParamBool m_paramCameraClipTerrain = new ParamBool(Localizer.LocalizeCommon("SETTINGS_CAMERA_CLIP_TERRAIN"), true);
        private ParamBool m_paramInvertMouseView = new ParamBool(Localizer.Localize("SETTINGS_INVERT_MOUSE_VIEW"), false);
        private ParamBool m_paramInvertMousePan = new ParamBool(Localizer.Localize("SETTINGS_INVERT_MOUSE_PAN"), false);
        private ParamEnumBase<EditorSettings.QualityLevel> m_paramEngineQuality = new ParamEnumBase<EditorSettings.QualityLevel>(Localizer.Localize("SETTINGS_ENGINE_QUALITY"), EditorSettings.QualityLevel.Custom, ParamEnumUIType.ComboBox);
        private ParamEnumBase<float> m_paramViewportQuality = new ParamEnumBase<float>(Localizer.Localize("SETTINGS_VIEWPORT_QUALITY"), 1f, ParamEnumUIType.ComboBox);
        private ParamBool m_paramKillDistanceOverride = new ParamBool(Localizer.Localize("SETTINGS_KILL_DISTANCE_OVERRIDE"), false);

        public EditorSettingsDock()
        {
            InitializeComponent();
            m_paramShowFog.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_FOG");
            m_paramShowShadow.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_SHADOW");
            m_paramShowWater.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_WATER");
            m_paramShowCollections.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_COLLECTION");
            m_paramShowIcons.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_ICONS");
            m_paramEnableSound.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_SOUND");
            m_paramShowGrid.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_GRID");
            m_paramInvincible.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_INVINCIBILITY");
            m_paramSnapObjects.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_KEEPOBJTERRAIN");
            m_paramAutoSnappingObjects.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_AUTOSNAP_OBJ");
            m_paramAutoSnappingObjectsRotation.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_AUTOSNAP_OBJ_ROT");
            m_paramAutoSnappingObjectsTerrain.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_AUTOSNAP_OBJ_TER");
            m_paramCameraClipTerrain.ToolTip = Localizer.LocalizeCommon("HELP_SETTINGS_CAMERA_CLIP");
            m_paramInvertMouseView.ToolTip = Localizer.Localize("HELP_SETTINGS_INVERT_MOUSE_VIEW");
            m_paramInvertMousePan.ToolTip = Localizer.Localize("HELP_SETTINGS_INVERT_MOUSE_PAN");
            m_paramEngineQuality.ToolTip = Localizer.Localize("HELP_SETTINGS_ENGINE_QUALITY");
            m_paramViewportQuality.ToolTip = Localizer.Localize("HELP_SETTINGS_VIEWPORT_QUALITY");
            m_paramKillDistanceOverride.ToolTip = Localizer.Localize("HELP_SETTINGS_KILL_DISTANCE");

            m_paramShowFog.ValueChanged += paramShowFog_ValueChanged;
            m_paramShowShadow.ValueChanged += paramShowShadow_ValueChanged;
            m_paramShowWater.ValueChanged += paramShowWater_ValueChanged;
            m_paramShowCollections.ValueChanged += paramShowCollections_ValueChanged;
            m_paramShowIcons.ValueChanged += paramShowIcons_ValueChanged;
            m_paramEnableSound.ValueChanged += paramEnableSound_ValueChanged;
            m_paramShowGrid.ValueChanged += paramShowGrid_ValueChanged;
            m_paramGridResolution.ValueChanged += paramGridResolution_ValueChanged;
            m_paramInvincible.ValueChanged += paramInvincible_ValueChanged;
            m_paramSnapObjects.ValueChanged += paramSnapObjects_ValueChanged;
            m_paramAutoSnappingObjects.ValueChanged += paramAutoSnappingObjects_ValueChanged;
            m_paramAutoSnappingObjectsRotation.ValueChanged += paramAutoSnappingObjectsRotation_ValueChanged;
            m_paramAutoSnappingObjectsTerrain.ValueChanged += paramAutoSnappingObjectsTerrain_ValueChanged;
            m_paramCameraClipTerrain.ValueChanged += paramCameraClipTerrain_ValueChanged;
            m_paramInvertMouseView.ValueChanged += paramInvertMouseView_ValueChanged;
            m_paramInvertMousePan.ValueChanged += paramInvertMousePan_ValueChanged;
            m_paramEngineQuality.ValueChanged += paramEngineQuality_ValueChanged;
            m_paramViewportQuality.ValueChanged += paramViewportQuality_ValueChanged;
            m_paramKillDistanceOverride.ValueChanged += paramKillDistanceOverride_ValueChanged;

            m_paramGridResolution.Names = new string[] { "16", "32", "64", "128" };
            m_paramGridResolution.Values = new float[] { 16f, 32f, 64f, 128f };

            m_paramEngineQuality.Names = new string[] {
                Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_LOW"), Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_MEDIUM"),
                Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_HIGH"), Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_VERYHIGH"),
                Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_ULTRAHIGH"), Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_OPTIMAL"),
                Localizer.Localize("MainMenu", "OPTIONS_DISPLAY_CUSTOM")
            };
            m_paramEngineQuality.Values = new EditorSettings.QualityLevel[] {
                EditorSettings.QualityLevel.Low, EditorSettings.QualityLevel.Medium, EditorSettings.QualityLevel.High,
                EditorSettings.QualityLevel.VeryHigh, EditorSettings.QualityLevel.UltraHigh, EditorSettings.QualityLevel.Optimal,
                EditorSettings.QualityLevel.Custom
            };

            m_paramViewportQuality.Names = new string[] { "100%", "90%", "80%", "70%", "60%", "50%" };
            m_paramViewportQuality.Values = new float[] { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f };

            Text = Localizer.Localize(Text);
        }

        public IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramShowFog;
            yield return m_paramShowShadow;
            yield return m_paramShowWater;
            yield return m_paramShowCollections;
            yield return m_paramShowIcons;
            yield return m_paramEnableSound;
            yield return m_paramShowGrid;
            yield return m_paramGridResolution;
            yield return m_paramInvincible;
            yield return m_paramSnapObjects;
            yield return m_paramAutoSnappingObjects;
            yield return m_paramAutoSnappingObjectsRotation;
            yield return m_paramAutoSnappingObjectsTerrain;
            yield return m_paramCameraClipTerrain;
            yield return m_paramInvertMouseView;
            yield return m_paramInvertMousePan;
            yield return m_paramEngineQuality;
            yield return m_paramViewportQuality;
            yield return m_paramKillDistanceOverride;
        }

        public IParameter GetMainParameter() => null;

        private void paramShowFog_ValueChanged(object sender, EventArgs e) => EditorSettings.ShowFog = m_paramShowFog.Value;
        private void paramShowShadow_ValueChanged(object sender, EventArgs e) => EditorSettings.ShowShadow = m_paramShowShadow.Value;
        private void paramShowWater_ValueChanged(object sender, EventArgs e) => EditorSettings.ShowWater = m_paramShowWater.Value;
        private void paramShowCollections_ValueChanged(object sender, EventArgs e) => EditorSettings.ShowCollections = m_paramShowCollections.Value;
        private void paramShowIcons_ValueChanged(object sender, EventArgs e) => EditorSettings.ShowIcons = m_paramShowIcons.Value;
        private void paramEnableSound_ValueChanged(object sender, EventArgs e) => EditorSettings.SoundEnabled = m_paramEnableSound.Value;
        private void paramShowGrid_ValueChanged(object sender, EventArgs e)
        {
            EditorSettings.ShowGrid = m_paramShowGrid.Value;
            m_paramGridResolution.Enabled = m_paramShowGrid.Value;
        }
        private void paramGridResolution_ValueChanged(object sender, EventArgs e) => EditorSettings.GridResolution = (int)m_paramGridResolution.Value;
        private void paramInvincible_ValueChanged(object sender, EventArgs e) => EditorSettings.Invincible = m_paramInvincible.Value;
        private void paramSnapObjects_ValueChanged(object sender, EventArgs e) => EditorSettings.SnapObjectsToTerrain = m_paramSnapObjects.Value;
        private void paramAutoSnappingObjects_ValueChanged(object sender, EventArgs e)
        {
            EditorSettings.AutoSnappingObjects = m_paramAutoSnappingObjects.Value;
            m_paramAutoSnappingObjectsRotation.Enabled = m_paramAutoSnappingObjects.Value;
        }
        private void paramAutoSnappingObjectsRotation_ValueChanged(object sender, EventArgs e) => EditorSettings.AutoSnappingObjectsRotation = m_paramAutoSnappingObjectsRotation.Value;
        private void paramAutoSnappingObjectsTerrain_ValueChanged(object sender, EventArgs e) => EditorSettings.AutoSnappingObjectsTerrain = m_paramAutoSnappingObjectsTerrain.Value;
        private void paramCameraClipTerrain_ValueChanged(object sender, EventArgs e) => EditorSettings.CameraClipTerrain = m_paramCameraClipTerrain.Value;
        private void paramInvertMouseView_ValueChanged(object sender, EventArgs e) => EditorSettings.InvertMouseView = m_paramInvertMouseView.Value;
        private void paramInvertMousePan_ValueChanged(object sender, EventArgs e) => EditorSettings.InvertMousePan = m_paramInvertMousePan.Value;
        private void paramEngineQuality_ValueChanged(object sender, EventArgs e) => EditorSettings.EngineQuality = m_paramEngineQuality.Value;
        private void paramViewportQuality_ValueChanged(object sender, EventArgs e)
        {
            EditorSettings.ViewportQuality = m_paramViewportQuality.Value;
            Editor.Viewport.UpdateSize();
        }
        private void paramKillDistanceOverride_ValueChanged(object sender, EventArgs e) => EditorSettings.KillDistanceOverride = m_paramKillDistanceOverride.Value;

        public void RefreshSettings()
        {
            if (Engine.Initialized)
            {
                m_paramShowFog.Value = EditorSettings.ShowFog;
                m_paramShowShadow.Value = EditorSettings.ShowShadow;
                m_paramShowWater.Value = EditorSettings.ShowWater;
                m_paramShowCollections.Value = EditorSettings.ShowCollections;
                m_paramShowIcons.Value = EditorSettings.ShowIcons;
                m_paramEnableSound.Value = EditorSettings.SoundEnabled;
                m_paramShowGrid.Value = EditorSettings.ShowGrid;
                m_paramGridResolution.Value = EditorSettings.GridResolution;
                m_paramGridResolution.Enabled = m_paramShowGrid.Value;
                m_paramInvincible.Value = EditorSettings.Invincible;
                m_paramSnapObjects.Value = EditorSettings.SnapObjectsToTerrain;
                m_paramAutoSnappingObjects.Value = EditorSettings.AutoSnappingObjects;
                m_paramAutoSnappingObjectsRotation.Value = EditorSettings.AutoSnappingObjectsRotation;
                m_paramAutoSnappingObjectsTerrain.Value = EditorSettings.AutoSnappingObjectsTerrain;
                m_paramCameraClipTerrain.Value = EditorSettings.CameraClipTerrain;
                m_paramInvertMouseView.Value = EditorSettings.InvertMouseView;
                m_paramInvertMousePan.Value = EditorSettings.InvertMousePan;
                m_paramEngineQuality.Value = EditorSettings.EngineQuality;
                m_paramViewportQuality.Value = EditorSettings.ViewportQuality;
                m_paramKillDistanceOverride.Value = EditorSettings.KillDistanceOverride;
            }
        }

        private void EditorSettingsDock_Load(object sender, EventArgs e)
        {
            parametersList.Parameters = this;
            RefreshSettings();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.parametersList = new FC2Editor.UI.ParametersList();
            this.SuspendLayout();
            // 
            // parametersList
            // 
            this.parametersList.AutoScroll = true;
            this.parametersList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.parametersList.Location = new System.Drawing.Point(0, 0);
            this.parametersList.Name = "parametersList";
            this.parametersList.Parameters = null;
            this.parametersList.Size = new System.Drawing.Size(250, 400);
            this.parametersList.TabIndex = 1;
            // 
            // EditorSettingsDock
            // 
            this.Controls.Add(this.parametersList);
            this.Name = "EditorSettingsDock";
            this.Text = "DOCK_EDITOR_SETTINGS";
            this.Load += new System.EventHandler(this.EditorSettingsDock_Load);
            this.ResumeLayout(false);
        }
    }
}