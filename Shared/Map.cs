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
    public partial class Map : View, IRenderedBy<MapViewRenderer>
    {
        public bool IsMarkerClicked { get; set; }

        public bool IsTouchInMap { get; set; }

        public Position Center { get; set; } = default(Position);

        public AsyncEvent CenterChanged;

        public Position UserLocation { get; set; } = default(Position);

        public AsyncEvent UserLocationChanged;

        public double ZoomLevel { get; set; } = 10.0;

        public AsyncEvent ZoomLevelChanged;

        public double Pitch { get; set; } = 0.0;

        public AsyncEvent PitchChanged;

        public bool PitchEnabled { get; set; }

        public AsyncEvent PitchEnabledChanged;

        public bool RotateEnabled { get; set; }

        public AsyncEvent RotateEnabledChanged;

        public double RotatedDegree { get; set; } = 0.0;

        public AsyncEvent RotateDegreeChanged;

        public MapStyle MapStyle { get; set; } = default(MapStyle);

        public AsyncEvent MapStyleChanged;

        public IEnumerable<Annotation> Annotations { get; set; } = default(IEnumerable<Annotation>);

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

