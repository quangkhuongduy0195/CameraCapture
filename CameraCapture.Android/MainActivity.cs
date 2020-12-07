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
        public string FirstTime;
        async System.Threading.Tasks.Task CreateOrGetStorageAsync ()
        {
            FirstTime = await SecureStorage.GetAsync("firsttime");
            if (FirstTime == null)
            {
                await SecureStorage.SetAsync("firsttime", "true");
                return;
            }
        }

        void CheckPermission()
        {
            if (int.Parse(Build.VERSION.Sdk.ToString()) >= 23)
            {
                var permCamera = ContextCompat.CheckSelfPermission(this, "android.permission.CAMERA");
                var permStorage = ContextCompat.CheckSelfPermission(this, "android.permission.READ_EXTERNAL_STORAGE");
                if (permCamera != Android.Content.PM.Permission.Granted
                    || permStorage != Android.Content.PM.Permission.Granted)
                {
                    ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.Camera, Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage}, 114);
                    
                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera) == false && permCamera != Android.Content.PM.Permission.Granted && FirstTime != null)
                    {
                        AlertDialog.Builder dialogCamera = new AlertDialog.Builder(this);
                        dialogCamera.SetTitle("Thông báo");
                        dialogCamera.SetMessage("Ứng dụng cần sử dụng máy ảnh. Vui lòng vào cài đặt để cấp quyền.");
                        dialogCamera.SetPositiveButton("Huỷ", (s, a) =>
                        {
                            dialogCamera.Dispose();
                        });
                        dialogCamera.SetNegativeButton("Cài đặt", (s, a) =>
                        {
                            var context = Android.App.Application.Context;
                            Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", this.PackageName, null);
                            i.SetData(uri);
                            context.StartActivity(i);
                        });
                        AlertDialog createDialogCamera = dialogCamera.Create();
                        createDialogCamera.Show();

                    }

                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage) == false && permStorage != Android.Content.PM.Permission.Granted && FirstTime != null)
                    {
                        AlertDialog.Builder dialogStorage = new AlertDialog.Builder(this);
                        dialogStorage.SetTitle("Thông báo");
                        dialogStorage.SetMessage("Ứng dụng cần sử dụng bộ nhớ. Vui lòng vào cài đặt để cấp quyền.");
                        dialogStorage.SetPositiveButton("Huỷ", (s, a) =>
                        {
                            dialogStorage.Dispose();
                        });
                        dialogStorage.SetNegativeButton("Cài đặt", (s, a) =>
                        {
                            var context = Android.App.Application.Context;
                            Intent i = new Android.Content.Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
                            Android.Net.Uri uri = Android.Net.Uri.FromParts("package", this.PackageName, null);
                            i.SetData(uri);
                            context.StartActivity(i);
                        });
                        AlertDialog createDialogStorage = dialogStorage.Create();
                        createDialogStorage.Show();

                    }
                }
            }
        }
    }
}