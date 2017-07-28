namespace Zebble.Plugin
{
    using System;
    using System.Collections.Generic;
    using Zebble.Services;

    partial class MapBox
    {
        public class Annotation
        {
            public string Title, SubTitle, IconPath, Id, Content;
            public bool Draggable, Flat, Visible = true;
            public GeoLocation Location = new GeoLocation();

            public AsyncEvent<Annotation> Tapped = new AsyncEvent<Annotation>();

            public object Native { get; internal set; }
        }
    }
}