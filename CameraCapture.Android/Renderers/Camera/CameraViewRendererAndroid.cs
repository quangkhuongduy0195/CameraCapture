﻿using System;
using System.ComponentModel;
using Android.Content;
using CameraCapture.Droid.Renderers.Camera;
using CameraCapture.Renderers.Camera;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRendererAndroid))]
namespace CameraCapture.Droid.Renderers.Camera
{
    public class CameraViewRendererAndroid : Xamarin.Forms.Platform.Android.AppCompat.ViewRenderer<CameraView, CameraPreview>, ICameraPreviewDelege
    {
        CameraPreview cameraPreview;
        Context context;

        public CameraViewRendererAndroid(Context context) : base(context)
        {
            this.context = context;
        }

        protected override async void OnElementChanged(ElementChangedEventArgs<CameraView> e)
        {
            base.OnElementChanged(e);
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            switch (status)
            {
                case PermissionStatus.Unknown:
                case PermissionStatus.Denied:
                case PermissionStatus.Disabled:
                    break;
                case PermissionStatus.Granted:
                    if (Control == null)
                    {
                        cameraPreview = new CameraPreview(Context, Element.CameraOption);
                        cameraPreview.iCameraPreview = this;
                        SetNativeControl(cameraPreview);
                    }

                    if (e.OldElement != null)
                    {
                        // Unsubscribe
                        cameraPreview.Click -= OnCameraPreviewClicked;
                    }
                    if (e.NewElement != null)
                    {
                        // Subscribe
                        cameraPreview.Click += OnCameraPreviewClicked;
                    }
                    break;
                case PermissionStatus.Restricted:
                    break;
            }
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            //if (e.PropertyName == CameraView.CameraOptionProperty.PropertyName)
                //cameraPreview?.ChangeCameraOption(Element.CameraOption);
        }

        void OnCameraPreviewClicked(object sender, EventArgs e)
        {
            //if (cameraPreview.IsPreviewing)
            //{
                //cameraPreview.Preview.StopPreview();
                //cameraPreview.StopCamera();
                //cameraPreview.IsPreviewing = false;
            //}
            //else
            //{
                //cameraPreview.Preview.StartPreview();
                //cameraPreview.StartCamera();
                //cameraPreview.IsPreviewing = true;
            //}
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Control.Preview.Release();
                //Control.ReleaseCamera();
            }
            base.Dispose(disposing);
        }


        public void resultQRCode(string code)
        {

        }
    }
}