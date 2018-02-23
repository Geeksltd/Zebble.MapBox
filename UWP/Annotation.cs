using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zebble.Services;

namespace Zebble.Plugin.MBox
{
    public partial class Annotation
    {
        public string IconPath, Content;
        public bool Draggable, Flat, Visible = true;
        public GeoLocation Location = new GeoLocation();

        public AsyncEvent<Annotation> Tapped = new AsyncEvent<Annotation>();

        public object Native { get; internal set; }
    }
}
