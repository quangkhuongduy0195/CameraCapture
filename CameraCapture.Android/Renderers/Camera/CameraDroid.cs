using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using CameraCapture.Renderers.Camera;
//using Camera2Forms.CustomViews;
//using Camera2Forms.Droid;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Size = Android.Util.Size;

namespace CameraCapture.Droid.Renderers.Camera
{
    public class CameraDroid : FrameLayout, TextureView.ISurfaceTextureListener
    {
        #region Camera States

        // Camera state: Showing camera preview.
        public const int STATE_PREVIEW = 0;

        // Camera state: Waiting for the focus to be locked.
        public const int STATE_WAITING_LOCK = 1;

        // Camera state: Waiting for the exposure to be precapture state.
        public const int STATE_WAITING_PRECAPTURE = 2;

        //Camera state: Waiting for the exposure state to be something other than precapture.
        public const int STATE_WAITING_NON_PRECAPTURE = 3;

        // Camera state: Picture was taken.
        public const int STATE_PICTURE_TAKEN = 4;

        #endregion

        // The current state of camera state for taking pictures.
        public int mState = STATE_PREVIEW;

        private int sensorOrientation;

        private static readonly SparseIntArray _orientations = new SparseIntArray();

        public event EventHandler<byte[]> Photo;

        public bool OpeningCamera { private get; set; }

        public CameraDevice _cameraDevice;

        private CameraStateListener _cameraStateListener;
        private CameraCaptureListener _cameraCaptureListener;

        private CaptureRequest.Builder _previewBuilder;
        private CaptureRequest.Builder _captureBuilder;
        private CaptureRequest _previewRequest;
        private CameraCaptureSession _previewSession;
        private CameraCaptureSession _previewSessionInit;
        private SurfaceTexture _viewSurface;
        private TextureView _cameraTexture;
        private Size _previewSize;
        private readonly Context _context;
        private CameraManager _manager;

        private bool _flashSupported;
        private Size[] _supportedJpegSizes;
        private Size _idealPhotoSize = new Size(480, 640);

        private HandlerThread _backgroundThread;
        private Handler _backgroundHandler;

        private ImageReader _imageReader;
        private string _cameraId = null;
        private LensFacing _lensFacing;

        public void SetCameraOption(CameraOptions cameraOptions)
        {
            this._lensFacing = (cameraOptions == CameraOptions.Front) ? LensFacing.Front : LensFacing.Back;

        }

        public void SetSwitchCamera(CameraOptions cameraOptions)
        {
            SetCameraOption(cameraOptions);
            CloseCamera();
            InitPreview();
        }

        public void CloseCamera()
        {
            _previewSession.Close();
            _previewSession = null;
            _cameraDevice?.Close();
            _cameraDevice = null;
            _imageReader.Close();
            _imageReader = null;
            _captureBuilder = null;
        }


        public CameraDroid(Context context) : base(context)
        {
            _context = context;

            var inflater = LayoutInflater.FromContext(context);

            if (inflater == null) return;

            InitPreview();

            _orientations.Append((int)SurfaceOrientation.Rotation0, 0);
            _orientations.Append((int)SurfaceOrientation.Rotation90, 90);
            _orientations.Append((int)SurfaceOrientation.Rotation180, 180);
            _orientations.Append((int)SurfaceOrientation.Rotation270, 270);
        }

        void InitPreview()
        {
            _cameraTexture = new TextureView(_context);

            _cameraTexture.SurfaceTextureListener = this;

            _cameraStateListener = new CameraStateListener { Camera = this };

            _cameraCaptureListener = new CameraCaptureListener(this);

            AddView(_cameraTexture);
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            var msw = MeasureSpec.MakeMeasureSpec(right - left, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(bottom - top, MeasureSpecMode.Exactly);
            _cameraTexture.Measure(msw, msh);
            _cameraTexture.Layout(0, 0, right - left, bottom - top);
        }

        int _width = 0;
        int _height = 0;
        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _width = width;
            _height = height;
            _viewSurface = surface;
            ConfigureTransform(width, height);
            StartBackgroundThread();

            OpenCamera(width, height);
        }

        private void ConfigureTransform(int viewWidth, int viewHeight)
        {
            if (_viewSurface == null || _previewSize == null || _context == null) return;

            var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();

            var rotation = windowManager.DefaultDisplay.Rotation;
            var matrix = new Matrix();
            var viewRect = new RectF(0, 0, viewWidth, viewHeight);
            var bufferRect = new RectF(0, 0, _previewSize.Width, _previewSize.Height);

            var centerX = viewRect.CenterX();
            var centerY = viewRect.CenterY();

            if (rotation == SurfaceOrientation.Rotation90 || rotation == SurfaceOrientation.Rotation270)
            {
                bufferRect.Offset(centerX - bufferRect.CenterX(), centerY - bufferRect.CenterY());
                matrix.SetRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.Fill);

                matrix.PostRotate(90 * ((int)rotation - 2), centerX, centerY);
            }

            _cameraTexture.SetTransform(matrix);
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            StopBackgroundThread();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {

        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {

        }

        private void SetUpCameraOutputs(int width, int height)
        {
            _manager = (CameraManager)_context.GetSystemService(Context.CameraService);
            string[] cameraIds = _manager.GetCameraIdList();
            if (_cameraId == null)
            {
                _cameraId = cameraIds[0];
            }

            for (int i = 0; i < cameraIds.Length; i++)
            {
                CameraCharacteristics chararc = _manager.GetCameraCharacteristics(cameraIds[i]);
                sensorOrientation = (int)chararc.Get(CameraCharacteristics.SensorOrientation);

                var facing = (Integer)chararc.Get(CameraCharacteristics.LensFacing);
                if (facing != null && facing == (Integer.ValueOf((int)_lensFacing)))
                {
                    _cameraId = cameraIds[i];

                    //Phones like Galaxy S10 have 2 or 3 frontal cameras usually the one with flash is the one
                    //that should be chosen, if not It will select the first one and that can be the fish
                    //eye camera
                    if (HasFLash(chararc))
                        break;
                }
            }

            var characteristics = _manager.GetCameraCharacteristics(_cameraId);
            var map = (StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap);

            if (_supportedJpegSizes == null && characteristics != null)
            {
                _supportedJpegSizes = ((StreamConfigurationMap)characteristics.Get(CameraCharacteristics.ScalerStreamConfigurationMap)).GetOutputSizes((int)ImageFormatType.Jpeg);
            }

            if (_supportedJpegSizes != null && _supportedJpegSizes.Length > 0)
            {
                _idealPhotoSize = GetOptimalSize(_supportedJpegSizes, 1050, 1400); //MAGIC NUMBER WHICH HAS PROVEN TO BE THE BEST
            }

            _imageReader = ImageReader.NewInstance(_idealPhotoSize.Width, _idealPhotoSize.Height, ImageFormatType.Jpeg, 1);

            var readerListener = new ImageAvailableListener();

            readerListener.Photo += (sender, buffer) =>
            {
                Photo?.Invoke(this, buffer);
            };

            _flashSupported = HasFLash(characteristics);

            _imageReader.SetOnImageAvailableListener(readerListener, _backgroundHandler);

            _previewSize = GetOptimalSize(map.GetOutputSizes(Class.FromType(typeof(SurfaceTexture))), width, height);
        }

        int GetOrientation(int rotation) => (_orientations.Get(rotation)) % 360;

        private bool HasFLash(CameraCharacteristics characteristics)
        {
            var available = (Java.Lang.Boolean)characteristics.Get(CameraCharacteristics.FlashInfoAvailable);
            if (available == null)
            {
                return false;
            }
            else
            {
                return (bool)available;
            }
        }

        public void OpenCamera(int width, int height)
        {
            if (_context == null || OpeningCamera)
            {
                return;
            }

            OpeningCamera = true;

            SetUpCameraOutputs(width, height);
            _manager.OpenCamera(_cameraId, _cameraStateListener, null);
        }

        public int GetJpegOrientation(CameraCharacteristics c, int deviceOrientation)
        {
            if (deviceOrientation == Android.Views.OrientationEventListener.OrientationUnknown)
                return 0;
            int sensorOrientation = (int)c.Get(CameraCharacteristics.SensorOrientation);
            deviceOrientation = (deviceOrientation + 45) / 90 * 90;

            var facing = (Integer)c.Get(CameraCharacteristics.LensFacing);
            bool facingFront = facing == (Integer.ValueOf((int)_lensFacing));
            if (facingFront) deviceOrientation = -deviceOrientation;
            int jpegOrientation = (sensorOrientation + deviceOrientation + 360) % 360;
            return jpegOrientation;
        }
        public int Orientation=0;
        private static SparseIntArray Orientations = new SparseIntArray();
        private static sbyte JPEG_QUALITY = 0;
        public void TakePhoto()
        {
            if (_context == null || _cameraDevice == null) return;

            if (_captureBuilder == null)
                _captureBuilder = _cameraDevice.CreateCaptureRequest(CameraTemplate.StillCapture);

            var windowManager = _context.GetSystemService(Context.WindowService).JavaCast<IWindowManager>();
            int rotation = (int)windowManager.DefaultDisplay.Rotation;
            //int orientation = GetOrientation(rotation);
            _captureBuilder.Set(CaptureRequest.ControlMode, new Integer((int)ControlMode.Auto));
            var charActer = _manager.GetCameraCharacteristics(_cameraId);
            Orientation = GetJpegOrientation(charActer, rotation);
            _captureBuilder.Set(CaptureRequest.JpegOrientation, new Integer(Orientations.Get(rotation)));
            
            _captureBuilder.AddTarget(_imageReader.Surface);

            _captureBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
            SetAutoFlash(_captureBuilder);
            _previewSession.StopRepeating();
            _previewSession.AbortCaptures();
            _previewSession.Capture(_captureBuilder.Build(),
                new CameraCaptureStillPictureSessionCallback
                {
                    OnCaptureCompletedAction = session =>
                    {
                        UnlockFocus();
                    }
                }, null);
        }
        

        public void StartPreview()
        {
            if (_cameraDevice == null || !_cameraTexture.IsAvailable || _previewSize == null) return;

            var texture = _cameraTexture.SurfaceTexture;

            texture.SetDefaultBufferSize(_previewSize.Width, _previewSize.Height);

            var surface = new Surface(texture);

            _previewBuilder = _cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
            _previewBuilder.AddTarget(surface);

            List<Surface> surfaces = new List<Surface>();
            surfaces.Add(surface);
            surfaces.Add(_imageReader.Surface);

            _cameraDevice.CreateCaptureSession(surfaces,
                new CameraCaptureStateListener
                {
                    OnConfigureFailedAction = session =>
                    {
                    },
                    OnConfiguredAction = session =>
                    {
                        _previewSession = session;
                        UpdatePreview();
                    }
                },
                _backgroundHandler);
        }

        private void UpdatePreview()
        {
            if (_cameraDevice == null || _previewSession == null) return;

            // Reset the auto-focus trigger
            _previewBuilder.Set(CaptureRequest.ControlAfMode, (int)ControlAFMode.ContinuousPicture);
            SetAutoFlash(_previewBuilder);

            _previewRequest = _previewBuilder.Build();
            _previewSession.SetRepeatingRequest(_previewRequest, _cameraCaptureListener, _backgroundHandler);
        }

        Size GetOptimalSize(IList<Size> sizes, int h, int w)
        {
            double AspectTolerance = 0.1;
            double targetRatio = (double)w / h;

            if (sizes == null)
            {
                return null;
            }

            Size optimalSize = null;
            double minDiff = double.MaxValue;
            int targetHeight = h;

            while (optimalSize == null)
            {
                foreach (Size size in sizes)
                {
                    double ratio = (double)size.Width / size.Height;

                    if (System.Math.Abs(ratio - targetRatio) > AspectTolerance)
                        continue;
                    if (System.Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = System.Math.Abs(size.Height - targetHeight);
                    }
                }

                if (optimalSize == null)
                    AspectTolerance += 0.1f;
            }

            return optimalSize;
        }
        public string OptionFlash;
        public void SetAutoFlash(CaptureRequest.Builder requestBuilder)
        {
            if (_flashSupported)
            {
                //requestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
                //requestBuilder.Set(CaptureRequest.FlashMode, (int)FlashMode.Off);
                switch (OptionFlash)
                {
                    case "on":
                        requestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAlwaysFlash);
                        requestBuilder.Set(CaptureRequest.FlashMode, (int)FlashMode.Single);
                        break;
                    case "off":
                        requestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.Off);
                        requestBuilder.Set(CaptureRequest.FlashMode, (int)FlashMode.Off);
                        break;
                    case "auto":
                    default:
                        requestBuilder.Set(CaptureRequest.ControlAeMode, (int)ControlAEMode.OnAutoFlash);
                        break;
                    
                }
            }
        }

        private void StartBackgroundThread()
        {
            _backgroundThread = new HandlerThread("CameraBackground");
            _backgroundThread.Start();
            _backgroundHandler = new Handler(_backgroundThread.Looper);
        }

        private void StopBackgroundThread()
        {
            _backgroundThread.QuitSafely();
            try
            {
                _backgroundThread.Join();
                _backgroundThread = null;
                _backgroundHandler = null;
            }
            catch (InterruptedException e)
            {
                e.PrintStackTrace();
            }
        }

        public void LockFocus()
        {
            try
            {
                // This is how to tell the camera to lock focus.
                _previewBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Start);
                // Tell #mCaptureCallback to wait for the lock.
                mState = STATE_WAITING_LOCK;
                _previewSession.Capture(_previewBuilder.Build(), _cameraCaptureListener, _backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void UnlockFocus()
        {
            try
            {
                // Reset the auto-focus trigger
                _previewBuilder.Set(CaptureRequest.ControlAfTrigger, (int)ControlAFTrigger.Cancel);
                SetAutoFlash(_previewBuilder);

                _previewSession.Capture(_previewBuilder.Build(), _cameraCaptureListener, _backgroundHandler);
                // After this, the camera will go back to the normal state of preview.
                mState = STATE_PREVIEW;
                _previewSession.SetRepeatingRequest(_previewRequest, _cameraCaptureListener, _backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }

        public void RunPrecaptureSequence()
        {
            try
            {
                _previewBuilder.Set(CaptureRequest.ControlAePrecaptureTrigger, (int)ControlAEPrecaptureTrigger.Start);
                mState = STATE_WAITING_PRECAPTURE;
                _previewSession.Capture(_previewBuilder.Build(), _cameraCaptureListener, _backgroundHandler);
            }
            catch (CameraAccessException e)
            {
                e.PrintStackTrace();
            }
        }
    }
}
