using System;
using Newtonsoft.Json;

namespace CameraCapture.ApiDefinitions.Camera
{
    public class ImageGetRequest
    {
        [JsonProperty("fileId")]
        public string FileId { get; set; }
    }
}
