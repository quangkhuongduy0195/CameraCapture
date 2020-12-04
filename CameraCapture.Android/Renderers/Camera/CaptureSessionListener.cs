using System;
using Android.Hardware.Camera2;
using Android.Util;
using Java.Lang;

namespace CameraCapture.Droid.Renderers.Camera
{
    public partial class CameraPreview
    {
        public class CaptureSessionListener : CameraCaptureSession.StateCallback
        {
            CameraPreview cameraPreview;

            public CaptureSessionListener(CameraPreview cameraPreview)
            {
                this.cameraPreview = cameraPreview;
            }

            public override void OnConfigured(CameraCaptureSession session)
            {

                if (cameraPreview._cameraDevice == null) return;

                cameraPreview._captureSession = session;
                cameraPreview.UpdatePreview();

                //cameraPreview.mCaptureSession = session;
                //try
                //{
                //    var previewRequestBuilder = cameraPreview._cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                //    cameraPreview._previewRequestBuilder = previewRequestBuilder;
                //    cameraPreview._previewRequestBuilder.AddTarget(cameraPreview.mCameraSurface);
                //    //cameraPreview._previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, CameraMetadata.ControlAfSceneChangeDetected);
                //    cameraPreview.mCaptureSession.SetRepeatingRequest(cameraPreview._previewRequestBuilder.Build(), null, null);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"setting up preview failed: {ex.Message}");
                //}
            }

            public override void OnConfigureFailed(CameraCaptureSession session)
            {

            }
        }
    }

    public class CompareSizesByArea : Java.Lang.Object, Java.Util.IComparator
    {
        public int Compare(Java.Lang.Object lhs, Java.Lang.Object rhs)
        {
            var lhsSize = (Size)lhs;
            var rhsSize = (Size)rhs;
            // We cast here to ensure the multiplications won't overflow
            return Long.Signum((long)lhsSize.Width * lhsSize.Height - (long)rhsSize.Width * rhsSize.Height);
        }
    }
}
