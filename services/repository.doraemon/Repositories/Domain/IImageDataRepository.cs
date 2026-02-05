using NetUtils.Repository;
using repository.doraemon.Entities;

namespace repository.doraemon.Repositories.Domain
{
    public interface IImageDataRepository : IRepository<DoraemonItem, Guid>
    {
        Task<IReadOnlyList<DoraemonItem>?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    }
}
