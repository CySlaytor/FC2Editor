using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using FC2Editor.Core;
using FC2Editor.Parameters;

namespace FC2Editor.UI
{
    internal class ParametersList : UserControl
    {
        private IContainer components = null;
        private Dictionary<IParameter, Control> m_controls = new Dictionary<IParameter, Control>();
        private IParameterProvider m_parameters;
        private EventHandler m_paramsChangedHandler;

        public IParameterProvider Parameters
        {
            get { return m_parameters; }
            set
            {
                if (m_parameters is IParameterProviderDynamic dynamicProvider)
                {
                    dynamicProvider.ParamsChanged -= m_paramsChangedHandler;
                }
                m_parameters = value;
                if (m_parameters is IParameterProviderDynamic dynamicProviderNew)
                {
                    dynamicProviderNew.ParamsChanged += m_paramsChangedHandler;
                }
                UpdateUI();
            }
        }

        public ParametersList()
        {
            InitializeComponent();
            m_paramsChangedHandler = _ParamsChanged;
        }

        private void ClearUI()
        {
            foreach (KeyValuePair<IParameter, Control> controlPair in m_controls)
            {
                // Assuming MainForm exists. This will be added later.
                // MainForm.Instance.ToolTip.SetToolTip(controlPair.Value, null);
                foreach (Control c in controlPair.Value.Controls)
                {
                    // MainForm.Instance.ToolTip.SetToolTip(c, null);
                }
                controlPair.Key.ReleaseUIControl(controlPair.Value);
            }
            m_controls.Clear();
            this.Controls.Clear();
        }

        private void UpdateUI()
        {
            Win32.SetRedraw(this, false);
            SuspendLayout();
            ClearUI();
            if (Parameters != null)
            {
                IParameter mainParameter = Parameters.GetMainParameter();
                bool isTop = true;
                foreach (IParameter parameter in Parameters.GetParameters())
                {
                    Control control = parameter.AcquireUIControl();
                    base.Controls.Add(control);

                    // Tooltip logic will be enabled when MainForm is implemented
                    // string toolTip = parameter.ToolTip;
                    // if (toolTip != null)
                    // {
                    //     MainForm.Instance.ToolTip.SetToolTip(control, parameter.ToolTip);
                    //     foreach (Control c in control.Controls)
                    //     {
                    //         MainForm.Instance.ToolTip.SetToolTip(c, parameter.ToolTip);
                    //     }
                    // }

                    if (parameter == mainParameter)
                    {
                        control.Dock = DockStyle.Fill;
                        base.Controls.SetChildIndex(control, 0);
                        isTop = false;
                    }
                    else
                    {
                        control.Dock = (isTop ? DockStyle.Top : DockStyle.Bottom);
                        base.Controls.SetChildIndex(control, (!isTop) ? (base.Controls.Count - 1) : 0);
                    }
                    m_controls.Add(parameter, control);
                }
            }
            ResumeLayout();
            Win32.SetRedraw(this, true);
            Refresh();
        }

        private void _ParamsChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        protected override void Dispose(bool disposing)
        {
            ClearUI();
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ParametersList
            // 
            this.AutoScroll = true;
            this.Name = "ParametersList";
            this.ResumeLayout(false);
        }
    }
}