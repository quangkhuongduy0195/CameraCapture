using System;
using System.Threading.Tasks;
using Refit;

namespace CameraCapture.ApiDefinitions.Camera
{
    public interface IImageApi
    {
        //https://filesave.herokuapp.com
        [Post("/api/saveFile")]
        Task<ImageSaveResponse> SaveImage([Body] ImageSaveRequest request);

        [Post("/api/getFile")]
        Task<ImageGetResponse> GetImage([Body]  ImageGetRequest request);
    }
}
