using System;
using static Mapbox.Maps.MapboxMap;

namespace Zebble.Plugin.MBox
{
    public class CancelableCallback : Java.Lang.Object, ICancelableCallback
    {
        public Action FinishHandler;
        public Action CancelHandler;

        public void OnCancel() => CancelHandler?.Invoke();

        public void OnFinish() => FinishHandler?.Invoke();
    }
}
