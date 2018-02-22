using CoreGraphics;
using CoreLocation;
using Foundation;
using Mapbox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace Zebble.Plugin.MBox
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MapViewRenderer : INativeRenderer, IMGLMapViewDelegate, IUIGestureRecognizerDelegate
    {
        MGLMapView MapView { get; set; }
        Map View;
        public async Task<UIView> Render(Renderer renderer)
        {
            View = (Map)renderer.View;
            if (View == null)
                throw new Exception("E1");
            if (MapView == null)
            {
                SetupUserInterface();
                HandleEvents();
                SetupFunctions();
            }

            return MapView;
        }

        void HandleEvents()
        {
            if (MapView == null || View == null)
            {
                return;
            }

            View.CenterChanged.Handle(() => UpdateCenter());
            View.ZoomLevelChanged.Handle(() =>
            {
                if (!Math.Round(View.ZoomLevel * 100).Equals(Math.Round(MapView.ZoomLevel * 100)))
                    MapView.ZoomLevel = View.ZoomLevel;
            });
            View.PitchEnabledChanged.Handle(() =>
            {
                if (MapView.PitchEnabled != View.PitchEnabled) MapView.PitchEnabled = View.PitchEnabled;
            });
            View.RotateEnabledChanged.Handle(() =>
            {
                if (MapView.RotateEnabled != View.RotateEnabled)
                {
                    MapView.RotateEnabled = View.RotateEnabled;
                }
            });
            View.AnnotationsChanged.Handle(() =>
            {
                if (View.Annotations != null)
                {
                    AddAnnotations(View.Annotations.ToArray());
                    if (View.Annotations is INotifyCollectionChanged notifyCollection)
                    {
                        notifyCollection.CollectionChanged += OnAnnotationsCollectionChanged;
                    }
                }
            });
            View.MapStyleChanged.Handle(() =>
            {
                if (View.MapStyle != null
                    && !string.IsNullOrEmpty(View.MapStyle.UrlString)
                    && (MapView.StyleURL == null
                        || MapView.StyleURL.AbsoluteString != View.MapStyle.UrlString))
                {
                    UpdateMapStyle();
                }
            });
            View.PitchChanged.Handle(() =>
            {
                if (!View.Pitch.Equals(MapView.Camera.Pitch))
                {
                    var currentCamera = MapView.Camera;
                    var newCamera = MGLMapCamera.CameraLookingAtCenterCoordinate(currentCamera.CenterCoordinate,
                        currentCamera.Altitude,
                        (nfloat)View.Pitch,
                        currentCamera.Heading);
                    MapView.SetCamera(newCamera, true);
                }
            });
            View.RotateDegreeChanged.Handle(() =>
            {
                if (!View.RotatedDegree.Equals(MapView.Camera.Heading))
                {
                    var currentCamera = MapView.Camera;
                    var newCamera = MGLMapCamera.CameraLookingAtCenterCoordinate(currentCamera.CenterCoordinate,
                        currentCamera.Altitude,
                        currentCamera.Pitch,
                        (nfloat)View.RotatedDegree);
                    MapView.SetCamera(newCamera, true);
                }
            });
            var tapGest = new UITapGestureRecognizer
            {
                NumberOfTapsRequired = 1,
                CancelsTouchesInView = false,
                Delegate = this
            };
            MapView.AddGestureRecognizer(tapGest);
            tapGest.AddTarget(obj =>
            {
                if (!(obj is UITapGestureRecognizer gesture) || gesture.State != UIGestureRecognizerState.Ended) return;
                var point = gesture.LocationInView(MapView);
                var touchedCooridinate = MapView.ConvertPoint(point, MapView);
                var position = new Position(touchedCooridinate.Latitude, touchedCooridinate.Longitude);
                View.DidTapOnMapCommand?.Invoke(position, new Point((float)point.X, (float)point.Y));
            });
        }

        void SetupUserInterface()
        {
            MapView = new MGLMapView(new CGRect(View.ActualX, View.ActualY, View.ActualWidth, View.ActualHeight))
            {
                ShowsUserLocation = true,
                PitchEnabled = View.PitchEnabled,
                RotateEnabled = View.RotateEnabled,
                ZoomLevel = View.ZoomLevel
            };
            UpdateMapStyle();
            UpdateCenter();
        }

        void UpdateCenter()
        {
            if (View.Center != null && MapView != null
                && (!View.Center.Lat.Equals(MapView.CenterCoordinate.Latitude)
                    || !View.Center.Long.Equals(MapView.CenterCoordinate.Longitude)))
            {
                MapView.SetCenterCoordinate(new CLLocationCoordinate2D(View.Center.Lat, View.Center.Long), true);
            }
        }

        void UpdateMapStyle()
        {
            if (!string.IsNullOrEmpty(View.MapStyle?.UrlString))
            {
                MapView.StyleURL = new NSUrl(View.MapStyle.UrlString);
                View.MapStyle.CustomLayersChanged.Handle(() =>
                {
                    if (View.MapStyle.CustomLayers != null)
                    {
                        if (View.MapStyle.CustomLayers is INotifyCollectionChanged notifiyCollection)
                        {
                            notifiyCollection.CollectionChanged += OnLayersCollectionChanged;
                        }

                        AddLayers(View.MapStyle.CustomLayers.ToList());
                    }
                });
                View.MapStyle.CustomSourcesChanged.Handle(() =>
                {
                    if (View.MapStyle.CustomSources != null)
                    {
                        if (View.MapStyle.CustomLayers is INotifyCollectionChanged notifiyCollection)
                        {
                            notifiyCollection.CollectionChanged += OnLayersCollectionChanged;
                        }

                        AddLayers(View.MapStyle.CustomLayers.ToList());
                    }
                });
            }
        }

        protected void SetupFunctions()
        {
            View.TakeSnapshotFunc = () =>
            {
                var image = MapView.Capture();
                var imageData = image.AsJPEG();
                var imgByteArray = new byte[imageData.Length];
                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes,
                                                            imgByteArray,
                                                            0,
                                                            Convert.ToInt32(imageData.Length));
                return imgByteArray;
            };

            View.GetFeaturesAroundPointFunc = (point, radius, layers) =>
            {
                var selectableLayers = SelectableLayersFromSources(layers);
                NSObject[] features;
                var cgPoint = new CGPoint(point.X, point.Y);
                if (radius <= 0)
                {
                    features = MapView.VisibleFeaturesAtPoint(cgPoint, selectableLayers);
                }
                else
                {
                    var rect = new CGRect(cgPoint.X - (nfloat)radius, cgPoint.Y - (nfloat)radius, (nfloat)radius * 2, (nfloat)radius * 2);
                    features = MapView.VisibleFeaturesInRect(rect, selectableLayers);
                }

                var output = new List<IFeature>();

                foreach (var obj in features)
                {
                    var feature = obj as IMGLFeature;
                    if (feature?.Attributes == null)
                    {
                        continue;
                    }

                    string id = null;
                    if (feature.Identifier != null)
                    {
                        if (feature.Identifier is NSNumber number)
                        {
                            id = number.StringValue;
                        }
                        else
                        {
                            id = feature.Identifier.ToString();
                        }
                    }

                    if (id == null || output.Any((arg) => (arg as Annotation)?.Id == id))
                    {
                        continue;
                    }

                    var geoData = feature.GeoJSONDictionary;
                    if (geoData == null) continue;

                    IFeature ifeat = null;

                    if (feature is MGLPointFeature pointFeature)
                    {
                        ifeat = new PointFeature();
                        ((PointFeature)ifeat).Title = pointFeature.Title;
                        ((PointFeature)ifeat).SubTitle = pointFeature.Subtitle;
                        ((PointFeature)ifeat).Coordinate = TypeConverter.FromCoordinateToPosition(pointFeature.Coordinate);
                    }
                    else
                    {
                        var geometry = geoData["geometry"];
                        NSArray coorArr = null;
                        var coordinates = (geometry as NSDictionary)?["coordinates"];
                        if (coordinates is NSArray)
                        {
                            coorArr = coordinates as NSArray;
                        }

                        if (feature is MGLPolylineFeature polylineFeature)
                        {
                            ifeat = new PolylineFeature();
                            ((PolylineFeature)ifeat).Title = polylineFeature.Title;
                            ((PolylineFeature)ifeat).SubTitle = polylineFeature.Subtitle;

                            if (coorArr != null)
                            {
                                var coorsList = new List<Position>();
                                ((PolylineFeature)ifeat).Coordinates = new Position[coorArr.Count];
                                for (nuint i = 0; i < coorArr.Count; i++)
                                {
                                    var childArr = coorArr.GetItem<NSArray>(i);
                                    if (childArr == null || childArr.Count != 2) continue;
                                    var coord = new Position(childArr.GetItem<NSNumber>(1).DoubleValue, //lat
                                        childArr.GetItem<NSNumber>(0).DoubleValue); //long
                                    coorsList.Add(coord);
                                }

                                ((PolylineFeature)ifeat).Coordinates = new ObservableCollection<Position>(coorsList).ToArray();
                            }
                        }
                        else if (feature is MGLMultiPolylineFeature)
                        {
                            ifeat = new MultiPolylineFeature();
                            ((MultiPolylineFeature)ifeat).Title = ((MGLMultiPolylineFeature)feature).Title;
                            ((MultiPolylineFeature)ifeat).SubTitle = ((MGLMultiPolylineFeature)feature).Subtitle;
                            if (coorArr != null)
                            {
                                ((MultiPolylineFeature)ifeat).Coordinates = new Position[coorArr.Count][];
                                for (nuint i = 0; i < coorArr.Count; i++)
                                {
                                    var childArr = coorArr.GetItem<NSArray>(i);
                                    if (childArr == null) continue;
                                    ((MultiPolylineFeature)ifeat).Coordinates[i] = new Position[childArr.Count];
                                    for (nuint j = 0; j < childArr.Count; j++)
                                    {
                                        var anscArr = childArr.GetItem<NSArray>(j);
                                        if (anscArr != null && anscArr.Count == 2)
                                        {
                                            ((MultiPolylineFeature)ifeat).Coordinates[i][j] = new Position(anscArr.GetItem<NSNumber>(1).DoubleValue, //lat
                                                anscArr.GetItem<NSNumber>(0).DoubleValue);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (ifeat == null) continue;
                    ((Annotation)ifeat).Id = id;
                    ifeat.Attributes = ConvertDictionary(feature.Attributes);
                    output.Add(ifeat);
                }

                return output.ToArray();
            };

            View.ResetPositionAction = () =>
            {
                MapView.ResetPosition();
            };

            View.ReloadStyleAction = () =>
            {
                MapView.ReloadStyle(MapView);
            };

            View.UpdateShapeOfSourceFunc = (annotation, sourceId) =>
            {
                if (annotation != null && !string.IsNullOrEmpty(sourceId))
                {
                    var mglSource = MapView.Style.SourceWithIdentifier(sourceId.ToCustomId());
                    if (mglSource is MGLShapeSource source)
                    {
                        Thread.UI.Run(() =>
                        {
                            source.Shape = ShapeFromAnnotation(annotation);
                        });
                        if (View.MapStyle.CustomSources != null)
                        {
                            var count = View.MapStyle.CustomSources.Count();
                            for (var i = 0; i < count; i++)
                            {
                                if (View.MapStyle.CustomSources.ElementAt(i).Id == sourceId)
                                {
                                    View.MapStyle.CustomSources.ElementAt(i).Shape = annotation;
                                    break;
                                }
                            }
                        }

                        return true;
                    }
                }

                return false;
            };

            View.UpdateLayerFunc = (layerId, isVisible, isCustom) =>
            {
                if (string.IsNullOrEmpty(layerId)) return false;
                var layerIdStr = isCustom ? layerId.ToCustomId() : (NSString)layerId;
                var layer = MapView.Style.LayerWithIdentifier(layerIdStr);
                if (layer == null) return false;
                layer.Visible = isVisible;
                if (!isCustom || View.MapStyle.CustomLayers == null) return true;
                var count = View.MapStyle.CustomLayers.Count();
                for (var i = 0; i < count; i++)
                {
                    if (View.MapStyle.CustomLayers.ElementAt(i).Id != layerId) continue;
                    View.MapStyle.CustomLayers.ElementAt(i).IsVisible = isVisible;
                    break;
                }

                return true;
            };

            View.UpdateViewPortAction = (centerLocation, zoomLevel, bearing, animated, completionBlock) =>
            {
                MapView.SetCenterCoordinate(
                    centerLocation?.ToCLCoordinate() ?? MapView.CenterCoordinate,
                    zoomLevel ?? MapView.ZoomLevel,
                    bearing ?? MapView.Direction,
                    animated,
                    completionBlock
                );
            };

            View.ToggleScaleBarFunc = show =>
            {
                if (MapView?.ScaleBar == null) return false;
                Thread.UI.RunAction(() =>
                {
                    MapView.ScaleBar.Hidden = !show;
                });

                return true;
            };

            View.GetStyleImageFunc = (imageName) =>
            {
                if (string.IsNullOrEmpty(imageName) || MapView?.Style == null) return null;
                return MapView.Style.ImageForName(imageName)?.AsPNG().ToArray();
            };

            View.GetStyleLayerFunc = (layerId, isCustom) =>
            {
                if (string.IsNullOrEmpty(layerId) || MapView?.Style == null) return null;
                var layerIdStr = isCustom ? layerId.ToCustomId() : (NSString)layerId;
                var layer = MapView.Style.LayerWithIdentifier(layerIdStr);
                if (layer is MGLVectorStyleLayer vLayer) return CreateStyleLayer(vLayer, layerId);
                return null;
            };
        }

        NSSet<NSString> SelectableLayersFromSources(string[] layersId)
        {
            if (layersId == null)
            {
                return null;
            }

            var output = new NSMutableSet<NSString>();
            foreach (var layerId in layersId)
            {
                var acceptedId = layerId.Replace("_", "-");
                output.Add((NSString)acceptedId);
                output.Add((NSString)(acceptedId + " (1)"));
            }

            return new NSSet<NSString>(output);
        }

        void AddAnnotation(Annotation annotation)
        {
            var shape = ShapeFromAnnotation(annotation);
            if (shape != null)
            {
                MapView.AddAnnotation(shape);
            }
        }

        void AddAnnotations(Annotation[] annotations)
        {
            MapView.AddAnnotations(annotations.Select(ShapeFromAnnotation).Where(shape => shape != null).Cast<IMGLAnnotation>().ToArray());
        }

        void RemoveAnnotations(Annotation[] annotations)
        {
            var currentAnnotations = MapView.Annotations;
            if (currentAnnotations == null)
            {
                return;
            }

            var annots = new List<MGLShape>();
            foreach (var at in annotations)
            {
                annots.AddRange(currentAnnotations.Select(curAnnot => curAnnot as MGLShape).Where(shape => !string.IsNullOrEmpty(shape?.Id())).Where(shape => shape.Id() == at.Id));
            }

            MapView.RemoveAnnotations(annots.ToArray());
        }

        void RemoveAllAnnotations()
        {
            if (MapView?.Annotations != null)
            {
                MapView.RemoveAnnotations(MapView.Annotations);
            }
        }

        void OnAnnotationsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var annots = new List<MGLShape>();
                        foreach (Annotation annot in e.NewItems)
                        {
                            var shape = ShapeFromAnnotation(annot);
                            if (shape != null)
                            {
                                annots.Add(shape);
                            }
                        }

                        MapView.AddAnnotations(annots.ToArray());
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    RemoveAnnotations(e.OldItems.Cast<Annotation>().ToArray());
                    break;
                case NotifyCollectionChangedAction.Reset:
                    RemoveAllAnnotations();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        RemoveAnnotations(e.OldItems.Cast<Annotation>().ToArray());
                        var annots = new List<MGLShape>();
                        foreach (Annotation annot in e.NewItems)
                        {
                            var shape = ShapeFromAnnotation(annot);
                            if (shape != null)
                            {
                                annots.Add(shape);
                            }
                        }

                        MapView.AddAnnotations(annots.ToArray());
                        break;
                    }
            }
        }
        void OnLayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddLayers(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveLayers(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var layersToRemove = MapView.Style.Layers.Where(layer => layer.Identifier.IsCustomId()).ToList();
                    foreach (var layer in layersToRemove)
                        MapView.Style.RemoveLayer(layer);


                    layersToRemove.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveLayers(e.OldItems);

                    AddLayers(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                default:
                    break;
            }
        }

        void AddLayers(IEnumerable layers)
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

                var id = layer.Id.ToCustomId();
                var oldLayer = MapView.Style.LayerWithIdentifier(id);
                if (oldLayer != null)
                {
                    MapView.Style.RemoveLayer(oldLayer);
                }

                if (!(layer is StyleLayer sl)) continue;
                var newLayer = GetStyleLayer(sl, id);
                if (newLayer != null)
                {
                    MapView.Style.AddLayer(newLayer);
                }
            }
        }

        void RemoveLayers(IEnumerable layers)
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

                var id = layer.Id.ToCustomId();
                var oldLayer = MapView.Style.LayerWithIdentifier(id);
                if (oldLayer != null)
                {
                    MapView.Style.RemoveLayer(oldLayer);
                }
            }
        }

        void OnShapeSourcesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddSources(e.NewItems);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveSources(e.OldItems);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    var sourcesToRemove = MapView.Style.Sources.Cast<MGLSource>().Where(source => source.Identifier.IsCustomId()).ToList();
                    foreach (var source in sourcesToRemove)
                        MapView.Style.RemoveSource(source);


                    sourcesToRemove.Clear();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveSources(e.OldItems);
                    AddSources(e.NewItems);
                    break;
            }
        }

        void AddSources(IEnumerable sources)
        {
            if (sources == null || MapView?.Style == null)
            {
                return;
            }

            foreach (ShapeSource source in sources)
            {
                if (source.Id != null && source.Shape != null)
                {
                    var shape = ShapeFromAnnotation(source.Shape);
                    var sourceId = source.Id.ToCustomId();
                    var oldSource = MapView.Style?.SourceWithIdentifier(sourceId);
                    if (oldSource is MGLShapeSource)
                    {
                        (oldSource as MGLShapeSource).Shape = shape;
                    }
                    else
                    {
                        var mglSource = new MGLShapeSource(sourceId, shape, null);
                        MapView.Style.AddSource(mglSource);
                    }
                }
            }
        }

        void RemoveSources(IEnumerable sources)
        {
            if (sources == null)
            {
                return;
            }

            foreach (ShapeSource source in sources)
            {
                if (source.Id == null) continue;
                if (MapView.Style.SourceWithIdentifier(source.Id.ToCustomId()) is MGLShapeSource oldSource)
                {
                    MapView.Style.RemoveSource(oldSource);
                }
            }
        }

        MGLShape ShapeFromAnnotation(Annotation annotation)
        {
            MGLShape shape = null;
            switch (annotation)
            {
                case PointAnnotation pointAnnotation:
                    shape = new MGLPointAnnotation()
                    {
                        Coordinate = pointAnnotation.Coordinate.ToCLCoordinate()
                    };
                    break;
                case PolylineAnnotation _:
                    {
                        var polyline = annotation as PolylineAnnotation;
                        shape = PolyLineWithCoordinates(polyline.Coordinates.ToArray());
                        // var notifiyCollection = polyline.Coordinates as INotifyCollectionChanged;
                        // if (notifiyCollection != null)
                        // {
                        //    notifiyCollection.CollectionChanged += (sender, e) => {
                        //        if (e.Action == NotifyCollectionChangedAction.Add)
                        //        {
                        //            foreach (Position pos in e.NewItems)
                        //            {
                        //                var coord = TypeConverter.FromPositionToCoordinate(pos);
                        //                (shape as MGLPolyline).AppendCoordinates(ref coord, 1);
                        //            }
                        //        }
                        //        else if (e.Action == NotifyCollectionChangedAction.Remove)
                        //        {
                        //            (shape as MGLPolyline).RemoveCoordinatesInRange(new NSRange(e.OldStartingIndex, e.OldItems.Count));
                        //        }
                        //    };
                        // }

                        break;
                    }
                case MultiPolylineAnnotation _:
                    {
                        var polyline = annotation as MultiPolylineAnnotation;
                        if (polyline != null && (polyline.Coordinates == null || polyline.Coordinates.Length == 0))
                        {
                            return null;
                        }

                        if (polyline != null)
                        {
                            var lines = new MGLPolyline[polyline.Coordinates.Length];
                            for (var i = 0; i < polyline.Coordinates.Length; i++)
                            {
                                if (polyline.Coordinates[i].Length == 0)
                                {
                                    continue;
                                }

                                lines[i] = PolyLineWithCoordinates(polyline.Coordinates[i]);
                            }

                            shape = MGLMultiPolyline.MultiPolylineWithPolylines(lines);
                        }

                        break;
                    }
            }

            if (shape != null)
            {
                if (annotation.Title != null)
                {
                    shape.Title = annotation.Title;
                }

                if (annotation.SubTitle != null)
                {
                    shape.Subtitle = annotation.SubTitle;
                }

                if (!string.IsNullOrEmpty(annotation.Id))
                {
                    shape.SetId(annotation.Id);
                }
            }

            return shape;
        }

        MGLPolyline PolyLineWithCoordinates(Position[] positions)
        {
            if (positions == null || positions.Length == 0)
            {
                return null;
            }

            var first = positions[0].ToCLCoordinate();
            var output = MGLPolyline.PolylineWithCoordinates(ref first, 1);
            var i = 1;
            while (i < positions.Length)
            {
                var coord = positions[i].ToCLCoordinate();
                // TODO
                // output.AppendCoordinates(ref coord, 1);
                i++;
            }

            return output;
        }

        #region MGLMapViewDelegate
        [Export("mapViewDidFinishRenderingMap:fullyRendered:"),]
        void DidFinishRenderingMap(MGLMapView mapView, bool fullyRendered)
        {
            View.DidFinishRenderingCommand?.Invoke(
                fullyRendered);
        }

        [Export("mapView:didUpdateUserLocation:"),]
        void DidUpdateUserLocation(MGLMapView mapView, MGLUserLocation userLocation)
        {
            if (userLocation != null)
            {
                View.UserLocation = new Position(
                    userLocation.Location.Coordinate.Latitude,
                    userLocation.Location.Coordinate.Longitude
                );
            }
        }

        [Export("mapView:didFinishLoadingStyle:"),]
        void DidFinishLoadingStyle(MGLMapView mapView, MGLStyle style)
        {
            MapStyle newStyle;
            if (View.MapStyle == null)
            {
                newStyle = new MapStyle(mapView.StyleURL.AbsoluteString) { Name = style.Name };
                View.MapStyle = newStyle;
            }
            else
            {
                if (View.MapStyle.UrlString == null
                || View.MapStyle.UrlString != mapView.StyleURL.AbsoluteString)
                {
                    View.MapStyle.SetUrl(mapView.StyleURL.AbsoluteString);
                    View.MapStyle.Name = style.Name;
                }

                newStyle = View.MapStyle;
            }

            if (View.MapStyle.CustomSources != null)
            {
                if (View.MapStyle.CustomSources is INotifyCollectionChanged notifiyCollection)
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

            newStyle.OriginalLayers = style.Layers.Select(arg => new Layer(arg.Identifier)
            {
                IsVisible = arg.Visible
            }
                                                         ).ToArray();
            newStyle.Name = style.Name;
            View.DidFinishLoadingStyleCommand?.Invoke(newStyle);
        }

        [Export("mapViewRegionIsChanging:"),]
        void RegionIsChanging(MGLMapView mapView)
        {
            View.Center = new Position(mapView.CenterCoordinate.Latitude, mapView.CenterCoordinate.Longitude);
        }

        [Export("mapView:regionDidChangeAnimated:"),]
        void RegionDidChangeAnimated(MGLMapView mapView, bool animated)
        {
            View.ZoomLevel = mapView.ZoomLevel;
            View.Pitch = (double)mapView.Camera.Pitch;
            View.RotatedDegree = (double)mapView.Camera.Heading;
            View?.RegionDidChangeCommand?.Invoke(animated);
        }

        [Export("mapView:annotationCanShowCallout:"),]
        bool AnnotationCanShowCallout(MGLMapView mapView, NSObject annotation)
        {
            if (annotation is MGLShape shape && View.CanShowCalloutChecker != null)
            {
                return View.CanShowCalloutChecker.Invoke(shape.Id());
            }

            return true;
        }
        #endregion

        #region UIGestureRecognizerDelegate
        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
        {
            return true;
        }

        #endregion

        Dictionary<string, object> ConvertDictionary(NSDictionary fromDict)
        {
            var output = new Dictionary<string, object>();
            foreach (var o in fromDict.Keys)
            {
                var key = (NSString)o;
                if (fromDict[key] is NSString str)
                {
                    if (str == "<NULL>")
                    {
                        continue;
                    }

                    output[key] = (string)str;
                }
                else if (fromDict[key] is NSNumber)
                {
                    output[key] = ((NSNumber)fromDict[key]).DoubleValue;
                }
                else if (fromDict[key] is NSDate)
                {
                    output[key] = ((NSDate)fromDict[key]).ToDateTimeOffset();
                }
                else
                {
                    output[key] = fromDict[key].ToString();
                }
            }

            return output;
        }

        public void Dispose()
        {
        }

        #region STYLELAYER

        NSObject GetValueFromCameraStyleFunction(MGLCameraStyleFunction csFunc)
        {
            if (csFunc.Stops == null || csFunc.Stops.Count == 0) return null;
            MGLStyleValue output = null;
            switch (csFunc.InterpolationMode)
            {
                case MGLInterpolationMode.Identity:
                    nuint i = 0;
                    while (i < csFunc.Stops.Count)
                    {
                        var key = csFunc.Stops.Keys[i];
                        var zoomLevel = ((NSNumber)key).DoubleValue;
                        if (zoomLevel < MapView.ZoomLevel)
                        {
                            output = csFunc.Stops[key];
                        }
                        else
                        {
                            break;
                        }

                        i++;
                    }

                    break;
                case MGLInterpolationMode.Exponential:
                    break;
                case MGLInterpolationMode.Interval:
                    break;
                case MGLInterpolationMode.Categorical:
                    break;
                default: break;
            }

            if (output == null)
            {
                output = csFunc.Stops.Values[0];
            }

            return GetObjectFromStyleValue(output);
        }

        NSObject GetObjectFromStyleValue(MGLStyleValue value)
        {
            switch (value)
            {
                case MGLConstantStyleValue cValue:
                    return cValue.RawValue;
                case MGLCameraStyleFunction csFunc:
                    return GetValueFromCameraStyleFunction(csFunc);
            }

            if (value != null && value.RespondsToSelector(new ObjCRuntime.Selector("rawValue")))
            {
                return value.ValueForKey((NSString)"rawValue");
            }

            return value;
        }

        MGLVectorStyleLayer GetStyleLayer(StyleLayer styleLayer, NSString id)
        {
            if (string.IsNullOrEmpty(styleLayer.SourceId))
            {
                return null;
            }

            var sourceId = styleLayer.SourceId.ToCustomId();

            var source = MapView.Style.SourceWithIdentifier(sourceId);
            if (source == null)
            {
                return null;
            }

            if (styleLayer is CircleLayer circleLayer)
            {
                var newLayer = new MGLCircleStyleLayer(id, source)
                {
                    CircleColor = MGLStyleValue.ValueWithRawValue(circleLayer.CircleColor.Render()),
                    CircleOpacity = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(circleLayer.CircleOpacity)),
                    CircleRadius = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(circleLayer.CircleRadius))
                };
                if (circleLayer.StrokeColor is Color strokeColor)
                {
                    newLayer.CircleStrokeColor = MGLStyleValue.ValueWithRawValue(strokeColor.Render());
                    newLayer.CircleStrokeOpacity = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(circleLayer.StrokeOpacity));
                    newLayer.CircleStrokeWidth = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(circleLayer.StrokeWidth));
                }

                return newLayer;
            }

            if (styleLayer is LineLayer lineLayer)
            {
                var newLayer = new MGLLineStyleLayer(id, source)
                {
                    LineWidth = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(lineLayer.LineWidth)),
                    LineColor = MGLStyleValue.ValueWithRawValue(lineLayer.LineColor.Render())
                };
                if (lineLayer.Dashes != null && lineLayer.Dashes.Length != 0)
                {
                    var arr = new NSMutableArray<NSNumber>();
                    foreach (var dash in lineLayer.Dashes)
                        arr.Add(NSNumber.FromDouble(dash));


                    newLayer.LineDashPattern = MGLStyleValue.ValueWithRawValue(arr);
                }

                return newLayer;
            }

            if (styleLayer is FillLayer fl)
            {
                var newLayer = new MGLFillStyleLayer(id, source)
                {
                    FillColor = MGLStyleValue.ValueWithRawValue(fl.FillColor.Render()),
                    FillOpacity = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(fl.FillOpacity))
                };
                return newLayer;
            }

            if (styleLayer is SymbolLayer sl)
            {
                var newLayer = new MGLSymbolStyleLayer(id, source)
                {
                    IconImageName = MGLConstantStyleValue.ValueWithRawValue((NSString)sl.IconImageName),
                    IconOpacity = MGLStyleValue.ValueWithRawValue(NSNumber.FromDouble(sl.IconOpacity))
                };
                return newLayer;
            }

            return null;
        }

        StyleLayer CreateStyleLayer(MGLVectorStyleLayer vectorLayer, string layerId = null)
        {
            if (vectorLayer is MGLSymbolStyleLayer sl && sl.IconImageName != null)
            {
                var newLayer = new SymbolLayer(layerId ?? vectorLayer.Identifier, vectorLayer.SourceIdentifier.TrimCustomId());
                if (sl.IconImageName is MGLCameraStyleFunction csFunc)
                {
                    var imgName = GetValueFromCameraStyleFunction(csFunc);
                    if (imgName != null)
                    {
                        newLayer.IconImageName = imgName.ToString();
                    }
                }
                else
                {
                    var imgName = GetObjectFromStyleValue(sl.IconImageName);
                    if (imgName != null)
                    {
                        newLayer.IconImageName = imgName.ToString();
                    }
                }

                return newLayer;
            }

            if (vectorLayer is MGLLineStyleLayer ll)
            {
                var newLayer = new LineLayer(layerId ?? vectorLayer.Identifier, vectorLayer.SourceIdentifier.TrimCustomId())
                {
                    LineColor = (GetObjectFromStyleValue(ll.LineColor) as UIColor).ToColor()
                };

                if (ll.LineDashPattern != null)
                {
                    if (GetObjectFromStyleValue(ll.LineDashPattern) is NSArray arr && arr.Count != 0)
                    {
                        var dash = new List<double>();
                        for (nuint i = 0; i < arr.Count; i++)
                        {
                            var obj = arr.GetItem<NSNumber>(i);
                            dash.Add(obj.DoubleValue);
                        }

                        newLayer.Dashes = dash.ToArray();
                    }
                    else
                    {
                        // TODO
                    }
                }

                return newLayer;
            }

            if (vectorLayer is MGLCircleStyleLayer cl)
            {
                var newLayer = new CircleLayer(layerId ?? vectorLayer.Identifier, vectorLayer.SourceIdentifier.TrimCustomId())
                {
                    CircleColor = (GetObjectFromStyleValue(cl.CircleColor) as UIColor)?.ToColor() ?? Colors.Transparent
                };
                return newLayer;
            }

            if (vectorLayer is MGLFillStyleLayer fl)
            {
                var newLayer = new FillLayer(layerId ?? vectorLayer.Identifier, vectorLayer.SourceIdentifier.TrimCustomId())
                {
                    FillColor = (GetObjectFromStyleValue(fl.FillColor) as UIColor)?.ToColor() ?? Colors.Transparent
                };
                return newLayer;
            }

            return null;
        }

        #endregion

        public IntPtr Handle { get; }
    }

    public static class NsDateExtensions
    {
        static DateTime _nsRef = new DateTime(2001, 1, 1, 0, 0, 0, 0, DateTimeKind.Local); // last zero is millisecond

        public static DateTimeOffset ToDateTimeOffset(this NSDate date)
        {
            var interval = date.SecondsSinceReferenceDate;
            return _nsRef.AddSeconds(interval);
        }
    }

    public static class StringExtensions
    {
        static string CustomPrefix = "NXCustom_";
        public static NSString ToCustomId(this string str)
        {
            if (str == null) return null;
            return (NSString)(CustomPrefix + str);
        }

        public static bool IsCustomId(this string str)
        {
            if (str == null) return false;
            return str.StartsWith(CustomPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public static string TrimCustomId(this string str)
        {
            if (str.StartsWith(CustomPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return str.Substring(CustomPrefix.Length);
            }

            return str;
        }
    }
    public static class ColorExtensions
    {
        public static Color ToColor(this UIColor uiColor)
        {
            return new Color(

                (byte)(uiColor.CIColor.Red * 255),
                (byte)(uiColor.CIColor.Green * 255),
                (byte)(uiColor.CIColor.Blue * 255),
                (byte)(uiColor.CIColor.Alpha * 255));
        }
    }
}