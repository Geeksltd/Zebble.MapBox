using System;
using System.Collections.Generic;

namespace Zebble.Plugin.MBox
{
    public class OfflinePack
    {
        public OfflinePackRegion Region
        {
            get;
            set;
        }

        public Dictionary<string, string> Info
        {
            get;
            set;
        }

        public OfflinePackProgress Progress
        {
            get;
            set;
        }

        public OfflinePackState State
        {
            get;
            set;
        }

        public IntPtr Handle
        {
            get;
            set;
        }
    }
}