
namespace Zebble.Plugin
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Framework;

    partial class MapBox : WebView
    {
        const bool SupportsWebGL = true;

        public override async Task OnPreRender()
        {
            await base.OnPreRender();
            MergeExternalResources = false;

            if (AccessToken.LacksValue())
                throw new Exception("MapBox access token is not set. Expected config key: 'MapBox.Access.Token'.");

            AnnotationAdded += RenderAnnotation;
            AnnotationRemoved += UnrenderAnnotation;

            Html = GenerateHtml();

            LoadFinished.Handle(() => OnShown());

            BrowserNavigating.Handle((e) =>
            {
                if (e.Url.Contains("Annotation_Tap"))
                {
                    var annotation = Annotations.FirstOrDefault(a => a.SubTitle == e.Url.TrimBefore("Annotation_Tap_").Remove("Annotation_Tap_"));
                    AnnotationClicked.Invoke(annotation);
                    e.Cancel = true;
                }
            });
        }

        public void OnShown()
        {
            Annotations.Do(RenderAnnotation);
        }

        string GenerateHtml()
        {
            var r = new StringBuilder();

            r.Append(
            @"<html>
            <head>
            <meta charset=""utf-8"" />
            <title>A simple map</title>
            <meta name='viewport' content='initial-scale=1,maximum-scale=1,user-scalable=no' />");

            if (SupportsWebGL)
            {
                r.AppendLine(@"<script src='https://api.mapbox.com/mapbox-gl-js/v0.32.1/mapbox-gl.js'></script>");
                r.AppendLine(@"<link href='https://api.mapbox.com/mapbox-gl-js/v0.32.1/mapbox-gl.css' rel='stylesheet'/>");
            }
            else
            {
                r.AppendLine(@"<script src='https://api.mapbox.com/mapbox.js/v3.0.1/mapbox.js'></script>");
                r.AppendLine(@"<link href='https://api.mapbox.com/mapbox.js/v3.0.1/mapbox.css' rel='stylesheet'/>");
            }


            r.Append(@"
            <style>.marker{
            width:25px;
            height:32px;
            margin-left: -25px;
            margin-top: -32px;
            background-image: url(" +
            Config.Get("Api.Base.Url").OrEmpty().TrimEnd("/")
            + AnnotationImagePath +
            @"); 
            background-size: cover;
            }
            .mapboxgl-popup {
                max-width: 200px;
            }
            h3{
                margin: 5px 10px 0 7px;
            }
            .mapboxgl-popup-close-button {
                padding-right: 3px;
                padding-top: 0px;
            }

            </style>
            </head>

            <body style='margin: 0; padding: 0;' id='body'>
            <div id='map' style='height:100%; width:100%; position:absolute; top:0; bottom:0;'></div>

            <script>");

            if (SupportsWebGL)
            {
                // See https://www.mapbox.com/mapbox-gl-js/api/

                r.AppendLine($@"mapboxgl.accessToken = '{AccessToken}';
                var map = new mapboxgl.Map({{
                    container: 'map',
                    center: [{Center.Longitude}, {Center.Latitude}],
                    zoom: {Zoom},
                    style: '{StyleUrl.Or("mapbox://styles/mapbox/streets-v9")}'
                }});");
            }
            else
            {
                r.AppendLine($"L.mapbox.accessToken = '{AccessToken}';");
                r.AppendLine("var map = L.mapbox.map('map', 'mapbox.streets')");
                r.AppendLine($".setView([{Center.Longitude}, {Center.Latitude}], {Zoom});");

                if (StyleUrl.HasValue()) r.AppendLine($"L.mapbox.styleLayer('{StyleUrl}').addTo(map);");
            }
            r.Append(@"AddAnnotation = function(long, lat, title, subtitle){
                            var el = document.createElement('div');
                            el.className = 'marker'; 
                            var url = ""'http://app.link/Annotation_Tap_""+ subtitle.replace(/'/g, '') + ""'"";
                            var popup = new mapboxgl.Popup({offset: [-14,-35]}).
                            setHTML('<h3 onclick=""Redirect('+ url +'); "">' + title.replace(/'/g, '') + '</h3>');
                            new mapboxgl.Marker(el).setLngLat([long, lat]).setPopup(popup).addTo(map);
                        }");
            r.AppendLine(@"
                            Redirect = function(url){" +
            @"window.location.href = url;
                            };
                </script>
            </body>
            </html>");

            return r.ToString();
        }

        void RenderAnnotation(Annotation annotation)
        {
            EvaluateJavaScriptFunction("AddAnnotation", new string[] { annotation.Location.Longitude.ToString(), annotation.Location.Latitude.ToString(), "'" + annotation.Title + "'", "'" + annotation.SubTitle + "'" });
        }

        void UnrenderAnnotation(Annotation annotation)
        {
            // TODO: When rendering, keep a reference.
        }
    }
}
