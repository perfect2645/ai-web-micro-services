using repository.doraemon.Repositories.Entities;

namespace repository.doraemon.Repositories
{
    public interface IFileRepository
    {
        ValueTask<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash, CancellationToken ct = default);
    }
}
