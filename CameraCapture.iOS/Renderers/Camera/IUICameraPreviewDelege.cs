using System;
using UIKit;

namespace CameraCapture.iOS.Renderers.Camera
{
    public interface IUICameraPreviewDelege
    {
        void ResultScanQRCode(string code);
        void ImageTake(UIImage image);
    }
}
