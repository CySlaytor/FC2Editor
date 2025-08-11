using System.Drawing;
using FC2Editor.Core.Nomad;
using FC2Editor.Properties;

namespace FC2Editor.Tools
{
    internal class ToolFinalize : IToolAction
    {
        public string GetToolName() => Localizer.Localize("TOOL_MAKE_BEAUTIFUL");
        public Image GetToolImage() => Resources.Finalize;
        public void Fire() => EditorDocument.FinalizeMap();
    }
}