using System.Drawing;

namespace FC2Editor.Tools
{
    internal interface IToolBase
    {
        string GetToolName();
        Image GetToolImage();
    }
}