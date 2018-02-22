namespace Zebble.Plugin.MBox
{
    public class PointAnnotation : Annotation
    {
        Position coordinate = new Position();
        public Position Coordinate
        {
            get => coordinate;
            set
            {
                if (coordinate == value) return;
                coordinate = value;
                AnnotationCoordinateChanged.Raise();
                AnnotationChanged.Raise();
            }
        }
        public AsyncEvent AnnotationCoordinateChanged = new AsyncEvent();
    }
}
