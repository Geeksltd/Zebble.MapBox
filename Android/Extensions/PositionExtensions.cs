using Mapbox.Geometry;

namespace Zebble.Plugin.MBox
{
    public static class PositionExtensions
    {
        public static LatLng ToLatLng(this Position pos) => new LatLng(pos.Lat, pos.Long);
    }
}
