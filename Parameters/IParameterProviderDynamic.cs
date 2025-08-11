using System;

namespace FC2Editor.Parameters
{
    internal interface IParameterProviderDynamic
    {
        event EventHandler ParamsChanged;
    }
}