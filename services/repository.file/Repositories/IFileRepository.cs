using repository.file.Repositories.Entities;

namespace repository.file.Repositories
{
    public interface IFileRepository
    {
        ValueTask<UploadedItem?> FindFileAsync(string sha256Hash, long fileSize, CancellationToken ct = default);
    }
}
