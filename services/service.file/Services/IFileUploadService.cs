using repository.file.Repositories.Entities;

namespace service.file.Services
{
    public interface IFileUploadService
    {
        Task<UploadedItem?> GetUploadedItemAsync(long fileSize, string sha256Hash, CancellationToken cancellationToken = default);
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string? description = null, CancellationToken cancellationToken = default);
    }
}
