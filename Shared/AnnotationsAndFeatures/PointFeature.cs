using System.Collections.Generic;

namespace Zebble.Plugin.MBox
{
    public class PointFeature : PointAnnotation, IFeature
    {
        public PointFeature() { }
        public PointFeature(PointAnnotation annotation)
        {
            Id = annotation.Id;
            Title = annotation.Title;
            SubTitle = annotation.SubTitle;
            Coordinate = annotation.Coordinate;
        }

        public Dictionary<string, object> Attributes
        {
            get; set;
        } = new Dictionary<string, object>();
    }
}
