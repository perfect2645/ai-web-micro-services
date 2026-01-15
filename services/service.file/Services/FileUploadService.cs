
using Logging;
using Microsoft.Extensions.Options;
using NetUtils.Repository;
using repository.file.Repositories.Entities;
using service.file.Configurations.DomainSettings;
using Utils.EncodingEx;

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

        public async Task<UploadedItem?> GetUploadedItemAsync(long fileSize, string sha256Hash, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAsync(item => item.FileSize == fileSize && item.FileHash == sha256Hash, true, cancellationToken);
        }

        public async Task<UploadedItem> UploadFileAsync(Stream fileDataStream, string fileName, string? description = null, CancellationToken cancellationToken = default)
        {
            // save file to disk D:\Web\image-ai\fileServiceStorage
            string hash = HashHelper.ComputeSHA256Hash(fileDataStream);
            long fileSize = fileDataStream.Length;
            var today = DateTime.Today;
            string remotePath = Path.Combine(_fileStorageSettings.RemotePath, $"/{today:yyyy/MM/dd}/{fileName}");
            var remoteUri = await SaveFileToStorage(fileDataStream, remotePath, cancellationToken);

            string backupPath = Path.Combine(_fileStorageSettings.BackupPath, $"/{today:yyyy/MM/dd}/{fileName}");
            var backupUri = await SaveFileToStorage(fileDataStream, remotePath, cancellationToken);
            var uploadedItem = new UploadedItem(fileName, fileSize, hash, backupUri, remoteUri, description);

            await _repository.AddAsync(uploadedItem, cancellationToken);
            Log4Logger.Logger.Info($"File metadata saved to database. FileId: {uploadedItem.Id}");
            return uploadedItem;
        }

        public async Task<UploadedItem> UploadFileAsync(IFormFile formFile, string? description = null, CancellationToken cancellationToken = default)
        {
            var fileName = formFile.FileName;
            using var fileDataStream = formFile.OpenReadStream();
            var fileStoragePath = await SaveFileToStorage(fileDataStream, fileName, cancellationToken);
            return await UploadFileAsync(fileDataStream, fileName, description, cancellationToken);
        }

        private async Task<string> SaveFileToStorage(Stream fileDataStream, string storagePath, CancellationToken cancellationToken)
        {
            var filePath = Path.Combine(storagePath, Guid.NewGuid().ToString());
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
