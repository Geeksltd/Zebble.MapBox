using System;
using System.Collections.Generic;

namespace Zebble.Plugin.MBox
{
    public class PositionChangeEventArgs : EventArgs
    {
        public Position NewPosition { get; }
        public PositionChangeEventArgs(Position newPosition)
        {
            NewPosition = newPosition;
        }
    }
    public class Map : View, IRenderedBy<MapViewRenderer>
    {
        public Map()
        {
            CenterChanged = new AsyncEvent();
            UserLocationChanged = new AsyncEvent();
            ZoomLevelChanged = new AsyncEvent();
            PitchChanged = new AsyncEvent();
            PitchEnabledChanged = new AsyncEvent();
            RotateEnabledChanged = new AsyncEvent();
            RotateDegreeChanged = new AsyncEvent();
            MapStyleChanged = new AsyncEvent();
            AnnotationsChanged = new AsyncEvent();

        }
        public bool IsMarkerClicked { get; set; }

        public bool IsTouchInMap { get; set; }

        private Position _center = new Position();
        public Position Center
        {
            get => _center;
            set
            {
                if (_center == value) return;
                _center = value;
                CenterChanged.Raise();
            }
        }

        public AsyncEvent CenterChanged;

        private Position _userLocation = new Position();
        public Position UserLocation
        {
            get => _userLocation;
            set
            {
                if (_userLocation == value) return;
                _userLocation = value;
                UserLocationChanged.Raise();
            }
        }

        public AsyncEvent UserLocationChanged;

        private double _zoomLevel = 10.0;
        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (_zoomLevel == value)
                    return;
                _zoomLevel = value;
                ZoomLevelChanged.Raise();
            }
        }

        public AsyncEvent ZoomLevelChanged;

        private double _pitch = 0.0;
        public double Pitch
        {
            get => _pitch;
            set
            {
                if (_pitch == value) return;
                _pitch = value;
                PitchChanged.Raise();
            }
        }

        public AsyncEvent PitchChanged;

        private bool _pitchEnabled;
        public bool PitchEnabled
        {
            get => _pitchEnabled;
            set
            {
                if (_pitchEnabled == value) return;
                _pitchEnabled = value;
                PitchEnabledChanged.Raise();
            }
        }

        public AsyncEvent PitchEnabledChanged;

        private bool _rotateEnabled;
        public bool RotateEnabled
        {
            get => _rotateEnabled;
            set
            {
                if (_rotateEnabled == value) return;
                _rotateEnabled = value;
                RotateEnabledChanged.Raise();
            }
        }

        public AsyncEvent RotateEnabledChanged;

        private double _rotateDegree = 0.0;
        public double RotatedDegree
        {
            get => _rotateDegree;
            set
            {
                if (_rotateDegree == value) return;
                _rotateDegree = value;
                RotateDegreeChanged.Raise();
            }
        }

        public AsyncEvent RotateDegreeChanged;

        private MapStyle _mapStyle = new MapStyle();
        public MapStyle MapStyle
        {
            get => _mapStyle;
            set
            {
                if (_mapStyle == value) return;
                _mapStyle = value;
                MapStyleChanged.Raise();
            }
        }

        public AsyncEvent MapStyleChanged;

        IEnumerable<Annotation> _annotations = new List<Annotation>();
        public IEnumerable<Annotation> Annotations
        {
            get => _annotations;
            set
            {
                _annotations = value;
                AnnotationsChanged.Raise();
            }
        }

        public AsyncEvent AnnotationsChanged;

        public Func<string, bool> CanShowCalloutChecker { get; set; } = DefaultCanShowCalloutChecker;

        public Func<byte[]> TakeSnapshotFunc { get; set; } = default(Func<byte[]>);

        public Func<Point, double, string[], IFeature[]> GetFeaturesAroundPointFunc { get; set; } =
            default(Func<Point, double, string[], IFeature[]>);

        public Action ResetPositionAction { get; set; } = default(Action);

        public Action ReloadStyleAction { get; set; } = default(Action);

        public Func<Annotation, string, bool> UpdateShapeOfSourceFunc { get; set; } =
            default(Func<Annotation, string, bool>);

        public Func<string, bool, bool, bool> UpdateLayerFunc { get; set; } = default(Func<string, bool, bool, bool>);

        public Action<Position, double?, double?, bool, Action> UpdateViewPortAction { get; set; } =
            default(Action<Position, double?, double?, bool, Action>);

        public Func<bool, bool> ToggleScaleBarFunc { get; set; } = default(Func<bool, bool>);

        public Func<string, byte[]> GetStyleImageFunc { get; set; } = default(Func<string, byte[]>);

        public Func<string, bool, StyleLayer> GetStyleLayerFunc { get; set; } = default(Func<string, bool, StyleLayer>);

        static readonly Func<string, bool> DefaultCanShowCalloutChecker = x => true;

        public Action<MapStyle> DidFinishLoadingStyleCommand { get; set; }

        public Action<bool> DidFinishRenderingCommand { get; set; }

        public Action<bool> RegionDidChangeCommand { set; get; }

        public Action<Position, Point> DidTapOnMapCommand { set; get; }
    }
}

