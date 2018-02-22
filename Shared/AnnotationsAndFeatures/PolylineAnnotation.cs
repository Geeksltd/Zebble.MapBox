namespace Zebble.Plugin.MBox
{
    public class PolylineAnnotation : Annotation
    {
        public PolylineAnnotation()
        {
        }
        Position[] coordinates = new Position[1];
        public Position[] Coordinates
        {
            get => coordinates;
            set
            {
                coordinates = value;
                AnnotationCoordinatesChanged.Raise();
                AnnotationChanged.Raise();
            }
        }

        public AsyncEvent AnnotationCoordinatesChanged = new AsyncEvent();
    }
}