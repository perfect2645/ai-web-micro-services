namespace service.file.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string? description = null, CancellationToken cancellationToken = default);
    }
}
