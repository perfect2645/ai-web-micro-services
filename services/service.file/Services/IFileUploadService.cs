using repository.doraemon.Repositories.Entities;

namespace service.file.Services
{
    public interface IFileUploadService
    {
        Task<UploadedItem?> GetUploadedItemAsync(long fileSize, string sha256Hash, CancellationToken cancellationToken = default);
        Task<UploadedItem> UploadFileAsync(Stream fileStream, string fileName, string? description = null, CancellationToken cancellationToken = default);
        Task<UploadedItem> UploadFileAsync(IFormFile formFile, string? description = null, CancellationToken cancellationToken = default);
    }
}
