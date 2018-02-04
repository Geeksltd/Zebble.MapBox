using Android.Support.V7.App;
using Java.Util;
using Mapbox.Annotations;
using Mapbox.Camera;
using Mapbox.Geometry;
using Mapbox.Maps;
using Mapbox.Style.Sources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bitmap = Android.Graphics.Bitmap;
using Sdk = Mapbox;
using View = Android.Views.View;

namespace Zebble.Plugin.MBox
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class MapViewRenderer : INativeRenderer, MapboxMap.ISnapshotReadyCallback, IOnMapReadyCallback
    {
        Map View;
        MapboxMap map;
        MapViewFragment fragment;
        const int SIZE_ZOOM = 13;
        Position currentCamera;
        static int NextId;

        Dictionary<string, Sdk.Annotations.Annotation> annotationDictionaries =
            new Dictionary<string, Sdk.Annotations.Annotation>();

        public async Task<Android.Views.View> Render(Renderer renderer)
        {
            View = (Map)renderer.View;

            SetupFunctions();
            await View.WhenShown(() => { Thread.UI.Run(LoadMap); });
            throw new NotImplementedException();
        }
        int FindFreeId()
        {
            NextId++;
            while (UIRuntime.CurrentActivity.FindViewById(NextId) != null) NextId++;
            return NextId;
        }

        protected async Task LoadMap()
        {
            var activity = (AppCompatActivity)Renderer.Context;
            var view = new Android.Widget.FrameLayout(activity)
            {
                Id = FindFreeId()
            };
            View.Native = view;

            fragment = new MapViewFragment();

            activity.SupportFragmentManager.BeginTransaction()
                .Replace(view.Id, fragment)
                .Commit();

            fragment.GetMapAsync(this);
            currentCamera = new Position();
            await Task.CompletedTask;
        }

        public void SetupFunctions()
        {
            View.TakeSnapshotFunc += TakeMapSnapshot;
            View.GetFeaturesAroundPointFunc += GetFeaturesAroundPoint;

            View.UpdateLayerFunc = (layerId, isVisible, IsCustom) =>
            {
                if (string.IsNullOrEmpty(layerId)) return false;

                var layerIdStr = IsCustom ? layerId.Prefix() : layerId;
                var layer = map.GetLayer(layerIdStr);

                if (layer == null) return false;

                // TODO
                // layer.SetProperties(layer.Visibility,
                //    isVisible ? Sdk.Style.Layers.PropertyFactory.Visibility(Sdk.Style.Layers.Property.Visible)
                //        : Sdk.Style.Layers.PropertyFactory.Visibility(Sdk.Style.Layers.Property.None));

                if (!IsCustom || View.MapStyle.CustomLayers == null) return true;

                var count = View.MapStyle.CustomLayers.Count();
                for (var i = 0; i < count; i++)
                {
                    if (View.MapStyle.CustomLayers.ElementAt(i).Id == layerId)
                    {
                        View.MapStyle.CustomLayers.ElementAt(i).IsVisible = isVisible;
                        break;
                    }
                }

                return true;
            };

            View.UpdateShapeOfSourceFunc = (Annotation annotation, string sourceId) =>
            {
                if (annotation == null || string.IsNullOrEmpty(sourceId)) return false;

                var shape = annotation.ToFeatureCollection();

                if (!(map.GetSource(sourceId.Prefix()) is GeoJsonSource source)) return false;

                Thread.UI.Run(() =>
                {
                    source.SetGeoJson(shape);
                });

                if (View.MapStyle.CustomSources == null) return true;
                var count = View.MapStyle.CustomSources.Count();
                for (var i = 0; i < count; i++)
                {
                    if (View.MapStyle.CustomSources.ElementAt(i).Id == sourceId)
                    {
                        View.MapStyle.CustomSources.ElementAt(i).Shape = annotation;
                        break;
                    }
                }

                return true;
            };

            View.ReloadStyleAction = () =>
            {
                map.StyleUrl = map.StyleUrl;
            };

            View.UpdateViewPortAction = (centerLocation, zoomLevel, bearing, animated, completionHandler) =>
            {
                var newPosition = new CameraPosition.Builder()
                                                    .Bearing(bearing ?? map.CameraPosition.Bearing)
                                                    .Target(centerLocation?.ToLatLng() ?? map.CameraPosition.Target)
                                                    .Zoom(zoomLevel ?? map.CameraPosition.Zoom)
                                                    .Build();
                var callback = completionHandler == null ? null : new CancelableCallback()
                {
                    FinishHandler = completionHandler,
                    CancelHandler = completionHandler
                };
                var update = CameraUpdateFactory.NewCameraPosition(newPosition);
                if (animated)
                {
                    map.AnimateCamera(update, callback);
                }
                else
                {
                    map.MoveCamera(update, callback);
                }
            };
            View.CenterChanged.Handle(() =>
            {
                if (!ReferenceEquals(View.Center, currentCamera))
                {
                    if (View.Center == null) return;
                    FocustoLocation(View.Center.ToLatLng());
                }
            });
            View.MapStyleChanged.Handle(() =>
            {
                if (map != null) UpdateMapStyle();
            });
            View.PitchEnabledChanged.Handle(() =>
            {
                if (map != null)
                {
                    map.UiSettings.TiltGesturesEnabled = View.PitchEnabled;
                }
            });
            View.RotateEnabledChanged.Handle(() =>
            {
                if (map != null)
                {
                    map.UiSettings.RotateGesturesEnabled = View.RotateEnabled;
                }
            });
            View.AnnotationsChanged.Handle(() =>
            {
                RemoveAllAnnotations();
                if (View.Annotations != null)
                {
                    AddAnnotations(View.Annotations.ToArray());
                    if (View.Annotations is INotifyCollectionChanged notifyCollection)
                    {
                        notifyCollection.CollectionChanged += OnAnnotationsCollectionChanged;
                    }
                }
            });
            View.ZoomLevelChanged.Handle(() =>
            {
                var dif = Math.Abs(map.CameraPosition.Zoom - View.ZoomLevel);
                System.Diagnostics.Debug.WriteLine($"Current zoom: {map.CameraPosition.Zoom} - New zoom: {View.ZoomLevel}");
                if (dif >= 0.01)
                {
                    System.Diagnostics.Debug.WriteLine("Updating zoom level");
                    map.AnimateCamera(CameraUpdateFactory.ZoomTo((float)View.ZoomLevel));
                }
            });
            View.MapStyleChanged.Handle(() => UpdateMapStyle());
        }

        byte[] TakeMapSnapshot()
        {
            // TODO
            map.Snapshot(this);
            return result;
        }

        IFeature[] GetFeaturesAroundPoint(Point point, double radius, string[] layers)
        {
            var rect = point.ToRect(Renderer.Context.ToPixels((float)radius));
            var listFeatures = map.QueryRenderedFeatures(rect, layers);
            return listFeatures.Select(x => x.ToFeature())
                               .Where(x => x != null)
                               .ToArray();
        }

        public void Dispose()
        {
            if (fragment != null)
            {
                RemoveMapEvents();

                if (fragment.StateSaved)
                {
                    var activity = (AppCompatActivity)Renderer.Context;
                    var fm = activity.SupportFragmentManager;

                    fm.BeginTransaction()
                        .Remove(fragment)
                        .Commit();
                }

                fragment.Dispose();
                fragment = null;
            }
        }

        Dictionary<string, object> ConvertToDictionary(string featureProperties)
        {
            Dictionary<string, object> objectFeature = JsonConvert.DeserializeObject<Dictionary<string, object>>(featureProperties);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(objectFeature["properties"].ToString()); ;
        }

        void FocustoLocation(LatLng latLng)
        {
            if (map == null) { return; }

            var position = new CameraPosition.Builder().Target(latLng).Zoom(SIZE_ZOOM).Build();
            var camera = CameraUpdateFactory.NewCameraPosition(position);
            map.AnimateCamera(camera);
        }

        void UpdateMapStyle()
        {
            if (View.MapStyle != null && !string.IsNullOrEmpty(View.MapStyle.UrlString))
            {
                map.StyleUrl = View.MapStyle.UrlString;
                // View.MapStyle.PropertyChanging += OnMapStylePropertyChanging;
                // View.MapStyle.PropertyChanged += OnMapStylePropertyChanged;

                View.MapStyle.CustomLayersChanging.Handle((() =>
                {
                    if (View.MapStyle.CustomSources is INotifyCollectionChanged notifiyCollection)
                    {
                        notifiyCollection.CollectionChanged -= OnShapeSourcesCollectionChanged;
                    }

                    RemoveSources(View.MapStyle.CustomSources.ToList());
                }));

                View.MapStyle.CustomSourcesChanged.Handle((() =>
                {
                    var style = View.MapStyle;
                    if (style == null) return;
                    if (style.CustomSources is INotifyCollectionChanged notifiyCollection)
                    {
                        notifiyCollection.CollectionChanged += OnShapeSourcesCollectionChanged;
                    }

                    AddSources(style.CustomSources.ToList());
                }));
                View.MapStyle.CustomLayersChanged.Handle((() =>
                {
                    if (View.MapStyle.CustomLayers is INotifyCollectionChanged notifiyCollection)
                    {
                        notifiyCollection.CollectionChanged += OnLayersCollectionChanged;
                    }

                    AddLayers(View.MapStyle.CustomLayers.ToList());
                }));
            }
        }

        void OnShapeSourcesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddSources(e.NewItems.Cast<ShapeSource>().ToList());
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveSources(e.OldItems.Cast<ShapeSource>().ToList());
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // TODO
                // var sources = map.Sources;
                // foreach (var s in sources)
                // {
                //    if (s.Id.HasPrefix())
                //    {
                //        map.RemoveSource(s);
                //    }
                // }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                RemoveSources(e.OldItems.Cast<ShapeSource>().ToList());
                AddSources(e.NewItems.Cast<ShapeSource>().ToList());
            }
        }

        void AddSources(List<ShapeSource> sources)
        {
            if (sources == null || map == null)
            {
                return;
            }

            foreach (ShapeSource ss in sources)
            {
                if (ss.Id != null && ss.Shape != null)
                {
                    var shape = ss.Shape.ToFeatureCollection();

                    if (!(map.GetSource(ss.Id.Prefix()) is GeoJsonSource source))
                    {
                        source = new Sdk.Style.Sources.GeoJsonSource(ss.Id.Prefix(), shape);
                        map.AddSource(source);
                    }
                    else
                    {
                        source.SetGeoJson(shape);
                    }
                }
            }
        }

        void RemoveSources(List<ShapeSource> sources)
        {
            if (sources == null)
            {
                return;
            }

            foreach (ShapeSource source in sources)
            {
                if (source.Id != null)
                {
                    map.RemoveSource(source.Id.Prefix());
                }
            }
        }

        void OnLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                AddLayers(e.NewItems.Cast<Layer>().ToList());
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                RemoveLayers(e.OldItems);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // TODO
                // var layers = map.Layers;
                // foreach (var layer in layers)
                // {
                //    if (layer.Id.HasPrefix())
                //    {
                //        map.RemoveLayer(layer);
                //    }
                // }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                RemoveLayers(e.OldItems);

                AddLayers(e.NewItems.Cast<Layer>().ToList());
            }
        }

        void RemoveLayers(System.Collections.IList layers)
        {
            if (layers == null)
            {
                return;
            }

            foreach (Layer layer in layers)
            {
                var native = map.GetLayer(layer.Id.Prefix());

                if (native != null)
                {
                    map.RemoveLayer(native);
                }
            }
        }

        void AddLayers(List<Layer> layers)
        {
            if (layers == null)
            {
                return;
            }

            foreach (Layer layer in layers)
            {
                if (string.IsNullOrEmpty(layer.Id))
                {
                    continue;
                }

                map.RemoveLayer(layer.Id.Prefix());

                if (layer is CircleLayer)
                {
                    var cross = (CircleLayer)layer;

                    var source = map.GetSource(cross.SourceId.Prefix());
                    if (source == null)
                    {
                        continue;
                    }

                    map.AddLayer(cross.ToNative());
                }
                else if (layer is LineLayer)
                {
                    var cross = (LineLayer)layer;

                    var source = map.GetSource(cross.SourceId.Prefix());
                    if (source == null)
                    {
                        continue;
                    }

                    map.AddLayer(cross.ToNative());
                }
            }
        }

        void OnAnnotationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (Annotation annot in e.NewItems)
                    AddAnnotation(annot);
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var items = e.OldItems.Cast<Annotation>().ToList();

                RemoveAnnotations(items.ToArray());
                foreach (var item in items)
                    annotationDictionaries.Remove(item.Id);
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                map.RemoveAnnotations();
                annotationDictionaries.Clear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                RemoveAnnotations(e.OldItems.Cast<Annotation>().ToArray());

                AddAnnotations(e.NewItems.Cast<Annotation>().ToArray());
            }
        }

        void RemoveAnnotations(Annotation[] annotations)
        {
            var currentAnnotations = map.Annotations;
            if (currentAnnotations == null)
            {
                return;
            }

            var annots = new List<Sdk.Annotations.Annotation>();
            foreach (var at in annotations)
            {
                if (annotationDictionaries.ContainsKey(at.Id))
                {
                    annots.Add(annotationDictionaries[at.Id]);
                }
            }

            map.RemoveAnnotations(annots.ToArray());
        }

        void AddAnnotations(Annotation[] annotations)
        {
            foreach (Annotation at in annotations)
                AddAnnotation(at);
        }

        Sdk.Annotations.Annotation AddAnnotation(Annotation at)
        {
            Sdk.Annotations.Annotation options = null;
            switch (at)
            {
                case PointAnnotation _:
                    var marker = new MarkerOptions();
                    marker.SetTitle(at.Title);
                    marker.SetSnippet(at.Title);
                    marker.SetPosition(((PointAnnotation)at).Coordinate.ToLatLng());
                    options = map.AddMarker(marker);
                    break;
                case PolylineAnnotation _:
                    {
                        var polyline = at as PolylineAnnotation;
                        if (polyline.Coordinates?.Count() == 0)
                        {
                            return null;
                        }

                        var notifyCollection = polyline.Coordinates.ToList() as INotifyCollectionChanged;
                        if (notifyCollection != null)
                        {
                            notifyCollection.CollectionChanged += (s, e) =>
                            {
                                if (e.Action == NotifyCollectionChangedAction.Add)
                                {
                                    if (annotationDictionaries.ContainsKey(at.Id))
                                    {
                                        var poly = annotationDictionaries[at.Id] as Polyline;
                                        poly.AddPoint(polyline.Coordinates.ElementAt(e.NewStartingIndex).ToLatLng());
                                    }
                                    else
                                    {
                                        var coords = new ArrayList();
                                        for (var i = 0; i < polyline.Coordinates.Count(); i++)
                                            coords.Add(polyline.Coordinates.ElementAt(i).ToLatLng());

                                        var polylineOpt = new PolylineOptions();
                                        polylineOpt.Polyline.Width = Renderer.Context.ToPixels(1);
                                        polylineOpt.Polyline.Color = Android.Graphics.Color.Blue;
                                        polylineOpt.AddAll(coords);
                                        options = map.AddPolyline(polylineOpt);
                                        annotationDictionaries.Add(at.Id, options);
                                    }
                                }
                                else if (e.Action == NotifyCollectionChangedAction.Remove)
                                {
                                    if (annotationDictionaries.ContainsKey(at.Id))
                                    {
                                        var poly = annotationDictionaries[at.Id] as Polyline;
                                        poly.Points.Remove(polyline.Coordinates.ElementAt(e.OldStartingIndex).ToLatLng());
                                    }
                                }
                            };
                        }

                        break;
                    }
                case MultiPolylineAnnotation _:
                    {
                        var polyline = at as MultiPolylineAnnotation;
                        if (polyline.Coordinates == null || polyline.Coordinates.Length == 0)
                        {
                            return null;
                        }

                        var lines = new List<PolylineOptions>();
                        for (var i = 0; i < polyline.Coordinates.Length; i++)
                        {
                            if (polyline.Coordinates[i].Length == 0)
                            {
                                continue;
                            }

                            var coords = new PolylineOptions();
                            for (var j = 0; j < polyline.Coordinates[i].Length; j++)
                            {
                                coords.Add(new LatLng(polyline.Coordinates[i][j].Lat, polyline.Coordinates[i][j].Long));
                            }

                            lines.Add(coords);
                        }

                        map.AddPolylines(lines);
                        break;
                    }
            }

            if (options != null)
            {
                if (at.Id != null)
                {
                    annotationDictionaries.Add(at.Id, options);
                }
            }

            return options;
        }

        void RemoveAllAnnotations()
        {
            if (map.Annotations != null)
            {
                map.RemoveAnnotations(map.Annotations);
            }
        }

        byte[] result;
        void MapboxMap.ISnapshotReadyCallback.OnSnapshotReady(Bitmap bmp)
        {
            var stream = new MemoryStream();
            bmp.Compress(Bitmap.CompressFormat.Png, 0, stream);
            result = stream.ToArray();
        }

        public void OnMapReady(MapboxMap p0)
        {
            map = p0;

            map.MyLocationEnabled = true;
            map.UiSettings.RotateGesturesEnabled = View.RotateEnabled;
            map.UiSettings.TiltGesturesEnabled = View.PitchEnabled;

            if (View.Center != null)
            {
                map.CameraPosition = new CameraPosition.Builder()
                    .Target(View.Center.ToLatLng())
               .Zoom(View.ZoomLevel)
               .Build();
            }
            else
            {
                map.CameraPosition = new CameraPosition.Builder()
                    .Target(map.CameraPosition.Target)
               .Zoom(View.ZoomLevel)
               .Build();
            }

            AddMapEvents();

            SetupFunctions();
            UpdateMapStyle();
        }

        public IntPtr Handle { get; }
    }

    public class MapboxMapReadyEventArgs : EventArgs
    {
        public Sdk.Maps.MapboxMap Map { get; }
        public Sdk.Maps.MapView MapView { get; }
        public MapboxMapReadyEventArgs(Sdk.Maps.MapboxMap map, Sdk.Maps.MapView mapview)
        {
            MapView = mapview;
            Map = map;
        }
    }
}