using Microsoft.EntityFrameworkCore;
using NetUtils.Repository;
using repository.doraemon.Entities;
using repository.doraemon.Repositories.Entities;
using Utils.Ioc;

namespace repository.doraemon.Repositories.Domain
{
    [Register(Key = Constants.ImageDataRepositoryIocKey, ServiceType = typeof(IImageDataRepository))]
    public class ImageDataRepository : RepositoryBase<DoraemonItem, Guid>, IImageDataRepository
    {
        public ImageDataRepository(AppDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<DoraemonItem>?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var targets = await GetAllAsync(item => item.UserId == userId, cancellationToken);
            return targets?.ToList();
        }
    }
}
