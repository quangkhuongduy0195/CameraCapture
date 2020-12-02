using System;
using System;
using System.Collections.Generic;
using Android;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Hardware.Camera2;
using Android.Hardware.Camera2.Params;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using CameraCapture.Renderers.Camera;
using Java.IO;
using Plugin.CurrentActivity;

namespace CameraCapture.Droid.Renderers.Camera
{
    public interface ICameraPreviewDelege
    {
        void resultQRCode(string code);
    }

    public partial class CameraPreview : ViewGroup, ISurfaceHolderCallback, Handler.ICallback//, View.IOnTouchListener //IProcessor
    {
        private Handler mHandler;
        SurfaceView surfaceView;
        ISurfaceHolder holder;
        CameraOptions cameraOption;
        public bool IsPreviewing { get; set; }
        public bool IsConfigureCamera { get; set; }
        public bool mManualFocusEngaged { get; set; }
        CameraManager mCameraManager;
        Context context;
        string[] mCameraIDsList;
        CameraDevice mCameraDevice;
        CameraCaptureSession mCaptureSession;
        CaptureRequest.Builder _previewRequestBuilder;
        private Surface mCameraSurface = null;


        //BarcodeDetector barcodeDetector;
        //CameraSource cameraSource;
        public ICameraPreviewDelege iCameraPreview;



        public CameraPreview(Context context, CameraOptions cameraOption)
            : base(context)
        {
            this.cameraOption = cameraOption;
            this.context = context;
            surfaceView = new SurfaceView(context);
            //surfaceView.SetOnTouchListener(this);
            AddView(surfaceView);

            holder = surfaceView.Holder;
            holder.AddCallback(this);

            mHandler = new Handler(this);

            try
            {
                this.mCameraManager = CrossCurrentActivity.Current.Activity.GetSystemService(Context.CameraService) as CameraManager;
                mCameraIDsList = this.mCameraManager.GetCameraIdList();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Exception: " + ex.Message);
            }

            OpenCamera(cameraOption);

            IsPreviewing = false;
        }

        public string FOLDER_NAME = "Photo";
        public string FILE_NAME = "image_";
        public string ROOT_FOLDER = "Demo Camera2";
        // Create file
        File reateImageFile()
        {
            var dirPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + File.Separator + ROOT_FOLDER + File.Separator + FOLDER_NAME;
            File dir = new File(dirPath);
            if (!dir.Exists())
                dir.Mkdirs();
            var fileName = FILE_NAME + DateTime.Today.ToLongTimeString() + "jpg";
            return new File(dir, fileName);
        }

        void CloseCamera()
        {
            mCaptureSession.Close();
            mCaptureSession = null;
            mCameraDevice?.Close();
            mCameraDevice = null;
            IsConfigureCamera = false;
        }

        void OpenCamera(CameraOptions cameraOption)
        {
            if (mCameraIDsList.Length > 1)
            {
                mCameraManager.OpenCamera(cameraOption == CameraOptions.Front ? mCameraIDsList[1] : mCameraIDsList[0], new CameraStateListener(this), new Handler());
            }
            else
            {
                mCameraManager.OpenCamera(mCameraIDsList[mCameraIDsList.Length - 1], new CameraStateListener(this), new Handler());
            }
        }

        internal void ChangeCameraOption(CameraOptions cameraOption)
        {
            this.cameraOption = cameraOption;
            CloseCamera();
            OpenCamera(cameraOption);
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            surfaceView.Measure(msw, msh);
            surfaceView.Layout(0, 0, r - l, b - t);
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            mCameraSurface = holder.Surface;
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            IsPreviewing = true;
        }

        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            mCameraSurface = holder.Surface;
            mHandler.SendEmptyMessage(2);
            IsPreviewing = true;
        }

        public bool HandleMessage(Message msg)
        {
            switch (msg.What)
            {
                case 1:
                case 2:
                    if (IsPreviewing && mCameraDevice != null && !IsConfigureCamera)
                    {
                        configureCamera();
                    }
                    break;
            }
            return true;
        }

        private void configureCamera()
        {
            List<Surface> sfl = new List<Surface>();
            sfl.Add(mCameraSurface);
            try
            {
                mCameraDevice.CreateCaptureSession(sfl, new CaptureSessionListener(this), null);
                IsConfigureCamera = true;
            }
            catch (Exception ex)
            {

            }
        }

        int i = -1;
        public override bool OnTouchEvent(MotionEvent motionEvent)
        {

            var actionMasked = motionEvent.ActionMasked;
            switch (actionMasked)
            {
                case MotionEventActions.Down:
                    i++;
                    if (i > 8)
                    {
                        i = 0;
                    }
                    _previewRequestBuilder?.Set(CaptureRequest.ControlEffectMode, i);
                    try
                    {
                        mCaptureSession?.SetRepeatingRequest(_previewRequestBuilder?.Build(), null, null);
                    }
                    catch (Exception ex)
                    {

                    }
                    break;
            }

            return true;
        }


        //public override bool OnTouchEvent(MotionEvent motionEvent)
        //{
        //    var actionMasked = motionEvent.ActionMasked;
        //    switch (actionMasked)
        //    {
        //        case MotionEventActions.Down:

        //            if (mCameraIDsList == null || mCameraIDsList.Length < 1) return true;
        //            var cameraId = mCameraIDsList[mCameraIDsList.Length - 1];
        //            if (mCameraIDsList.Length > 1)
        //            {
        //                cameraId = cameraOption == CameraOptions.Front ? mCameraIDsList[1] : mCameraIDsList[0];
        //            }
        //            CameraCharacteristics cc = null;
        //            try
        //            {
        //                cc = mCameraManager.GetCameraCharacteristics(cameraId);
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //            // Lay id cua ngon tay dau tien
        //            int pointerId = motionEvent.GetPointerId(0);
        //            // Lay vi tri cua ngon tay dau tien
        //            int pointerIndex = motionEvent.FindPointerIndex(pointerId);

        //            // Lay toa do cua ngon tay dau tien.
        //            float x = motionEvent.GetX(pointerIndex);
        //            float y = motionEvent.GetY(pointerIndex);

        //            var touchRect = new Rect((int)(x - 100), (int)(y - 100), (int)(x + 100), (int)(y + 100));

        //            MeteringRectangle focusArea = new MeteringRectangle(touchRect, MeteringRectangle.MeteringWeightDontCare);
        //            _previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, 2);

        //            try
        //            {
        //                mCaptureSession.Capture(_previewRequestBuilder.Build(), new CameraCaptureSessionCallBack(this), new Handler());
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //            _previewRequestBuilder.Set(CaptureRequest.ControlAeRegions, new MeteringRectangle[] { focusArea });
        //            _previewRequestBuilder.Set(CaptureRequest.ControlAfRegions, new MeteringRectangle[] { focusArea });

        //            _previewRequestBuilder.Set(CaptureRequest.ControlAfMode, 4);
        //            _previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, 1);
        //            _previewRequestBuilder.Set(CaptureRequest.ControlAePrecaptureTrigger, 1);

        //            try
        //            {
        //                mCaptureSession.SetRepeatingRequest(_previewRequestBuilder.Build(), new CameraCaptureSessionCallBack(this), new Handler());
        //            }
        //            catch (Exception ex)
        //            {

        //            }

        //            return true;
        //        default:
        //            return false;
        //    }
        //}

        //CameraCharacteristics mCameraCharacteristics = null;
        //public bool OnTouch(View v, MotionEvent motionEvent)
        //{
        //var actionMasked = motionEvent.ActionMasked;
        //switch (actionMasked)
        //{
        //    case MotionEventActions.Down:

        //        if (mCameraIDsList == null || mCameraIDsList.Length < 1 || mManualFocusEngaged) return true;
        //        var cameraId = mCameraIDsList[mCameraIDsList.Length - 1];
        //        if (mCameraIDsList.Length > 1)
        //        {
        //            cameraId = cameraOption == CameraOptions.Front ? mCameraIDsList[1] : mCameraIDsList[0];
        //        }

        //        try
        //        {
        //            mCameraCharacteristics = mCameraManager.GetCameraCharacteristics(cameraId);
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //        Rect sensorArraySize = mCameraCharacteristics.Get(CameraCharacteristics.SensorInfoActiveArraySize) as Rect;

        //        int x = (int)((motionEvent.GetX() / (float)v.Width) * (float)sensorArraySize.Height());
        //        int y = (int)((motionEvent.GetY() / (float)v.Height) * (float)sensorArraySize.Width());
        //        int halfTouchWidth = 50;
        //        int halfTouchHeight = 50;

        //        var focusAreaTouh = new MeteringRectangle(Math.Max(x - halfTouchWidth, 0), Math.Max(y - halfTouchHeight, 0), halfTouchWidth * 2, halfTouchHeight * 2, MeteringRectangle.MeteringWeightMax - 1);


        //        try
        //        {
        //            mCaptureSession.StopRepeating();
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //        _previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, 2);
        //        _previewRequestBuilder.Set(CaptureRequest.ControlAfMode, 0);


        //        try
        //        {
        //            mCaptureSession.Capture(_previewRequestBuilder.Build(), new CameraCaptureSessionCallBack(this), new Handler());
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //        if (IsMeteringAreaAFSupported())
        //        {
        //            _previewRequestBuilder.Set(CaptureRequest.ControlAfRegions, new MeteringRectangle[] { focusAreaTouh });
        //        }
        //        _previewRequestBuilder.Set(CaptureRequest.ControlMode, 1);
        //        _previewRequestBuilder.Set(CaptureRequest.ControlAfMode, 1);
        //        _previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, 1);


        //        try
        //        {
        //            mCaptureSession.Capture(_previewRequestBuilder.Build(), new CameraCaptureSessionCallBack(this), new Handler());
        //        }
        //        catch (Exception ex)
        //        {

        //        }

        //        mManualFocusEngaged = true;

        //        return true;
        //    default:
        //        return false;
        //}

        //bool IsMeteringAreaAFSupported()
        //{
        //    int val = (int)mCameraCharacteristics.Get(CameraCharacteristics.ControlMaxRegionsAf);
        //    return val >= 1;
        //}

        //}

        public class CameraCaptureSessionCallBack : CameraCaptureSession.CaptureCallback
        {
            CameraPreview cameraPreview;

            public CameraCaptureSessionCallBack(CameraPreview cameraPreview)
            {
                this.cameraPreview = cameraPreview;
            }

            public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request, TotalCaptureResult result)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureCompleted");
                base.OnCaptureCompleted(session, request, result);
                this.cameraPreview.mManualFocusEngaged = false;
                if (request?.Tag?.ToString() == "FOCUS_TAG")
                {
                    this.cameraPreview._previewRequestBuilder.Set(CaptureRequest.ControlAfTrigger, null);
                    try
                    {
                        this.cameraPreview.mCaptureSession.SetRepeatingRequest(this.cameraPreview._previewRequestBuilder.Build(), null, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"======OnCaptureCompleted: {ex.Message}");
                    }
                }
            }

            public override void OnCaptureFailed(CameraCaptureSession session, CaptureRequest request, CaptureFailure failure)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureFailed");
                base.OnCaptureFailed(session, request, failure);
                this.cameraPreview.mManualFocusEngaged = false;
            }

            public override void OnCaptureStarted(CameraCaptureSession session, CaptureRequest request, long timestamp, long frameNumber)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureStarted");
                base.OnCaptureStarted(session, request, timestamp, frameNumber);
            }

            public override void OnCaptureSequenceCompleted(CameraCaptureSession session, int sequenceId, long frameNumber)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureSequenceCompleted");
                base.OnCaptureSequenceCompleted(session, sequenceId, frameNumber);
            }

            public override void OnCaptureSequenceAborted(CameraCaptureSession session, int sequenceId)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureSequenceAborted");
                base.OnCaptureSequenceAborted(session, sequenceId);
            }

            public override void OnCaptureProgressed(CameraCaptureSession session, CaptureRequest request, CaptureResult partialResult)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureProgressed");
                base.OnCaptureProgressed(session, request, partialResult);
            }

            public override void OnCaptureBufferLost(CameraCaptureSession session, CaptureRequest request, Surface target, long frameNumber)
            {
                System.Diagnostics.Debug.WriteLine("======OnCaptureBufferLost");
                base.OnCaptureBufferLost(session, request, target, frameNumber);
            }
        }
    }
}
