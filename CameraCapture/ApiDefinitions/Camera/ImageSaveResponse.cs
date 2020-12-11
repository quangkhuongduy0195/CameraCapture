using System;
using Newtonsoft.Json;

namespace CameraCapture.ApiDefinitions.Camera
{
    public class ImageSaveResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
