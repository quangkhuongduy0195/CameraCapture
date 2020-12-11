using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraCapture.ApiDefinitions.Camera;
using CameraCapture.Models;
using CameraCapture.Renderers.Camera;
using Refit;
using Xamarin.Forms;

namespace CameraCapture
{
    public partial class MainPage : ContentPage
    {
        IImageApi _imageApi;
        public MainPage()
        {
            InitializeComponent();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            cameraView.Capture();
        }

        async void Button_GetImage(System.Object sender, System.EventArgs e)
        {
            var getImage = await cameraView.GetImageServerAsync();
            if (getImage == null) return;
            string linkImage = getImage.LinkFile;
            string url = "https://filesave.herokuapp.com" + linkImage;
            //imgServer.Source = ImageSource.FromUri(new Uri(url));
            imgServer.Source = url;
        }
        //https://filesave.herokuapp.com/public/files/22225img2020-12-10T12:25:50TZD.png

        void cameraView_DidFinishProcessingPhoto(System.Object sender, Xamarin.Forms.ImageSource e)
        {
            imgPreview.Source = e;
        }

        void btnhidePreview_Clicked(System.Object sender, System.EventArgs e)
        {
            //gridPreview.IsVisible = false;
            //imgPreview.Source = null;
        }

        //Button_Save
        void Button_Save(System.Object sender, System.EventArgs e)
        {
            cameraView.HandleDidSavePhotoServer();
        }
    }
}
