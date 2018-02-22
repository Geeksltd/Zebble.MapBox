using CoreLocation;

namespace Zebble.Plugin.MBox
{
    public static class PositionExtensions
    {
        public static CLLocationCoordinate2D ToCLCoordinate(this Position pos)
        {
            return new CLLocationCoordinate2D(pos.Lat, pos.Long);
        }
    }
}
