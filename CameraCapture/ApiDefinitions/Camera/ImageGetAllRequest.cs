using System;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace CameraCapture.ApiDefinitions.Camera
{
    public class ImageGetAllRequest
    {
        [JsonProperty("fileType")]
        public string FileType { get; set; }

        [JsonProperty("extFile")]
        public string ExtFile { get; set; }
    }
}

