using System;
using Android.Hardware.Camera2;

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
                cameraPreview.mCaptureSession = session;
                try
                {
                    var previewRequestBuilder = cameraPreview.mCameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                    cameraPreview._previewRequestBuilder = previewRequestBuilder;
                    cameraPreview._previewRequestBuilder.AddTarget(cameraPreview.mCameraSurface);
                    //cameraPreview._previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, CameraMetadata.ControlAfSceneChangeDetected);
                    cameraPreview.mCaptureSession.SetRepeatingRequest(cameraPreview._previewRequestBuilder.Build(), null, null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"setting up preview failed: {ex.Message}");
                }
            }

            public override void OnConfigureFailed(CameraCaptureSession session)
            {

            }
        }
    }
}
