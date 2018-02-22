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

        Annotation shape = new Annotation();
        public Annotation Shape
        {
            get => shape;
            set
            {
                shape = value;
                ShapeChanged.Raise();
            }
        }

        public AsyncEvent ShapeChanged = new AsyncEvent();
    }
}