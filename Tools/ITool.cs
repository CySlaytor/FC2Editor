using FC2Editor.Parameters;

namespace FC2Editor.Tools
{
    internal interface ITool : IToolBase, IParameterProvider
    {
        void Activate();
        void Deactivate();
        string GetContextHelp();
    }
}