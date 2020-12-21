using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using CameraCapture.ApiDefinitions.Camera;
using CameraCapture.Renderers.Camera;
using Refit;
using Xamarin.Forms;
using File = CameraCapture.ApiDefinitions.Camera.File;

namespace CameraCapture.Pages
{

    public partial class ImageGalleryPageView : ContentPage
    {
        public ImageGalleryPageView()
        {
            InitializeComponent();
            LoadBitmapCollection();
        }

        async void LoadBitmapCollection()
        {
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    var request = new ImageGetAllRequest
                    {
                        FileType = "1",
                        ExtFile = "png"
                    };
                    var apiResponse = RestService.For<IImageApi>("https://filesave.herokuapp.com");
                    var response = await apiResponse.GetAllImage(request);
                    TapGestureRecognizer _arrowtapGesture = new TapGestureRecognizer();
                    RowDefinitionCollection rowDefinitions = new RowDefinitionCollection();
                    _arrowtapGesture.Tapped += ArrowtapGesture_Tapped;
                    int numberGrid = 0;
                    for (int i = response.Files.Count - 1 ; i >= 0; i--)
                    {
                        numberGrid++;
                        string url = "https://filesave.herokuapp.com" + response.Files[i].LinkFile;

                        Image imgCapture = new Image();
                        
                        imgCapture.Source = url;
                        imgCapture.BindingContext = response.Files[i];
                        imgCapture.BackgroundColor = Color.LightGray;
                        
                        imgCapture.WidthRequest = 200;
                        imgCapture.HeightRequest = 200;

                        imgCapture.HorizontalOptions = LayoutOptions.Fill;
                        imgCapture.VerticalOptions = LayoutOptions.Fill;

                        imgCapture.GestureRecognizers.Add(_arrowtapGesture);

                        RowDefinition nameRowDefinition = new RowDefinition();
                        nameRowDefinition.Height = new GridLength(200, GridUnitType.Absolute);
                        rowDefinitions.Add(nameRowDefinition);
                        ColumnDefinition nameColumnDefinition = new ColumnDefinition();
                        nameColumnDefinition.Width = new GridLength(200, GridUnitType.Absolute);
                        if (numberGrid % 2 == 0)
                        {
                            Grid.SetColumn(imgCapture, 1);
                            Grid.SetRow(imgCapture, numberGrid / 2 - 1);
                        }
                        else
                        {
                            Grid.SetColumn(imgCapture, 0);
                            Grid.SetRow(imgCapture, (numberGrid - 1) / 2 );
                        }
                        copyrightGrid.Children.Add(imgCapture);
                    }
                }
                catch
                {
                    copyrightGrid.Children.Add(new Label
                    {
                        Text = "Cannot access list of bitmap files"
                    });
                }
            }
        }

        private async void ArrowtapGesture_Tapped(object sender, EventArgs e)
        {
            Image image = ((Image)sender);
            var item = image.BindingContext as File;
            if(item.FullLinkFile == null)
                item.FullLinkFile = "https://filesave.herokuapp.com" + item.LinkFile;
            var detailPage = new ImageDetailPageView();
            detailPage.BindingContext = item;
            await Navigation.PushAsync(detailPage);
        }

        //GoBackEvent
        async void GoBackEventAsync(System.Object sender, System.EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
