using repository.file.Repositories.Entities;

namespace repository.file.Repositories
{
    public interface IFileRepository
    {
        ValueTask<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash, CancellationToken ct = default);
    }
}
