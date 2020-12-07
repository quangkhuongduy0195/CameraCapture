using System;
using System.IO;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Provider;
using Android.Util;
using CameraCapture.Droid.Renderers.Camera;
using CameraCapture.Renderers.Camera;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRendererAndroid))]
namespace CameraCapture.Droid.Renderers.Camera
{
    public class CameraViewRendererAndroid : ViewRenderer<CameraView, CameraDroid>
    {
        private CameraDroid _camera;
        private CameraView _currentElement;
        private readonly Context _context;

        public CameraViewRendererAndroid(Context context) : base(context)
        {
            _context = context;
        }

        protected async override void OnElementChanged(ElementChangedEventArgs<CameraView> e)
        {
            base.OnElementChanged(e);
            var permissionCamera = await Permissions.CheckStatusAsync<Permissions.Camera>();
            switch (permissionCamera)
            {
                case PermissionStatus.Unknown:
                case PermissionStatus.Denied:
                case PermissionStatus.Disabled:
                    break;
                case PermissionStatus.Granted:
                    _camera = new CameraDroid(Context);
                    SetNativeControl(_camera);

                    if (e.NewElement != null && _camera != null)
                    {
                        //e.NewElement.CameraClick = new Command(() => TakePicture());
                        _currentElement = e.NewElement;
                        _camera.SetCameraOption(_currentElement.CameraOption);
                        _camera.Photo += OnPhoto;
                        _currentElement._captureAction = TakePicture;
                    }
                    break;
                case PermissionStatus.Restricted:
                    break;
            } 
        }

        public void TakePicture()
        {
            _camera?.LockFocus();
        }

        private void OnPhoto(object sender, byte[] imgSource)
        {
            Device.BeginInvokeOnMainThread(() => {
                var stream = new MemoryStream(imgSource);
                var imageSource = ImageSource.FromStream(() => stream);
                _currentElement.HandleDidFinishProcessingPhoto(imageSource);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if(_camera != null)
            {
                _camera.Photo -= OnPhoto;
            }
            if(_currentElement != null)
            {
                _currentElement._captureAction = null;
            }
            base.Dispose(disposing);
        }
    }

}
