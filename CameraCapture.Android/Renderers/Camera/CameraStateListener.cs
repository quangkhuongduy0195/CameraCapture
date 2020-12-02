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

            }

            public override void OnError(CameraDevice camera, [GeneratedEnum] Android.Hardware.Camera2.CameraError error)
            {

            }

            public override void OnOpened(CameraDevice camera)
            {
                cameraPreview.mCameraDevice = camera;
                cameraPreview.mHandler.SendEmptyMessage(1);
            }
        }
    }
}
