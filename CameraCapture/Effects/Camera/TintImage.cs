using System;
using Xamarin.Forms;

namespace CameraCapture.Effects.Camera
{
    public class TintImage : RoutingEffect
    {
        public Color TintColor { get; set; }
        public TintImage() : base($"MyCompany.TintImage")
        {
            TintColor = Color.White;
        }

    }
}
