using System;
using Android.Hardware.Camera2;
using Android.Runtime;

namespace CameraCapture.Droid.Renderers.Camera
{
    public partial class CameraPreview
    {
        public class CameraStateListener : CameraDevice.StateCallback
        {
            CameraPreview cameraPreview;
            public CameraStateListener(CameraPreview cameraPreview)
            {
                this.cameraPreview = cameraPreview;
            }

            public override void OnDisconnected(CameraDevice camera)
            {
                cameraPreview._cameraDevice.Close();
            }

            public override void OnError(CameraDevice camera, [GeneratedEnum] Android.Hardware.Camera2.CameraError error)
            {
                cameraPreview._cameraDevice.Close();
                cameraPreview._cameraDevice = null;
            }

            public override void OnOpened(CameraDevice camera)
            {
                cameraPreview._cameraDevice = camera;
                cameraPreview.createCameraPreview();
                //cameraPreview.mHandler.SendEmptyMessage(1);
            }
        }
    }
}
