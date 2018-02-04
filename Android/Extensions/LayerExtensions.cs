using System;

namespace Zebble.Plugin.MBox
{
    public static class LayerExtensions
    {
        public static Mapbox.Style.Layers.CircleLayer ToNative(this CircleLayer layer)
        {
            if (layer == null) { return null; }

            var native = new Mapbox.Style.Layers.CircleLayer(layer.Id.Prefix(), layer.SourceId.Prefix());
            native.SetProperties(
                Mapbox.Style.Layers.PropertyFactory.CircleColor(layer.CircleColor.Render()),
                Mapbox.Style.Layers.PropertyFactory.CircleOpacity(new Java.Lang.Float(layer.CircleOpacity)),
                Mapbox.Style.Layers.PropertyFactory.CircleRadius(new Java.Lang.Float(layer.CircleRadius))
            );

            return native;
        }

        public static Mapbox.Style.Layers.LineLayer ToNative(this LineLayer layer)
        {
            if (layer == null) { return null; }

            var native = new Mapbox.Style.Layers.LineLayer(layer.Id.Prefix(), layer.SourceId.Prefix());
            native.SetProperties(
                Mapbox.Style.Layers.PropertyFactory.LineWidth(new Java.Lang.Float(layer.LineWidth)),
                Mapbox.Style.Layers.PropertyFactory.LineColor(layer.LineColor.Render()),
                Mapbox.Style.Layers.PropertyFactory.LineCap(layer.LineCap.ToString().ToLower()),
                Mapbox.Style.Layers.PropertyFactory.LineOpacity(new Java.Lang.Float(layer.LineOpacity))
            );

            return native;
        }
    }

    public static class IdExtensions
    {
        static string PREFIX = "__naxam_prefix__";

        public static string Prefix(this string self) => $"{PREFIX}{self}";

        public static bool HasPrefix(this string self) => self?.StartsWith($"{PREFIX}") ?? false;
    }
}