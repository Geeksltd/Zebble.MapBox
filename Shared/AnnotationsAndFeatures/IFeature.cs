using System.Collections.Generic;

namespace Zebble.Plugin.MBox
{
    public interface IFeature
    {
        Dictionary<string, object> Attributes
        {
            get;
            set;
        }
    }
}
