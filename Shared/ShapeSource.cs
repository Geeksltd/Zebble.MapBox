namespace Zebble.Plugin.MBox
{
    public class ShapeSource
    {
        public ShapeSource()
        {
        }

        public ShapeSource(string id, Annotation shape)
        {
            Id = id;
            Shape = shape;
        }

        public string Id
        {
            get;
            set;
        }

        public Annotation Shape { get; set; } = default(Annotation);
    }
}