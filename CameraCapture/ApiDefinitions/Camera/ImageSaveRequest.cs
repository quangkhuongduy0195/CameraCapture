using System;
using Newtonsoft.Json;

namespace CameraCapture.ApiDefinitions.Camera
{
    public class ImageSaveRequest
    {
        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("base64File")]
        public string Base64File { get; set; }
    }
}
