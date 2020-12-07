using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Plugin.CurrentActivity;
using Android.Support.V4.Content;
using Android;
using Android.Support.V4.App;
using Android.Content;
using Xamarin.Essentials;

namespace CameraCapture.Droid
{
    [Activity(Label = "CameraCapture", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            _ = CreateOrGetStorageAsync();
            CheckPermission();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public bool FirstTime = false;
        async System.Threading.Tasks.Task CreateOrGetStorageAsync ()
        {
            var firstTime = await SecureStorage.GetAsync("firsttime");
            if (firstTime != null)
                FirstTime = bool.Parse(firstTime);

            if (FirstTime == false)
            {
                await SecureStorage.SetAsync("firsttime", "true");
                return;
            }
        }

        public void ShowDialog (string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle("Thông báo");
            dialog.SetMessage(message);
            dialog.SetPositiveButton("Huỷ", (s, a) =>
            {
                dialog.Dispose();
            });
            dialog.SetNegativeButton("Cài đặt", (s, a) =>
            {
                var context = Android.App.Application.Context;
                Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                Android.Net.Uri uri = Android.Net.Uri.FromParts("package", this.PackageName, null);
                i.SetData(uri);
                context.StartActivity(i);
            });
            AlertDialog createDialogCamera = dialog.Create();
            createDialogCamera.Show();
        }

        public void CheckPermission()
        {
            if (int.Parse(Build.VERSION.Sdk.ToString()) >= 23)
            {
                var permCamera = ContextCompat.CheckSelfPermission(this, "android.permission.CAMERA");
                var permStorage = ContextCompat.CheckSelfPermission(this, "android.permission.READ_EXTERNAL_STORAGE");
                string messCamera = "Ứng dụng cần sử dụng máy ảnh. Vui lòng vào cài đặt để cấp quyền.";
                string messStorage = "Ứng dụng cần sử dụng bộ nhớ. Vui lòng vào cài đặt để cấp quyền.";
                if (permCamera != Android.Content.PM.Permission.Granted
                    || permStorage != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.Camera, Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage}, 114);
                    
                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera) == false && permCamera != Android.Content.PM.Permission.Granted && FirstTime != false)
                    {
                        ShowDialog(messCamera);
                    }

                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage) == false && permStorage != Android.Content.PM.Permission.Granted && FirstTime != false)
                    {
                        ShowDialog(messStorage);
                    }
                }
            }
        }
    }
}