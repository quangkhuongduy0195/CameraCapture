using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace CameraCapture.Pages
{
    public partial class ImageDetailPageView : ContentPage
    {
        public ImageDetailPageView()
        {
            InitializeComponent();
        }

        async void GoBackEventAsync(System.Object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
