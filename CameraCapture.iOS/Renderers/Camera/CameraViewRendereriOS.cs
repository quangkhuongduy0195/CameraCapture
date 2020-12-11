using System;
using System.ComponentModel;
using CameraCapture.iOS.Renderers.Camera;
using CameraCapture.Renderers.Camera;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRendereriOS))]
namespace CameraCapture.iOS.Renderers.Camera
{
    public class CameraViewRendereriOS : ViewRenderer<CameraView, UICameraPreview>, IUICameraPreviewDelege //IUICameraPreviewDelege
    {

        UICameraPreview _uiCameraPreview;

        protected override void OnElementChanged(ElementChangedEventArgs<CameraView> e)
        {
            base.OnElementChanged(e);
            if (Control == null)
            {
                _uiCameraPreview = new UICameraPreview(e.NewElement.CameraOption);
                _uiCameraPreview.delege = this;
                SetNativeControl(_uiCameraPreview);
            }
            if (e.OldElement != null)
            {
                // Unsubscribe
                Element._captureAction = null;
            }
            if (e.NewElement != null)
            {
                // Subscribe
                Element._captureAction = CaptureAction;
            }

        }

        private void CaptureAction()
        {
            _uiCameraPreview.HandleAction();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == CameraView.CameraOptionProperty.PropertyName)
                _uiCameraPreview?.ChangeCameraOption(Element.CameraOption);
        }

        private void OnCameraPreviewTapped(object sender, EventArgs e)
        {
            _uiCameraPreview.HandleAction();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Control._captureSession.Dispose();
                Control.Dispose();
            }
            base.Dispose(disposing);
        }

        void IUICameraPreviewDelege.ResultScanQRCode(string code)
        {

        }

        void IUICameraPreviewDelege.ImageTake(UIImage image)
        {
            var newImage = FixOrientation(image);
            byte[] imageByte;
            using (NSData imageData = newImage.AsPNG())
            {
                Byte[] myByteArray = new Byte[imageData.Length];
                System.Runtime.InteropServices.Marshal.Copy(imageData.Bytes, myByteArray, 0, Convert.ToInt32(imageData.Length));
                imageByte = myByteArray;
            }
            string fileId = "img" + DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssTZD");
            var img = Xamarin.Forms.ImageSource.FromStream(() => newImage.AsPNG().AsStream());
            Element.HandleDidFinishProcessingPhoto(img, imageByte, fileId);
        }

        UIImage FixOrientation(UIImage image)
        {
            if (image.Orientation == UIImageOrientation.Up)
            {
                return image;
            }

            UIGraphics.BeginImageContextWithOptions(image.Size, false, image.CurrentScale);
            image.Draw(new CoreGraphics.CGRect(0, 0, image.Size.Width, image.Size.Height));
            var normalizedImage = UIGraphics.GetImageFromCurrentImageContext();
            if (normalizedImage != null)
            {
                UIGraphics.EndImageContext();
                return normalizedImage;
            }
            return image;
        }

    }
}
