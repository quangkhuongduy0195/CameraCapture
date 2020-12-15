using System;
using System.Linq;
using Android.Graphics;
using Android.Widget;
using CameraCapture.Droid.Effects;
using CameraCapture.Effects.Camera;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ResolutionGroupName("MyCompany")]
[assembly: ExportEffect(typeof(TintImageImpl), nameof(TintImage))]
namespace CameraCapture.Droid.Effects
{
    public class TintImageImpl : PlatformEffect
    {
        protected override void OnAttached()
        {
            try
            {
                var effect = (TintImage)Element.Effects.FirstOrDefault(e => e is TintImage);

                if (effect == null || !(Control is ImageView image))
                    return;

                var filter = new PorterDuffColorFilter(effect.TintColor.ToAndroid(), PorterDuff.Mode.SrcIn);
                image.SetColorFilter(filter);
            }
            catch (Exception ex)
            {

            }
        }

        protected override void OnDetached() { }
    }
}
