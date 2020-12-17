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
        AVCapturePhotoOutput _photoOutput;
        public IUICameraPreviewDelege delege;
        public AVCaptureSession _captureSession { get; private set; }
        public AVCaptureDevice _currentCaptureDevice { get; set; }
        AVCapturePhotoSettings setFlash { get; set; }
        public UICameraPreview(CameraOptions options)
        {
            cameraOptions = options;
            Initialize();
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            if (_cameraLayer != null)
                _cameraLayer.Frame = rect;
        }

        public void ChangeCameraOption(CameraOptions options)
        {
            cameraOptions = options;
            Initialize();
        }
        AVCaptureFlashMode FlashMode;
        public void ChangeFlashOption(FlashOptions options)
        {
           
            AVCaptureDevice backCamera = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
            backCamera.LockForConfiguration(out var err);

            switch (options)
            {
                case FlashOptions.On:
                    FlashMode = AVCaptureFlashMode.On;
                    backCamera.UnlockForConfiguration();
                    break;
                case FlashOptions.Off:
                    FlashMode = AVCaptureFlashMode.Off;
                    backCamera.UnlockForConfiguration();
                    break;
                case FlashOptions.Auto:
                    backCamera.TorchMode = AVCaptureTorchMode.Auto;
                    backCamera.FlashMode = AVCaptureFlashMode.Auto;
                    FlashMode = AVCaptureFlashMode.Auto;
                    backCamera.UnlockForConfiguration();
                    break;
            }
        }

        public void Initialize()
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

        AVCaptureDevice getFrontCamera()
        {
            var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            foreach (var dv in videoDevices)
            {
                AVCaptureDevice device = dv as AVCaptureDevice;
                if (device.Position == AVCaptureDevicePosition.Front)
                {
                    return device;
                }
            }
            return null;
        }

        AVCaptureDevice getBackCamera()
        {
            var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            var captureDevice = AVCaptureDevice.GetDefaultDevice(AVCaptureDeviceType.BuiltInWideAngleCamera, AVMediaTypes.Video, cameraPosition);
            return captureDevice;
        }

        void SettupCaptureSession()
        {
            _photoOutput = new AVCapturePhotoOutput();
            _captureSession = new AVCaptureSession();

            var captureDevice = (cameraOptions == CameraOptions.Front) ? getFrontCamera() : getBackCamera();

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
                            captureDevice.FlashMode = AVCaptureFlashMode.On;
                            captureDevice.TorchMode = AVCaptureTorchMode.On;
                        }
                        captureDevice.SetTorchModeLevel(1.0f, out var _);
                        captureDevice.UnlockForConfiguration();
                    }
                }
                _captureSession.StartRunning();
            }
        }

        AVCapturePhotoSettings GetSetting(AVCaptureDevice camera, AVCaptureFlashMode flashMode)
        {
            var settings = AVCapturePhotoSettings.Create();
            if(camera.HasFlash)
            {
                settings.FlashMode = flashMode;
            }
            return settings;
        }

        public void HandleAction()
        {
            var photoSettings = AVCapturePhotoSettings.Create();
            var photoPreviewType = photoSettings.AvailablePreviewPhotoPixelFormatTypes.First();
            if (photoPreviewType != null)
            {
                AVCaptureDevice backCamera = AVCaptureDevice.GetDefaultDevice(AVMediaTypes.Video);
                photoSettings = GetSetting(backCamera, FlashMode);
                photoSettings.PreviewPhotoFormat = new NSDictionary<NSString, NSObject>(CVPixelBuffer.PixelFormatTypeKey, photoPreviewType);
                _photoOutput.CapturePhoto(photoSettings, this);
            }

        }

        [Export("captureOutput:didFinishProcessingPhoto:error:")]
        public void DidFinishProcessingPhoto(AVCapturePhotoOutput output, AVCapturePhoto photo, NSError error)
        {
            var imageData = photo.FileDataRepresentation;
            if (imageData != null)
            {
                var previewImage = new UIImage(imageData);
                previewImage.SaveToPhotosAlbum((img, err) =>
                {
                    //if (err == null)
                    //    App.Current.MainPage.DisplayAlert("", "Saved image to photos", "OK");
                    //else
                    //    App.Current.MainPage.DisplayAlert("", "Save image to photos Fail", "OK");
                });
                delege?.ImageTake(previewImage);
            }
        }
    }
}
