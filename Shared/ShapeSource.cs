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

        Annotation _shape = new Annotation();
        public Annotation Shape
        {
            get => _shape;
            set
            {
                _shape = value;
                ShapeChanged.Raise();
            }
        }

        public AsyncEvent ShapeChanged = new AsyncEvent();

    }
}