using System.Collections.Generic;

namespace FC2Editor.Parameters
{
    internal interface IParameterProvider
    {
        IEnumerable<IParameter> GetParameters();
        IParameter GetMainParameter();
    }
}