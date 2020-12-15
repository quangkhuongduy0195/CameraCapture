using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CameraCapture.ApiDefinitions.Camera;
using CameraCapture.Models;
using Refit;
using Xamarin.Forms;

namespace CameraCapture.Renderers.Camera
{
    public enum CameraOptions
    {
        Rear,
        Front
    }

    public class CameraView : View
    {
        public static readonly BindableProperty CameraOptionProperty = BindableProperty.Create(propertyName: nameof(CameraOption),
                                                                                         returnType: typeof(CameraOptions),
                                                                                         declaringType: typeof(CameraView),
                                                                                         defaultValue: CameraOptions.Rear);

        public CameraOptions CameraOption
        {
            get { return (CameraOptions)GetValue(CameraOptionProperty); }
            set { SetValue(CameraOptionProperty, value); }
        }

        public Action _captureAction;
        public void Capture()
        {
            _captureAction?.Invoke();
        }

        public Action _switchCameraAction;
        public void SwitchCamera()
        {
            _switchCameraAction?.Invoke();
        }

        public event EventHandler<ImageSource> FinishProcessingPhoto;
        public void HandleDidFinishProcessingPhoto(ImageSource image ,byte[] imageByte, string fileId)
        {
            ImageByte = imageByte;
            FileId = fileId;
            FinishProcessingPhoto?.Invoke(this, image);
        }
        public string OptionFlash { get; set; } = "auto";
        bool SaveImageServer;
        byte[] ImageByte { get; set; }
        string FileId { get; set; }
        private ImageServerModel _imageServerModel;
        IImageApi _imageApi;
        public async void HandleDidSavePhotoServer()
        {
            if (ImageByte == null) return;
            string toBase64String = Convert.ToBase64String(ImageByte);
            var request = new ImageSaveRequest
            {
                FileId = FileId,
                FileName = FileId + ".png",
                Base64File = toBase64String
            };
            var apiResponse = RestService.For<IImageApi>("https://filesave.herokuapp.com");
            var response = await apiResponse.SaveImage(request);
            if(response.Success)
            {
                SaveImageServer = true;
                await App.Current.MainPage.DisplayAlert("", "Saved image to server is successful!", "OK");
            }
        }

       
        public async Task<ImageGetResponse> GetImageServerAsync()
        {
            if(SaveImageServer)
            {
                var request = new ImageGetRequest
                {
                    FileId = FileId
                };
                var apiResponse = RestService.For<IImageApi>("https://filesave.herokuapp.com");
                var response = await apiResponse.GetImage(request);
                return response;
            }
            return null;
        }
    }
}
