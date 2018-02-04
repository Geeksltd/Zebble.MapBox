using Android.Graphics;
using Mapbox.Services.Commons.GeoJson;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Zebble.Plugin.MBox
{
    using Point = Mapbox.Services.Commons.GeoJson.Point;
    using Position = Mapbox.Services.Commons.Models.Position;
    public static class FeatureExtensions
    {
        public static IFeature ToFeature(this Feature feature)
        {
            if (feature == null)
            {
                return null;
            }

            IFeature forms = null;

            if (feature.Geometry is Point)
            {
                var point = (Point)feature.Geometry;

                forms = new PointFeature(new PointAnnotation
                {
                    Id = feature.Id,
                    Coordinate = point.Coordinates.ToZebble()
                });
            }
            else if (feature.Geometry is LineString)
            {
                var line = (LineString)feature.Geometry;
                var coords = line.Coordinates
                                  .Select(ToZebble)
                                  .ToArray();

                forms = new PolylineFeature(new PolylineAnnotation
                {
                    Id = feature.Id,
                    Coordinates = coords
                });
            }
            else if (feature.Geometry is MultiLineString)
            {
                var line = (MultiLineString)feature.Geometry;
                var coords = line.Coordinates
                                  .Select(x => x.Select(ToZebble).ToArray())
                                  .ToArray();

                forms = new MultiPolylineFeature(new MultiPolylineAnnotation
                {
                    Id = feature.Id,
                    Coordinates = coords
                });
            }

            if (forms != null)
            {
                var json = feature.Properties.ToString();

                forms.Attributes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
            }

            return forms;
        }

        public static MBox.Position ToZebble(this Mapbox.Services.Commons.Models.Position position)
        {
            return new MBox.Position
            {
                Lat = position.Latitude,
                Long = position.Longitude
            };
        }

        public static RectF ToRect(this Zebble.Point point, double radius)
        {
            return new RectF(
                            (float)(point.X - radius),
                            (float)(point.Y - radius),
                            (float)(point.X + radius),
                            (float)(point.Y + radius));
        }

        public static IEnumerable<T> Cast<T>(this Android.Runtime.JavaList list)
        {
            return list.ToArray().Cast<T>();
        }
    }
}
