using Microsoft.AspNetCore.Http;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IFileManagerService
    {
        void DeleteFile(string filePath);
        bool BeAValidImage(IFormFile file);
        Task<string> UploadFileAsync(IFormFile file);
        IFormFile GetFile(string fileName, string rootPath);
        Task<string> DownloadAndSaveImageAsync(string imageUrl);
    }
}
