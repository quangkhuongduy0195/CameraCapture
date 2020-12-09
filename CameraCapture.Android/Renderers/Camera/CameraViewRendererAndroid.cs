using System;
using System.IO;
using System.Threading.Tasks;
using Android;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Media;
using Android.Provider;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using CameraCapture.Droid.Renderers.Camera;
using CameraCapture.Renderers.Camera;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Plugin.CurrentActivity;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRendererAndroid))]
[assembly: Dependency(typeof(CameraViewRendererAndroid))]
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
            await CheckCameraPermission();
        }

        private void AddCamera()
        {
            _camera = new CameraDroid(Context);
            SetNativeControl(_camera);

            if (Element != null && _camera != null)
            {
                _currentElement = Element;
                _camera.SetCameraOption(_currentElement.CameraOption);
                _camera.Photo += OnPhoto;
                _currentElement._captureAction = TakePicture;
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


                //var stream = new MemoryStream();
                //imgSource.Compress(Bitmap.CompressFormat.Png, 100, stream);
                //imgSource.Recycle();
                //var imageSource = ImageSource.FromStream(() => stream);
                //_currentElement.HandleDidFinishProcessingPhoto(imageSource);
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (_camera != null)
            {
                _camera.Photo -= OnPhoto;
            }
            if (_currentElement != null)
            {
                _currentElement._captureAction = null;
            }
            base.Dispose(disposing);
        }

        public async Task CheckCameraPermission()
        {
            var rationale = ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.Camera);
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            await CreateOrGetCameraAsync();
            if (rationale == true)
                SetfirsttimeCamera();
            switch (status)
            {
                case PermissionStatus.Unknown:
                case PermissionStatus.Denied:
                case PermissionStatus.Disabled:
                    if (rationale == false && FirstTimeCamera)
                    {
                        var rs = await App.Current.MainPage.DisplayAlert("Thông báo", "Ứng dụng cần sử dụng máy ảnh. Vui lòng vào cài đặt để cấp quyền", "Cài đặt", "Huỷ");
                        if (rs)
                        {
                            Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", _context.PackageName, null);
                            i.SetData(uri);
                            _context.StartActivity(i);
                        }
                    }
                    await RequestCameraPermission();
                    break;
                case PermissionStatus.Granted:
                    AddCamera();
                    await CheckStogarePermission();
                    break;
                case PermissionStatus.Restricted:
                    break;
            }
        }

        private async Task RequestCameraPermission()
        {
            await Permissions.RequestAsync<Permissions.Camera>();
            await CheckCameraPermission();
        }

        private async Task CheckStogarePermission()
        {
            var rationale = ActivityCompat.ShouldShowRequestPermissionRationale(CrossCurrentActivity.Current.Activity, Manifest.Permission.ReadExternalStorage);
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            await CreateOrGetStorageAsync();
            if (rationale == true)
                SetfirsttimeStorage();
            switch (status)
            {
                case PermissionStatus.Unknown:
                case PermissionStatus.Denied:
                case PermissionStatus.Disabled:
                    // Check user choose "Don't show again"
                    if (rationale == false && FirstTimeStorage)
                    {
                        var rs = await App.Current.MainPage.DisplayAlert("", "Ứng dụng cần sử dụng bộ nhớ. Vui lòng vào cài đặt để cấp quyền", "Cài đặt", "Close");
                        if (rs)
                        {
                            Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", _context.PackageName, null);
                            i.SetData(uri);
                            _context.StartActivity(i);
                        }
                    }
                    await RequestStogarePermission();
                    break;
                case PermissionStatus.Granted:
                    SetfirsttimeCamera();
                    break;
                case PermissionStatus.Restricted:
                    break;
            }
        }
        private async Task RequestStogarePermission()
        {
            await Permissions.RequestAsync<Permissions.StorageRead>();
            await CheckStogarePermission();
        }

        public bool FirstTimeCamera = false;
        async System.Threading.Tasks.Task CreateOrGetCameraAsync()
        {
            var firstTime = await SecureStorage.GetAsync("firsttimeCamera");
            if (firstTime != null)
                FirstTimeCamera = bool.Parse(firstTime);

        }

        async void SetfirsttimeCamera()
        {
            if (FirstTimeCamera == false)
            {
                await SecureStorage.SetAsync("firsttimeCamera", "true");
                return;
            }
        }

        public bool FirstTimeStorage = false;
        async System.Threading.Tasks.Task CreateOrGetStorageAsync()
        {
            var firstTime = await SecureStorage.GetAsync("firsttimeStorage");
            if (firstTime != null)
                FirstTimeStorage = bool.Parse(firstTime);
        }

        async void SetfirsttimeStorage()
        {
            if (FirstTimeStorage == false)
            {
                await SecureStorage.SetAsync("firsttimeStorage", "true");
                return;
            }

        }
    }

}