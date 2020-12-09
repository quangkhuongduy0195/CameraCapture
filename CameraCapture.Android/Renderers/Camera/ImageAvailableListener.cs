using System;
using System.IO;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Provider;
using Plugin.CurrentActivity;

namespace CameraCapture.Droid.Renderers.Camera
{
    public class ImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        int _orientation = 0;
        public ImageAvailableListener(int orientation)
        {
            this._orientation = orientation;
        }

        public event EventHandler<byte[]> Photo;

        public void OnImageAvailable(ImageReader reader)
        {
            Image image = null;

            try
            {


                image = reader.AcquireLatestImage();
                var buffer = image.GetPlanes()[0].Buffer;
                var imageData = new byte[buffer.Capacity()];
                buffer.Get(imageData);
                var imgBitmap = rotateImage(imageData, _orientation);

                MediaStore.Images.Media.InsertImage(CrossCurrentActivity.Current.Activity.ContentResolver, imgBitmap, "Test", "DuyQK");

                MemoryStream stream = new MemoryStream();
                imgBitmap.Compress(Bitmap.CompressFormat.Png, 0, stream);
                byte[] bytepData = stream.ToArray();

                Photo?.Invoke(this, imageData);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                image?.Close();
            }
        }

        public static Bitmap rotateImage(byte[] source, float angle)
        {
            Bitmap storedBitmap = BitmapFactory.DecodeByteArray(source, 0, source.Length, null);
            Matrix matrix = new Matrix();
            matrix.PostRotate(angle);
            return Bitmap.CreateBitmap(storedBitmap, 0, 0, storedBitmap.Width, storedBitmap.Height, matrix, true);
        }
    }
}