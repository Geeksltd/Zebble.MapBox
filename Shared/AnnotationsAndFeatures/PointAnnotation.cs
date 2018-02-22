namespace Zebble.Plugin.MBox
{
    public class PointAnnotation : Annotation
    {
        Position _coordinate = new Position();
        public Position Coordinate
        {
            get => _coordinate;
            set
            {
                if (_coordinate == value) return;
                _coordinate = value;
                AnnotationCoordinateChanged.Raise();
                AnnotationChanged.Raise();
            }
        }
        public AsyncEvent AnnotationCoordinateChanged = new AsyncEvent();
    }
}
