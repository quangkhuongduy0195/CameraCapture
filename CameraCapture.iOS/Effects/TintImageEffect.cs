using System;
using System.Linq;
using UIKit;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using CameraCapture.iOS.Effects;
using CameraCapture.Effects.Camera;
using System.ComponentModel;

[assembly: ResolutionGroupName("MyCompany")]
[assembly: ExportEffect(typeof(TintImageEffect), nameof(TintImage))]
namespace CameraCapture.iOS.Effects
{
    public class TintImageEffect : PlatformEffect
    {   
        protected override void OnAttached()
        {
            SetTinColor();
        }

        void SetTinColor()
        {
            try
            {
                var effect = (TintImage)Element.Effects.FirstOrDefault(e => e is TintImage);

                if (effect == null || !(Control is UIImageView image))
                    return;

                image.Image = image.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
                image.TintColor = effect.TintColor.ToUIColor();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"An error occurred when setting the {typeof(TintImageEffect)} effect: {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected override void OnElementPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnElementPropertyChanged(args);

            if(args.PropertyName == Image.SourceProperty.PropertyName)
            {
                SetTinColor();
            }
        }

        protected override void OnDetached()
        {

        }
    }
}
