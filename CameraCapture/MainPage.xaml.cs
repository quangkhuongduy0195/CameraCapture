using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraCapture.ApiDefinitions.Camera;
using CameraCapture.Models;
using CameraCapture.Pages;
using CameraCapture.Renderers.Camera;
using Refit;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace CameraCapture
{

    public enum Flash
    {
        On, Off, Auto
    }

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            cameraView.Capture();
            cameraView.AutoSave = switchAutoSave.IsToggled;
            if (switchAutoSave.IsToggled)
            {
                LoadingProgressBar();
            }
        }

        async void LoadingProgressBar()
        {
            progressBar.Progress = 0;
            do
            {
                if (progressBar.Progress < 0.9)
                {
                    progressBar.Progress = progressBar.Progress + 0.15;
                    await Task.Delay(1000);
                }
            } while (!cameraView.SaveImageServer);
            progressBar.Progress = 1;
            await Task.Delay(1000);
            progressBar.Progress = 0;
        }

        void cameraView_DidFinishProcessingPhoto(System.Object sender, Xamarin.Forms.ImageSource e)
        {
            imgPreview.Source = e;
        }

        //Button_Save
        void Button_Save()
        {
            cameraView.HandleDidSavePhotoServer();
        }

        //TapFlash
        Flash flash = Flash.Off;
        void TapFlash(System.Object sender, System.EventArgs e)
        {
            switch (flash)
            {
                case Flash.On:
                    imageFlash.Source = "flash_off";
                    cameraView.FlashOption = FlashOptions.Off;
                    flash = Flash.Off;
                    break;
                case Flash.Off:
                    imageFlash.Source = "flash_auto";
                    cameraView.FlashOption = FlashOptions.Auto;
                    flash = Flash.Auto;
                    break;
                case Flash.Auto:
                    imageFlash.Source = "flash_on";
                    cameraView.FlashOption = FlashOptions.On;
                    flash = Flash.On;
                    break;
            }
        }

        void TapSwitchCamera(System.Object sender, System.EventArgs e)
        {
            if(cameraView.CameraOption == CameraOptions.Front)
                cameraView.CameraOption = CameraOptions.Rear;
            else
                cameraView.CameraOption = CameraOptions.Front;
            cameraView.SwitchCamera();
        }
        //TapLirary
        async void TapLirary(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ImageGalleryPageView());
        }

    }
}