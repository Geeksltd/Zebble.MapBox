#if LATER
namespace Zebble.Plugin
{
    using System;
    using System.Threading.Tasks;
    using CoreLocation;
    using Foundation;
    using Mapbox;
    using ObjCRuntime;
    using UIKit;

    partial class MapBox : CustomRenderedView<MapBoxRenderer> { }

    public class MapBoxRenderer : ICustomRenderer
    {
        MapBox View;
        ZebbleMapboxView Result;

        public object Render(object view)
        {
            View = (MapBox)view;

            Result = new ZebbleMapboxView(new CoreGraphics.CGRect(0, 0, 1, 1))
            {
                AnnotationImagePath = View.AnnotationImagePath,
                AnnotationImageSize = View.AnnotationImageSize
            };

            Result.SetZoomLevel(View.Zoom, animated: false);

            Result.SetCenterCoordinate(
                new CLLocationCoordinate2D(View.Center.Latitude,
                View.Center.Longitude), animated: false);

            Result.ShowsUserLocation = View.ShowsUserLocation;

            Result.StyleURL = new Foundation.NSUrl(View.StyleUrl.Or("mapbox://styles/mapbox/streets-v9"));

            View.AnnotationAdded += (a) =>
            {
                Result.AddAnnotation(new PointAnnotation
                {
                    Coordinate = new CLLocationCoordinate2D(a.Location.Latitude, a.Location.Longitude),
                    Title = a.Title,
                    Subtitle = a.SubTitle
                });
            };

            return Result;
        }

        public void Dispose() => Result.Dispose();
    }

    public class ZebbleMapboxView : MapView
    {
        public string AnnotationImagePath;
        public Size AnnotationImageSize;

        public ZebbleMapboxView(CoreGraphics.CGRect frame) : base(frame) { }

        public override AnnotationImage DequeueReusableAnnotationImage(string identifier)
        {
            if (AnnotationImagePath.IsEmpty()) return base.DequeueReusableAnnotationImage(identifier);

            var image = Services.ImageService.GetNativeImage<UIImage>(AnnotationImagePath, AnnotationImageSize, Stretch.Fit).GetAwaiter().GetResult();
            return AnnotationImage.Create(image, identifier);
        }
    }
}
#endif