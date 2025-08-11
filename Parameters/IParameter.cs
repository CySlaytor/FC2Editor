using System.Windows.Forms;

namespace FC2Editor.Parameters
{
    internal interface IParameter
    {
        string DisplayName { get; }
        string ToolTip { get; }
        Control AcquireUIControl();
        void ReleaseUIControl(Control control);
    }
}