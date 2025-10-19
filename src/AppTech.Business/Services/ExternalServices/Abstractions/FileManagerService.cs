using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Exceptions.UploadException;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class FileManagerService : IFileManagerService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileManagerService> _logger;

        public FileManagerService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<FileManagerService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public bool BeAValidImage(IFormFile file)
        {
            return file != null && file.ContentType.Contains("image") && 1024 * 1024 * 5 >= file.Length;
        }

        public void DeleteFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Local file deleted after successful upload.");
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            string fileName = await UploadLocalAsync(file);

            return "https://auth.apptech.edu.az/uploads/" + fileName;
        }

        public async Task UploadFileToServerAsync(string filePath, string uploadPath, string serverHost, string username, string password)
        {
            //var filePath = await UploadLocalAsync(file);

            //var uploadPath = "/var/www/backend/test.apptech.az/wwwroot/uploads";
            //var serverHost = _configuration["Sftp:ServerHost"];
            //var username = _configuration["Sftp:Username"];
            //var password = _configuration["Sftp:Password"];

            //await UploadFileToServerAsync(filePath, uploadPath, serverHost, username, password);

            //var uploadedUrl = Path.Combine($"http://test.apptech.az/uploads/", Path.GetFileName(filePath));

            //_logger.LogInformation($"File successfully uploaded to {uploadedUrl}");

            //return uploadedUrl;

            const int maxRetries = 3;
            int attempt = 0;
            bool success = false;

            while (attempt < maxRetries && !success)
            {
                try
                {
                    using var client = new SshClient(serverHost, username, password);
                    client.Connect();

                    using var scp = new ScpClient(client.ConnectionInfo);
                    scp.Connect();

                    using var fileStream = new FileStream(filePath, FileMode.Open);
                    await Task.Run(() => scp.Upload(fileStream, Path.Combine(uploadPath, Path.GetFileName(filePath))));

                    scp.Disconnect();
                    client.Disconnect();

                    success = true;
                    _logger.LogInformation("File successfully uploaded to server.");
                }
                catch (Exception ex)
                {
                    attempt++;
                    _logger.LogError(ex, $"Failed to upload file to server. Attempt {attempt} of {maxRetries}.");
                    if (attempt == maxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(2));
                }
            }
        }

        public async Task<string> UploadLocalAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("The file parameter cannot be null.");

            if (!BeAValidImage(file))
                throw new FileIsNotValidException();

            var fileName = Guid.NewGuid().ToString() + "_" +
                Path.GetFileNameWithoutExtension(file.FileName) +
                Path.GetExtension(file.FileName);

            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                _logger.LogInformation("Uploads directory created.");
            }

            var filePath = Path.Combine(uploadsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation($"File successfully saved locally at {filePath}");

            return fileName;
        }

        public async Task<string> DownloadAndSaveImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentException("The imageUrl parameter cannot be null or empty.");
            }

            using var httpClient = new HttpClient();
            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

            var fileName = Guid.NewGuid().ToString() + ".png";
            var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "google");

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
                _logger.LogInformation("Uploads directory created.");
            }

            var filePath = Path.Combine(uploadsPath, fileName);

            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

            _logger.LogInformation($"Image successfully downloaded and saved locally at {filePath}");

            return fileName;
        }

        public IFormFile GetFile(string fileName, string rootPath)
        {
            var filePath = Path.Combine(rootPath, fileName);
            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            var memoryStream = new MemoryStream();
            fileStream.CopyTo(memoryStream);
            memoryStream.Position = 0;

            var formFile = new FormFile(memoryStream, 0, memoryStream.Length, fileName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/png"
            };

            fileStream.Dispose();

            return formFile;
        }
    }
}
