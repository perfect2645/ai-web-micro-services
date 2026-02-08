
using Logging;
using Microsoft.Extensions.Options;
using NetUtils.Aspnet.Generic;
using NetUtils.Repository;
using repository.doraemon.Repositories.Entities;
using service.file.Configurations;
using service.file.Configurations.DomainSettings;
using Utils.EncodingEx;
using Utils.Ioc;

namespace service.file.Services
{
    [Register(ServiceType = typeof(IFileUploadService))]
    public class FileUploadService : IFileUploadService
    {

        private readonly IRepository<UploadedItem, Guid> _repository;
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public FileUploadService([FromKeyedServices(repository.doraemon.Constants.FileRepositoryIocKey)]IRepository<UploadedItem, Guid> repository, 
            IOptions<FileStorageSettings> fileStorageSettings,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _fileStorageSettings = fileStorageSettings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UploadedItem?> GetUploadedItemAsync(long fileSize, string sha256Hash, CancellationToken cancellationToken = default)
        {
            return await _repository.GetAsync(item => item.FileSize == fileSize && item.FileHash == sha256Hash, true, cancellationToken);
        }

        public async Task<UploadedItem> UploadFileAsync(IFormFile formFile, string? description = null, CancellationToken cancellationToken = default)
        {
            var fileName = formFile.FileName;
            using var fileDataStream = formFile.OpenReadStream();
            return await UploadFileAsync(fileDataStream, fileName, description, cancellationToken);
        }

        public async Task<UploadedItem> UploadFileAsync(Stream fileDataStream, string fileName, string? description = null, CancellationToken cancellationToken = default)
        {
            // save file to disk D:\Web\image-ai\fileServiceStorage
            var fileHashInCache = _httpContextAccessor.GetString(Constants.Context_FileHash);
            var fileHash = fileHashInCache ?? HashHelper.ComputeSHA256Hash(fileDataStream);
            long fileSize = fileDataStream.Length;
            var backupPath = string.Empty;
            var remotePath = string.Empty;

            _ = _httpContextAccessor.TryGetValue(Constants.Context_ExistingFile, out UploadedItem? existingFile);
            if (existingFile != null)
            {
                backupPath = existingFile.BackupUrl;
                remotePath = existingFile.RemoteUrl;
            }
            else
            {
                var datePathPattern = $"{DateTime.Today:yyyyMMdd}/{fileHash}/{fileName}";
                backupPath = Path.Combine(_fileStorageSettings.BackupPath, datePathPattern);
                remotePath = Path.Combine(_fileStorageSettings.RemotePath, datePathPattern);
            }

            var backupUrl = await SaveFileToStorageAsync(fileDataStream, backupPath, cancellationToken);
            ConvertLocalPathToStaticUrl(ref backupUrl);

            fileDataStream.Position = 0;

            var remoteUrl = await SaveFileToStorageAsync(fileDataStream, remotePath, cancellationToken);
            ConvertLocalPathToStaticUrl(ref remoteUrl);
            var uploadedItem = new UploadedItem(fileName, fileSize, fileHash, backupUrl, remoteUrl, description);

            await SaveFileItemToDbAsync(uploadedItem, cancellationToken);
            return uploadedItem;
        }

        private async ValueTask SaveFileItemToDbAsync(UploadedItem uploadedItem, CancellationToken cancellationToken)
        {
            _ = _httpContextAccessor.TryGetValue(Constants.Context_ExistingFile, out UploadedItem? existingFile);
            if (existingFile != null)
            {
                Log4Logger.Logger.Info($"Target file (SHA256: {uploadedItem.FileHash}) already exists in the system.");
                return;
            }

            await _repository.AddAsync(uploadedItem, cancellationToken);
            await _repository.SaveChangeAsync();
            Log4Logger.Logger.Info($"File metadata saved to database. FileId: {uploadedItem.Id}");
        }

        private async Task<string> SaveFileToStorageAsync(Stream fileDataStream, string filePath, CancellationToken cancellationToken)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            //if (File.Exists(filePath))
            //{
            //    //throw new InvalidOperationException($"Same file already exists, skip saving.[{filePath}]");
            //    Log4Logger.Logger.Info($"Same file already exists, skip saving.[{filePath}]");
            //    return filePath;
            //}
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await fileDataStream.CopyToAsync(fileStream, 81920, cancellationToken);
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

        private void ConvertLocalPathToStaticUrl(ref string filePath)
        {
            var relativePath = Path.GetRelativePath(_fileStorageSettings.LocalRootPath, filePath);

            var urlRelativePath = relativePath.Replace('\\', '/');

            filePath = $"{_fileStorageSettings.ApiBaseUrl}{_fileStorageSettings.RequestPath}/{urlRelativePath}";
        }
    }
}
