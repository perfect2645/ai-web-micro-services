
using Logging;
using Microsoft.Extensions.Options;
using NetUtils.Repository;
using repository.file.Repositories.Entities;
using service.file.Configurations.DomainSettings;

namespace service.file.Services
{
    public class FileUploadService : IFileUploadService
    {

        private readonly IRepository<UploadedItem, string> _repository;
        private readonly FileStorageSettings _fileStorageSettings;

        public FileUploadService(IRepository<UploadedItem, string> repository, IOptions<FileStorageSettings> fileStorageSettings)
        {
            _repository = repository;
            _fileStorageSettings = fileStorageSettings.Value;
        }

        public async Task<string> UploadFileAsync(Stream fileDataStream, string fileName, string? description = null, CancellationToken cancellationToken = default)
        {
            // save file to disk D:\Web\image-ai\fileServiceStorage
            var fileStoragePath = await SaveFileToStorage(fileDataStream, fileName, cancellationToken);
            if (string.IsNullOrEmpty(fileStoragePath))
            {
                return string.Empty;
            }

            //var uploadedItem = new UploadedItem(fileName, )

            //await _repository.AddAsync(uploadedItem, cancellationToken);
            //Log4Logger.Logger.Info($"File metadata saved to database. FileId: {uploadedItem.Id}");
            //return uploadedItem.RemoteUrl;
            return string.Empty;
        }

        private async Task<string> SaveFileToStorage(Stream fileDataStream, string fileName, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(_fileStorageSettings.StoragePath, Guid.NewGuid().ToString(), fileName);
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            try
            {
                await fileDataStream.CopyToAsync(fileStream, cancellationToken);
                // make sure all data is written to disk
                await fileStream.FlushAsync(cancellationToken);
                Log4Logger.Logger.Info($"UploadFile succeed. File saved under [{filePath}]");
                return filePath;
            }
            catch (OperationCanceledException)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                Log4Logger.Logger.Warn($"File upload cancelled. [{filePath}]");
                throw;
            }
            catch (Exception ex)
            {
                if (File.Exists(filePath)) File.Delete(filePath);
                Log4Logger.Logger.Error($"Upload file failed. [{filePath}]", ex);
                throw;
            }
        }
    }
}
