using Mapbox.Maps;
using System.Collections.Specialized;
using System.Linq;
using MapView = Mapbox.Maps.MapView;

namespace Zebble.Plugin.MBox
{
    public partial class MapViewRenderer : MapView.IOnMapChangedListener
    {
        void AddMapEvents()
        {
            map.MarkerClick += MarkerClicked;
            map.MapClick += MapClicked;
            map.MyLocationChange += MyLocationChanged;
            fragment.OnMapChangedListener = (this);
        }

        void RemoveMapEvents()
        {
            map.MarkerClick -= MarkerClicked;
            map.MapClick -= MapClicked;
            map.MyLocationChange -= MyLocationChanged;
            fragment.OnMapChangedListener = null;
        }

        void MyLocationChanged(object o, MapboxMap.MyLocationChangeEventArgs args)
        {
            if (View.UserLocation == null)
                View.UserLocation = new Position();

            View.UserLocation.Lat = args.Location.Latitude;
            View.UserLocation.Long = args.Location.Longitude;
        }

        void MapClicked(object o, MapboxMap.MapClickEventArgs args)
        {
            View.IsTouchInMap = false;

            var point = map.Projection.ToScreenLocation(args.Position);
            var xfPoint = new Zebble.Point(point.X, point.Y);
            var xfPosition = new Position(args.Position.Latitude, args.Position.Longitude);

            View.DidTapOnMapCommand?.Invoke(xfPosition, xfPoint);
        }

        void MarkerClicked(object o, MapboxMap.MarkerClickEventArgs args)
        {
            View.Center.Lat = args.Marker.Position.Latitude;
            View.Center.Long = args.Marker.Position.Longitude;
            View.IsMarkerClicked = true;

            var annotationKey = annotationDictionaries.FirstOrDefault(x => x.Value == args.Marker).Key;

            if (View.CanShowCalloutChecker?.Invoke(annotationKey) == true)
            {
                args.Marker.ShowInfoWindow(map, fragment.View as MapView);
            }
        }

        public void OnMapChanged(int p0)
        {
            switch (p0)
            {
                case MapView.DidFinishLoadingStyle:
                    var mapStyle = View.MapStyle;
                    if (mapStyle == null
                        || (!string.IsNullOrEmpty(map.StyleUrl) && mapStyle.UrlString != map.StyleUrl))
                    {
                        mapStyle = new MapStyle(map.StyleUrl);
                    }

                    if (View.MapStyle.CustomSources != null)
                    {
                        var notifiyCollection = View.MapStyle.CustomSources as INotifyCollectionChanged;
                        if (notifiyCollection != null)
                        {
                            notifiyCollection.CollectionChanged += OnShapeSourcesCollectionChanged;
                        }

                        AddSources(View.MapStyle.CustomSources.ToList());
                    }

                    if (View.MapStyle.CustomLayers != null)
                    {
                        if (View.MapStyle.CustomLayers is INotifyCollectionChanged notifiyCollection)
                        {
                            notifiyCollection.CollectionChanged += OnLayersCollectionChanged;
                        }

                        AddLayers(View.MapStyle.CustomLayers.ToList());
                    }
                    // TODO
                    // mapStyle.OriginalLayers = map.Layers.Select((arg) =>
                    //                                                    new Layer(arg.Id)
                    //                                                   ).ToArray();
                    View.MapStyle = mapStyle;
                    View.DidFinishLoadingStyleCommand?.Invoke(mapStyle);
                    break;
                case MapView.DidFinishRenderingMap:
                    View.Center = new Position(map.CameraPosition.Target.Latitude, map.CameraPosition.Target.Longitude);
                    View.DidFinishRenderingCommand?.Invoke(false);
                    break;
                case MapView.DidFinishRenderingMapFullyRendered:
                    View.DidFinishRenderingCommand?.Invoke(true);
                    break;
                case MapView.RegionDidChange:
                    View.RegionDidChangeCommand?.Invoke(false);
                    break;
                case MapView.RegionDidChangeAnimated:
                    View.RegionDidChangeCommand?.Invoke(true);
                    break;
                default:
                    break;
            }
        }
    }
}