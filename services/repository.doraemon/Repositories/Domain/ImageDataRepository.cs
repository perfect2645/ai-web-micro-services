using Microsoft.EntityFrameworkCore;
using NetUtils.Repository;
using repository.doraemon.Entities;

namespace repository.doraemon.Repositories.Domain
{
    public class ImageDataRepository : RepositoryBase<DoraemonItem, Guid>, IImageDataRepository
    {
        public ImageDataRepository(DbContext dbContext) : base(dbContext)
        {
        }

        public async Task<IReadOnlyList<DoraemonItem>?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var targets = await GetAllAsync(item => item.UserId == userId, cancellationToken);
            return targets?.ToList();
        }
    }
}
