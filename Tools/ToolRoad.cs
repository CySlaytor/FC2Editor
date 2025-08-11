using System;
using System.Collections.Generic;
using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Parameters;
using FC2Editor.Properties;
using FC2Editor.UI; // Needed for ParamRoad

namespace FC2Editor.Tools
{
    // Note: ParamRoad needs to be defined
    internal class ParamRoad : Parameter
    {
        protected int m_value = -1;

        public int Value
        {
            get { return m_value; }
            set
            {
                m_value = value;
                this.ValueChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler ValueChanged;

        public ParamRoad(string display) : base(display) { }

        protected override System.Windows.Forms.Control CreateUIControl()
        {
            ParamSplineList list = new ParamSplineList
            {
                Value = m_value
            };
            list.ValueChanged += delegate (object sender, EventArgs e)
            {
                Value = ((ParamSplineList)sender).Value;
            };
            return list;
        }

        protected override void UpdateUIControl(System.Windows.Forms.Control control)
        {
            ((ParamSplineList)control).UpdateUI();
        }
    }

    internal class ToolRoad : ToolSpline
    {
        private ParamRoad m_paramRoad = new ParamRoad(null);
        private ParamFloat m_paramRoadWidth = new ParamFloat(Localizer.Localize("PARAM_ROAD_WIDTH"), 8f, 4f, 16f, 0.1f);
        private SplineRoad m_splineRoad;

        public ToolRoad()
        {
            m_paramRoad.ValueChanged += spline_ValueChanged;
            m_paramRoadWidth.ValueChanged += roadWidth_ValueChanged;
        }

        public override string GetToolName() => Localizer.Localize("TOOL_ROADS");
        public override Image GetToolImage() => Resources.Spline;

        public override IEnumerable<IParameter> GetParameters()
        {
            yield return m_paramRoad;
            yield return m_paramEditTool;
            yield return m_paramRoadWidth;
        }

        public override string GetContextHelp() => Localizer.LocalizeCommon("HELP_TOOL_ROAD") + "\r\n\r\n" + GetSplineHelp();

        private void spline_ValueChanged(object sender, EventArgs e) => UpdateSelectedSpline();
        private void roadWidth_ValueChanged(object sender, EventArgs e)
        {
            if (m_splineRoad.IsValid)
            {
                m_splineRoad.Width = m_paramRoadWidth.Value;
                m_splineRoad.UpdateSpline();
            }
        }

        private void UpdateSelectedSpline()
        {
            SetSplineRoad((m_paramRoad.Value != -1) ? SplineManager.GetRoadFromId(m_paramRoad.Value) : SplineRoad.Null);
        }

        private void SetSplineRoad(SplineRoad splineRoad)
        {
            m_splineRoad = splineRoad;
            SetSpline(splineRoad);
            m_paramRoadWidth.Enabled = m_splineRoad.IsValid;
            if (m_splineRoad.IsValid)
            {
                m_paramRoadWidth.Value = m_splineRoad.Width;
            }
        }

        public override void Activate()
        {
            base.Activate();
            UpdateSelectedSpline();
        }

        public override void OnEditorEvent(uint eventType, IntPtr eventPtr)
        {
            base.OnEditorEvent(eventType, eventPtr);
            if (eventType == EditorEventUndo.TypeId)
            {
                m_paramRoad.UpdateUIControls();
            }
        }
    }
}