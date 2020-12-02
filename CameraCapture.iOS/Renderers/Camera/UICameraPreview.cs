using System;
using System.Collections.Generic;
using System.Linq;
using AVFoundation;
using CameraCapture.Renderers.Camera;
using CoreFoundation;
using CoreGraphics;
using CoreVideo;
using Foundation;
using UIKit;

namespace CameraCapture.iOS.Renderers.Camera
{
    public class UICameraPreview : UIView, IAVCaptureMetadataOutputObjectsDelegate, IAVCapturePhotoCaptureDelegate
    {
        AVCaptureVideoPreviewLayer _cameraLayer;
        CameraOptions cameraOptions;
        AVCapturePhotoOutput _photoOutput = new AVCapturePhotoOutput();
        public IUICameraPreviewDelege delege;
        public AVCaptureSession _captureSession { get; private set; }

        public UICameraPreview(CameraOptions options)
        {
            cameraOptions = options;
            Initialize();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            _cameraLayer.Frame = rect;
        }

        public void ChangeCameraOption(CameraOptions options)
        {
            cameraOptions = options;
            Initialize();
        }

        void Initialize()
        {
            OpenCamera();
        }

        void OpenCamera()
        {
            switch (AVCaptureDevice.GetAuthorizationStatus(AVAuthorizationMediaType.Video))
            {
                case AVAuthorizationStatus.Authorized:
                    SettupCaptureSession();
                    break;
                case AVAuthorizationStatus.NotDetermined:
                    break;
                case AVAuthorizationStatus.Denied:
                    break;
                case AVAuthorizationStatus.Restricted:
                    break;
                default:
                    break;
            }
        }

        void SettupCaptureSession()
        {
            _captureSession = new AVCaptureSession();
            var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            var captureDevice = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaTypes.Video, cameraPosition);
            if (captureDevice != null)
            {
                var input = new AVCaptureDeviceInput(captureDevice, out var error);
                if (error == null)
                {
                    if (_captureSession.CanAddInput(input))
                    {
                        _captureSession.AddInput(input);
                    }
                }
                if (_captureSession.CanAddOutput(_photoOutput))
                {
                    _captureSession.AddOutput(_photoOutput);
                }
                _cameraLayer = new AVCaptureVideoPreviewLayer(_captureSession);
                _cameraLayer.Frame = this.Bounds;
                _cameraLayer.VideoGravity = AVLayerVideoGravity.ResizeAspectFill;
                this.Layer.AddSublayer(_cameraLayer);

                //Turn on flash
                if (captureDevice.HasTorch)
                {
                    captureDevice.LockForConfiguration(out var err);
                    if (err == null)
                    {
                        if (captureDevice.TorchMode == AVCaptureTorchMode.Off)
                        {
                            captureDevice.TorchMode = AVCaptureTorchMode.On;
                            captureDevice.FlashMode = AVCaptureFlashMode.On;
                        }
                        captureDevice.SetTorchModeLevel(1.0f, out var _);
                        captureDevice.UnlockForConfiguration();
                    }
                }
                _captureSession.StartRunning();
            }
        }

        public void HandleAction()
        {
            var photoSettings = AVCapturePhotoSettings.Create();
            var photoPreviewType = photoSettings.AvailablePreviewPhotoPixelFormatTypes.First();
            if(photoPreviewType != null)
            {
                photoSettings.PreviewPhotoFormat = new NSDictionary<NSString, NSObject>(CVPixelBuffer.PixelFormatTypeKey, photoPreviewType);
                _photoOutput.CapturePhoto(photoSettings, this);
            }

        }

        [Export("captureOutput:didFinishProcessingPhoto:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError error)
        {
            var imageData = photo.FileDataRepresentation;
            if(imageData != null)
            {
                var previewImage = new UIImage(imageData);
                previewImage.SaveToPhotosAlbum((img, err) =>
                {
                    if (err == null)
                        App.Current.MainPage.DisplayAlert("", "Saved image to photos", "OK");
                    else
                        App.Current.MainPage.DisplayAlert("", "Save image to photos Fail", "OK");
                });
                delege?.ImageTake(previewImage);
            }
        }
    }
}
