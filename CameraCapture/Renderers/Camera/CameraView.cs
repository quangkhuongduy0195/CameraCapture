using System;
using Xamarin.Forms;

namespace CameraCapture.Renderers.Camera
{
    public enum CameraOptions
    {
        Rear,
        Front
    }

    public class CameraView : View
    {
        public static readonly BindableProperty CameraOptionProperty = BindableProperty.Create(propertyName: nameof(CameraOption),
                                                                                         returnType: typeof(CameraOptions),
                                                                                         declaringType: typeof(CameraView),
                                                                                         defaultValue: CameraOptions.Rear);

        public CameraOptions CameraOption
        {
            get { return (CameraOptions)GetValue(CameraOptionProperty); }
            set { SetValue(CameraOptionProperty, value); }
        }

        public Action _captureAction;
        public void Capture()
        {
            _captureAction?.Invoke();
        }

        public event EventHandler<ImageSource> FinishProcessingPhoto;
        public void HandleDidFinishProcessingPhoto(ImageSource image)
        {
            FinishProcessingPhoto?.Invoke(this, image);
        }
    }
}
