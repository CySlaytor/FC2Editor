using System.Windows.Forms;

namespace FC2Editor.Parameters
{
    internal class ParamText : Parameter
    {
        public ParamText(string display) : base(display) { }

        protected override Control CreateUIControl()
        {
            Label label = new Label
            {
                AutoSize = true,
                Padding = new Padding(0, 2, 0, 2),
                Text = base.DisplayName
            };
            return label;
        }

        protected override void UpdateUIControl(Control control)
        {
            control.Text = base.DisplayName;
        }

        protected override void OnDisplayChanged()
        {
            UpdateUIControls();
        }
    }
}