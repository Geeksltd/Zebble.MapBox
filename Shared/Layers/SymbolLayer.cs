using System;

namespace Zebble.Plugin.MBox
{
    public class SymbolLayer : StyleLayer
    {
        public SymbolLayer(string id, string sourceId) : base(id, sourceId)
        {
        }

        public string IconImageName;

        double iconOpacity = 0.8;
        public double IconOpacity
        {
            get => iconOpacity;
            set
            {
                iconOpacity = Math.Min(1.0, Math.Max(value, 0.0));
            }
        }
    }
}
