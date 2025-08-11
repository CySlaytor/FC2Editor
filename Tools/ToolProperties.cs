using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;
using FC2Editor.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FC2Editor.Tools
{
    // Note: ToolParamSnapshot needs to be defined
    internal class ToolParamSnapshot : Parameter
    {
        public ToolParamSnapshot() : base(null) { }
        protected override Control CreateUIControl() => new ParamSnapshot();
    }

    internal class ToolProperties : ITool
    {
        private ParamString m_paramMapName = new ParamString(Localizer.Localize("PARAM_MAP_NAME"), null);
        private ParamText m_paramCreatorName = new ParamText("");
        private ParamString m_paramAuthorName = new ParamString(Localizer.Localize("PARAM_MAP_AUTHOR"), null);
        private ParamEnum<EditorDocument.BattlefieldSizes> m_paramBattlefield = new ParamEnum<EditorDocument.BattlefieldSizes>(Localizer.Localize("PARAM_MAP_SIZE"), EditorDocument.BattlefieldSizes.Medium, ParamEnumUIType.ComboBox);
        private ParamEnum<EditorDocument.PlayerSizes> m_paramPlayers = new ParamEnum<EditorDocument.PlayerSizes>(Localizer.Localize("PARAM_MAP_PLAYERS"), EditorDocument.PlayerSizes.Medium, ParamEnumUIType.ComboBox);
        private ToolParamSnapshot m_paramSnapshot = new ToolParamSnapshot();

        public ToolProperties()
        {
            m_paramBattlefield.Names = new string[] { Localizer.LocalizeCommon("PROPERTIES_BATTLEZONE_SMALL"), Localizer.LocalizeCommon("PROPERTIES_BATTLEZONE_MEDIUM"), Localizer.LocalizeCommon("PROPERTIES_BATTLEZONE_LARGE") };
            m_paramPlayers.Names = new string[] { "2-4", "4-8", "8-12", "12-16" };
            m_paramMapName.ValueChanged += mapName_ValueChanged;
            m_paramAuthorName.ValueChanged += authorName_ValueChanged;
            m_paramBattlefield.ValueChanged += battlefield_ValueChanged;
            m_paramPlayers.ValueChanged += players_ValueChanged;
        }

        public string GetToolName() => Localizer.Localize("TOOL_PROPERTIES");
        public Image GetToolImage() => Resources.Properties;
        public string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOPIC_PROPERTIES") + "\r\n\r\n" + Localizer.LocalizeCommon("HELP_TOOL_SNAPSHOT");

        public IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramMapName;
            yield return m_paramCreatorName;
            yield return m_paramAuthorName;
            yield return m_paramBattlefield;
            yield return m_paramPlayers;
            yield return m_paramSnapshot;
        }

        public IParameter GetMainParameter() => null;

        private void mapName_ValueChanged(object sender, EventArgs e) => EditorDocument.MapName = m_paramMapName.Value;
        private void authorName_ValueChanged(object sender, EventArgs e) => EditorDocument.AuthorName = m_paramAuthorName.Value;
        private void battlefield_ValueChanged(object sender, EventArgs e) => EditorDocument.BattlefieldSize = m_paramBattlefield.Value;
        private void players_ValueChanged(object sender, EventArgs e) => EditorDocument.PlayerSize = m_paramPlayers.Value;

        public void Activate()
        {
            m_paramMapName.Value = EditorDocument.MapName;
            string creatorName = EditorDocument.CreatorName;
            m_paramCreatorName.DisplayName = Localizer.Localize("PARAM_MAP_CREATOR") + ": " + (string.IsNullOrEmpty(creatorName) ? Localizer.Localize("PARAM_UNDEFINED") : creatorName);
            m_paramAuthorName.Value = EditorDocument.AuthorName;
            m_paramBattlefield.Value = EditorDocument.BattlefieldSize;
            m_paramPlayers.Value = EditorDocument.PlayerSize;
        }

        public void Deactivate() { }
    }
}