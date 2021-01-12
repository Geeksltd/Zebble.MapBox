namespace Zebble.Plugin
{
    using System;
    using Zebble;
    using System.Collections.Generic;
    using Olive;
    using Olive.GeoLocation;

    public partial class MapBox
    {
        GeoLocation center = new GeoLocation(51.5, 0.12);
        float zoom = 13;
        bool showsUserLocation = true;
        List<Annotation> annotations = new List<Annotation>();

        internal readonly AsyncEvent ApiZoomChanged = new AsyncEvent();
        internal readonly AsyncEvent ApiCenterChanged = new AsyncEvent();
        internal event Action<Annotation> AnnotationAdded, AnnotationRemoved;
        internal event Action<GeoLocation, GeoLocation> FitBoundsCalled;
        public event Action<Annotation> AnnotationClicked;

        public MapBox() => AccessToken = Config.Get("MapBox.Access.Token");

        public string AccessToken { get; set; }
        public string StyleUrl { get; set; }
        public string AnnotationImagePath;
        public Size AnnotationImageSize = new Size(16, 16);

        public IEnumerable<Annotation> Annotations => annotations;

        public GeoLocation Center
        {
            get => center;
            set
            {
                if (center == value || value == null) return;
                center = value;
                ApiCenterChanged.Raise();
            }
        }

        public bool ShowsUserLocation
        {
            get => showsUserLocation;
            set
            {
                if (showsUserLocation == value) return;
                showsUserLocation = value;
            }
        }

        public float Zoom
        {
            get => zoom;
            set
            {
                if (zoom.AlmostEquals(value)) return;
                zoom = value;
                ApiZoomChanged.Raise();
            }
        }

        public void Add(params Annotation[] annotations)
        {
            this.annotations.AddRange(annotations);
            foreach (var a in annotations) AnnotationAdded?.Invoke(a);
        }

        public void FitBounds(GeoLocation start, GeoLocation end)
        {
            FitBoundsCalled?.Invoke(start, end);
        }

        public void Remove(params Annotation[] annotations)
        {
            this.annotations.Remove(annotations);
            foreach (var a in annotations) AnnotationRemoved?.Invoke(a);
        }
    }
}