using Android.Util;
namespace Zebble.Plugin.MBox
{
    public static class ContextExtensions
    {
        public static float ToPixels(this Android.Content.Context context, float dip)
        {
            var displayMetrics = context.Resources.DisplayMetrics;
            return TypedValue.ApplyDimension(TypedValue.DensityDefault, dip, displayMetrics);
        }
    }
}