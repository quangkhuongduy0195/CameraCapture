using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CameraCapture
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            cameraView.Capture();
        }

        void cameraView_DidFinishProcessingPhoto(System.Object sender, Xamarin.Forms.ImageSource e)
        {
            gridPreview.IsVisible = true;
            imgPreview.Source = e;
        }

        void btnhidePreview_Clicked(System.Object sender, System.EventArgs e)
        {
            gridPreview.IsVisible = false;
            imgPreview.Source = null;
        }
    }
}
