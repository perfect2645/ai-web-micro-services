using repository.doraemon.Entities;
using service.domain.Models;

namespace service.domain.Services
{
    public interface IImageDataService
    {
        Task<IReadOnlyList<DoraemonItem>?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<DoraemonItem> AddAsync(DoraemonItemCreateDto doraemonItemCreateDto,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(DoraemonItem item, CancellationToken cancellationToken = default);
    }
}
