using AppTech.Business.DTOs.Commons;
using AppTech.Business.Services.ExternalServices.Interfaces;
using AutoMapper;
using System.IO;

namespace AppTech.Business.MappingProfiles.Utilities
{
    public class CustomMappingAction<TSource, TDestination> : IMappingAction<TSource, TDestination> where TSource : IAuditedEntityDTO
    {
        private readonly IFileManagerService _fileManagerService;

        public CustomMappingAction(IFileManagerService fileManagerService)
        {
            _fileManagerService = fileManagerService;
        }

        public void Process(TSource source, TDestination destination, ResolutionContext context)
        {
            if (source.Image != null)
            {
                var property = destination.GetType().GetProperty("ImageUrl");
                if (property != null && property.PropertyType == typeof(string))
                {
                    var oldImageUrl = property.GetValue(destination)?.ToString();

                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        var oldFileName = oldImageUrl.Split("/").LastOrDefault();
                        var filePath = Path.Combine("C:/inetpub/wwwroot/backend2/wwwroot/uploads", oldFileName);

                        _fileManagerService.DeleteFile(filePath);
                    }
                }

                var uploadedUrl = _fileManagerService.UploadFileAsync(source.Image).Result;

                if (property != null && property.PropertyType == typeof(string))
                {
                    property.SetValue(destination, uploadedUrl);
                }
            }
        }
    }
}
