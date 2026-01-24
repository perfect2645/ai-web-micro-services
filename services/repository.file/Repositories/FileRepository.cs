using NetUtils.Repository;
using repository.file.Repositories.Entities;
using Utils.Ioc;

namespace repository.file.Repositories
{
    [Register(Key = Constants.FileRepositoryIocKey, ServiceType = typeof(IRepository<UploadedItem, Guid>))]
    public class FileRepository : RepositoryBase<UploadedItem, Guid>, IFileRepository
    {

        public FileRepository(FileDbContext fileDbContext) : base(fileDbContext)
        {
        }

        public async ValueTask<UploadedItem?> FindFileAsync(long fileSize, string sha256Hash, CancellationToken ct = default)
        {
            return await GetAsync(item => item.FileHash == sha256Hash && item.FileSize == fileSize, true, ct);
        }
    }
}
