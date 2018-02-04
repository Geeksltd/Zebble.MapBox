using System;

namespace Zebble.Plugin.MBox
{
    public class FillLayer : StyleLayer
    {
        public FillLayer(string id, string sourceId) : base(id, sourceId)
        {
        }

        public Color FillColor = Colors.Gray;

        double fillOpactity = 0.8;
        public double FillOpacity
        {
            get => fillOpactity;
            set
            {
                fillOpactity = Math.Min(1.0, Math.Max(value, 0.0));
            }
        }
    }
}
