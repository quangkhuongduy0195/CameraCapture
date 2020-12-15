﻿using System;
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

    public enum Flash
    {
        On, Off, Auto
    }

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
            if(switchAutoSave.IsToggled)
                Button_Save();
        }

        //async void Button_GetImage(System.Object sender, System.EventArgs e)
        //{
        //    var getImage = await cameraView.GetImageServerAsync();
        //    if (getImage == null) return;
        //    string linkImage = getImage.LinkFile;
        //    string url = "https://filesave.herokuapp.com" + linkImage;
        //    //imgServer.Source = ImageSource.FromUri(new Uri(url));
        //    imgServer.Source = url;
        //}
        //https://filesave.herokuapp.com/public/files/22225img2020-12-10T12:25:50TZD.png

        void cameraView_DidFinishProcessingPhoto(System.Object sender, Xamarin.Forms.ImageSource e)
        {
            imgPreview.Source = e;
        }

        //void btnhidePreview_Clicked(System.Object sender, System.EventArgs e)
        //{
        //    //gridPreview.IsVisible = false;
        //    //imgPreview.Source = null;
        //}

        //Button_Save
        void Button_Save()
        {
            cameraView.HandleDidSavePhotoServer();
        }

        //TapFlash
        Flash flash = Flash.Auto;
        void TapFlash(System.Object sender, System.EventArgs e)
        {
            switch (flash)
            {
                case Flash.On:
                    imageFlash.Source = "flash_off";
                    cameraView.OptionFlash = "off";
                    flash = Flash.Off;
                    break;
                case Flash.Off:
                    imageFlash.Source = "flash_auto";
                    cameraView.OptionFlash = "auto";
                    flash = Flash.Auto;
                    break;
                case Flash.Auto:
                    imageFlash.Source = "flash_on";
                    cameraView.OptionFlash = "on";
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
    }
}