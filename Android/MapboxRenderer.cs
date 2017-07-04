#if LATER
namespace Zebble.Plugin
{
    using Mapbox;
    using Mapbox.MapboxSdk.Views;
    using Mapbox.MapboxSdk.Api;
    using System;

    partial class MapBox : CustomRenderedView<MapBoxRenderer>
    {
    }

    public class MapBoxRenderer : ICustomRenderer
    {
        MapBox View;
        MapView Result;

        public object Render(object view)
        {
            try
            {
                View = (MapBox)view;

                Result = new MapView(UIRuntime.CurrentActivity);
                Result.SetAccessToken(View.AccessToken);
                Result.SetCenter(new LatLong(View.Center));
                Result.SetZoom(View.ZoomLevel);

                return Result;
            }
            catch (Exception ex)
            {
                Device.Log.Error("Failed to load the MapBox: " + ex.ToFullMessage());
                throw;
            }
        }

        public void Dispose() => Result.Dispose();

        class LatLong : Java.Lang.Object, ILatLng
        {
            Services.IGeoLocation Position;
            public LatLong(Services.IGeoLocation position) { Position = position; }
            public double Altitude => 0;
            public double Latitude => Position.Latitude;
            public double Longitude => Position.Longitude;
        }
    }
}
#endif