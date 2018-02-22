using System;
using Mapbox;

namespace Zebble.Plugin.MBox
{
    public static class MGLOfflinePackProgressExtensions
    {
        public static OfflinePackProgress ToFormsProgress(this MGLOfflinePackProgress progress)
        {
            var formsProgress = new OfflinePackProgress()
            {
                CountOfBytesCompleted = progress.countOfBytesCompleted,
                CountOfTilesCompleted = progress.countOfTilesCompleted,
                CountOfResourcesExpected = progress.countOfResourcesExpected,
                CountOfResourcesCompleted = progress.countOfResourcesCompleted,
                CountOfTileBytesCompleted = progress.countOfTileBytesCompleted,
                MaximumResourcesExpected = progress.maximumResourcesExpected
            };
            return formsProgress;
        }
    }
}
