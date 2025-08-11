using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;
using FC2Editor.UI;

namespace FC2Editor.Tools
{
    // Note: ParamValidationReport needs to be defined
    internal class ParamValidationReport : Parameter
    {
        private ValidationReport m_value;
        public ValidationReport Value
        {
            get { return m_value; }
            set { m_value = value; UpdateUIControls(); }
        }

        public event System.EventHandler ValueChanged;

        public ParamValidationReport(string display) : base(display) { }

        protected void OnValueChanged()
        {
            this.ValueChanged?.Invoke(this, new System.EventArgs());
        }

        protected override System.Windows.Forms.Control CreateUIControl()
        {
            ParamReport report = new ParamReport
            {
                ParameterName = base.DisplayName
            };
            UpdateUIControl(report);
            return report;
        }

        protected override void UpdateUIControl(System.Windows.Forms.Control control)
        {
            ((ParamReport)control).UpdateUI(m_value);
        }
    }

    internal class ToolValidation : ITool
    {
        private ParamEnum<GameModes> m_paramGameModes = new ParamEnum<GameModes>(Localizer.Localize("PARAM_GAMEMODE"), GameModes.Deathmatch, ParamEnumUIType.ComboBox);
        private ParamValidationReport m_paramGameModeReport = new ParamValidationReport(Localizer.Localize("PARAM_GAMEMODE_CRITERIAS"));
        private ParamValidationReport m_paramGameReport = new ParamValidationReport(Localizer.Localize("PARAM_MAP_REPORT"));
        private ValidationReport m_gameModeReport;
        private ValidationReport m_gameReport;

        public ToolValidation()
        {
            m_paramGameModes.Names = new string[] { Localizer.LocalizeCommon("VALIDATION_MODE_DM"), Localizer.LocalizeCommon("VALIDATION_MODE_TDM"), Localizer.LocalizeCommon("VALIDATION_MODE_CTF"), Localizer.LocalizeCommon("VALIDATION_MODE_VIP") };
            m_paramGameModes.ValueChanged += gameModes_ValueChanged;
        }

        public string GetToolName() => Localizer.Localize("TOOL_VALIDATION");
        public Image GetToolImage() => Resources.Validation;
        public string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOPIC_VALIDATION");

        public IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramGameModes;
            yield return m_paramGameModeReport;
            yield return m_paramGameReport;
        }

        public IParameter GetMainParameter() => m_paramGameReport;
        private void gameModes_ValueChanged(object sender, System.EventArgs e) => UpdateReports();

        public void Activate() => UpdateReports();

        public void Deactivate()
        {
            m_gameModeReport.Destroy();
            m_gameReport.Destroy();
            RefreshReports();
        }

        private void ClearReports()
        {
            m_gameModeReport.Destroy();
            m_gameReport.Destroy();
        }

        private void RefreshReports()
        {
            m_paramGameModeReport.Value = m_gameModeReport;
            m_paramGameReport.Value = m_gameReport;
        }

        private void UpdateReports()
        {
            ClearReports();
            m_gameModeReport = Validation.ValidateGameMode(m_paramGameModes.Value);
            m_gameReport = Validation.ValidateGame();
            RefreshReports();
        }
    }
}