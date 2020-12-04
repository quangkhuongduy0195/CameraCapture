using System;
using System.Collections.Generic;
using System.Linq;
using Android;
using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using CameraCapture.Renderers.Camera;
using Java.IO;
using Java.Util;
using Plugin.CurrentActivity;
using static Android.Views.TextureView;

namespace CameraCapture.Droid.Renderers.Camera
{
    public interface ICameraPreviewDelege
    {
        void resultQRCode(string code);
    }

    public partial class CameraPreview : ViewGroup, ISurfaceTextureListener//, View.IOnTouchListener //IProcessor
    {
        private Handler mHandler;
        CameraOptions cameraOption;
        AutoFitTextureView _textureView;
        Context context;
        CameraDevice _cameraDevice;
        string _cameraId;
        private Size? imageDimension;
        CameraStateListener _stateCallback;
        CaptureRequest.Builder captureRequestBuilder;
        CaptureSessionListener _captureSessionListener;
        CameraCaptureSession _captureSession;
        private Handler mBackgroundHandler;
        private HandlerThread mBackgroundThread;
        int _width = 0;

        public ICameraPreviewDelege iCameraPreview;

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);
            _textureView.Measure(msw, msh);
            _textureView.Layout(0, 0, r - l, b - t);
            _width = r;
        }

        public CameraPreview(Context context, CameraOptions cameraOption)
            : base(context)
        {
            this.cameraOption = cameraOption;
            this.context = context;
            SettupCaptureSession();
        }


        void OpenCamera()
        {
            CameraManager cameraManager = (CameraManager)context.GetSystemService(Context.CameraService);
            try
            {
                var windowManager = context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
                var displayRotation = windowManager.DefaultDisplay.Rotation;
                _cameraId = cameraManager.GetCameraIdList()[0];
                CameraCharacteristics cameraCharacteristics = cameraManager.GetCameraCharacteristics(_cameraId);
                var sensorOrientation = (int)cameraCharacteristics.Get(CameraCharacteristics.SensorOrientation);
                bool swappedDimensions = false;
                switch (displayRotation)
                {
                    case SurfaceOrientation.Rotation0:
                    case SurfaceOrientation.Rotation180:
                        if (sensorOrientation == 90 || sensorOrientation == 270)
                        {
                            swappedDimensions = true;
                        }
                        break;
                    case SurfaceOrientation.Rotation90:
                    case SurfaceOrientation.Rotation270:
                        if (sensorOrientation == 0 || sensorOrientation == 180)
                        {
                            swappedDimensions = true;
                        }
                        break;
                    default:
                        System.Diagnostics.Debug.WriteLine($"Display rotation is invalid: {displayRotation}");
                        break;
                }
                Point displaySize = new Point();
                windowManager.DefaultDisplay.GetSize(displaySize);
                var rotatedPreviewWidth = _textureView.Width;
                var rotatedPreviewHeight = _textureView.Height;
                var maxPreviewWidth = displaySize.X;
                var maxPreviewHeight = displaySize.Y;

                if (swappedDimensions)
                {
                    rotatedPreviewWidth = _textureView.Height;
                    rotatedPreviewHeight = _textureView.Width;
                    maxPreviewWidth = displaySize.Y;
                    maxPreviewHeight = displaySize.X;
                }

                StreamConfigurationMap map = (StreamConfigurationMap)cameraCharacteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);
                var sizes = map.GetOutputSizes(Java.Lang.Class.FromType(typeof(SurfaceTexture)));//(int)ImageFormatType.Jpeg);
                var largest = (Size)Java.Util.Collections.Max(Arrays.AsList(sizes), new CompareSizesByArea());
                //imageDimension = sizes.FirstOrDefault(x => x.Width / x.Height == 4 / 3 && x.Height == _width);
                //imageDimension = sizes[5];
                //https://forums.xamarin.com/discussion/158202/how-to-set-the-orientation-of-camera-in-android-app-using-the-camera2-api
                imageDimension = ChooseOptimalSize(sizes, rotatedPreviewWidth, rotatedPreviewHeight, maxPreviewWidth, maxPreviewHeight, largest);

                if (ActivityCompat.CheckSelfPermission(context, Manifest.Permission.Camera) != Android.Content.PM.Permission.Granted
                    && ActivityCompat.CheckSelfPermission(context, Manifest.Permission.WriteExternalStorage) != Android.Content.PM.Permission.Granted)
                {
                    return;
                }
                _stateCallback = new CameraStateListener(this);
                cameraManager.OpenCamera(_cameraId, _stateCallback, null);
            }
            catch (Exception ex)
            {

            }
        }

        void SettupCaptureSession()
        {
            _textureView = new AutoFitTextureView(context);
            _textureView.SurfaceTextureListener = this;
            AddView(_textureView);

        }

        protected void createCameraPreview()
        {
            try
            {
                SurfaceTexture texture = _textureView.SurfaceTexture;
                texture.SetDefaultBufferSize(imageDimension.Width, imageDimension.Height);
                Surface surface = new Surface(texture);
                captureRequestBuilder = _cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                captureRequestBuilder.AddTarget(surface);
                var surfaces = new List<Surface>();
                surfaces.Add(surface);
                _captureSessionListener = new CaptureSessionListener(this);
                _cameraDevice.CreateCaptureSession(surfaces, _captureSessionListener, null);
            }
            catch (Exception ex)
            {

            }
        }

        protected void UpdatePreview()
        {
            if (_cameraDevice == null) return;
            captureRequestBuilder.Set(CaptureRequest.ControlMode, (int)ControlAEMode.OnAutoFlash);
            try
            {
                _captureSession.SetRepeatingRequest(captureRequestBuilder.Build(), null, mBackgroundHandler);
            }
            catch (Exception ex)
            {

            }
        }

        protected void StartBackgroundThread()
        {
            mBackgroundThread = new HandlerThread("Camera Background");
            mBackgroundThread.Start();
            mBackgroundHandler = new Handler(mBackgroundThread.Looper);
        }

        protected void StopBackgroundThread()
        {
            mBackgroundThread.QuitSafely();
            try
            {
                mBackgroundThread.Join();
                mBackgroundThread = null;
                mBackgroundHandler = null;
            }
            catch (Exception e)
            {

            }
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            OpenCamera();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            return false;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {

        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {

        }

        private static Size ChooseOptimalSize(Size[] choices, int textureViewWidth,
            int textureViewHeight, int maxWidth, int maxHeight, Size aspectRatio)
        {
            // Collect the supported resolutions that are at least as big as the preview Surface
            var bigEnough = new List<Size>();
            // Collect the supported resolutions that are smaller than the preview Surface
            var notBigEnough = new List<Size>();
            int w = aspectRatio.Width;
            int h = aspectRatio.Height;

            for (var i = 0; i < choices.Length; i++)
            {
                Size option = choices[i];
                if (option.Height == option.Width * h / w)
                {
                    if (option.Width >= textureViewWidth &&
                        option.Height >= textureViewHeight)
                    {
                        bigEnough.Add(option);
                    }
                    else if ((option.Width <= maxWidth) && (option.Height <= maxHeight))
                    {
                        notBigEnough.Add(option);
                    }
                }
            }

            // Pick the smallest of those big enough. If there is no one big enough, pick the
            // largest of those not big enough.
            if (bigEnough.Count > 0)
            {
                return (Size)Collections.Min(bigEnough, new CompareSizesByArea());
            }
            else if (notBigEnough.Count > 0)
            {
                return (Size)Collections.Max(notBigEnough, new CompareSizesByArea());
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Couldn't find any suitable preview size");
                return choices[0];
            }
        }
    }
}
