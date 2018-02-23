
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace Zebble.Plugin.MBox
{

    public class MapViewRenderer : INativeRenderer
    {
        MapBox View;

        public async Task<FrameworkElement> Render(Renderer renderer)
        {
            var native = await (new MapBox()).Render();
            return native.Native();
        }
        public void Dispose() => View.Dispose();
    }
}