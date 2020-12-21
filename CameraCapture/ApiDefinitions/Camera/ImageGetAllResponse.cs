using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace CameraCapture.ApiDefinitions.Camera
{
    public class ImageGetAllResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("files")]
        public IList<File> Files { get; set; }
    }

    public class File
    {

        [JsonProperty("fileId")]
        public string FileId { get; set; }

        [JsonProperty("fileName")]
        public string FileName { get; set; }

        [JsonProperty("linkFile")]
        public string LinkFile { get; set; }

        public string FullLinkFile { get; set; }
    }
}

