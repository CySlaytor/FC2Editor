using System;
using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolEnvironment : ITool
    {
        private ParamTime m_paramTime = new ParamTime(Localizer.Localize("PARAM_TIME"));
        private ParamFloat m_paramStormFactor = new ParamFloat(Localizer.Localize("PARAM_STORM_FACTOR"), 0f, 0f, 1f, 0.01f);
        private ParamFloat m_paramWaterLevel = new ParamFloat(Localizer.Localize("PARAM_WATER_LEVEL"), -1f, -1f, 255f, 0.1f);

        public ToolEnvironment()
        {
            m_paramTime.ValueChanged += time_ValueChanged;
            m_paramStormFactor.ValueChanged += stormFactor_ValueChanged;
            m_paramWaterLevel.ValueChanged += waterLevel_ValueChanged;
        }

        public string GetToolName() => Localizer.Localize("TOOL_ENVIRONMENT");
        public Image GetToolImage() => Resources.Environment;
        public string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_ENVIRONMENT");

        public IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramTime;
            yield return m_paramStormFactor;
            yield return m_paramWaterLevel;
        }

        public IParameter GetMainParameter() => null;

        private void time_ValueChanged(object sender, EventArgs e) => Engine.TimeOfDay = m_paramTime.Value;
        private void stormFactor_ValueChanged(object sender, EventArgs e) => Engine.StormFactor = m_paramStormFactor.Value;
        private void waterLevel_ValueChanged(object sender, EventArgs e) => TerrainManager.WaterLevel = m_paramWaterLevel.Value;

        public void Activate()
        {
            m_paramTime.Value = Engine.TimeOfDay;
            m_paramStormFactor.Value = Engine.StormFactor;
            m_paramWaterLevel.Value = TerrainManager.WaterLevel;
        }

        public void Deactivate() { }
    }
}