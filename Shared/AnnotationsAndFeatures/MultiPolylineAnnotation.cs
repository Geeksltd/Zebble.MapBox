namespace Zebble.Plugin.MBox
{
    public class MultiPolylineAnnotation : Annotation
    {
        Position[][] _coordinates = new Position[1][];
        public Position[][] Coordinates
        {
            get => _coordinates;
            set
            {
                _coordinates = value;
                AnnotationCoordinatesChanged.Raise();
                AnnotationChanged.Raise();
            }
        }

        public AsyncEvent AnnotationCoordinatesChanged = new AsyncEvent();
    }
}