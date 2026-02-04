using repository.doraemon.Entities;
using repository.doraemon.Repositories.Domain;
using service.domain.Models;
using Utils.Ioc;

namespace service.domain.Services
{
    [Register]
    public class ImageDataService(IImageDataRepository imageDataRepository) : IImageDataService
    {
        private readonly IImageDataRepository _imageDataRepository = imageDataRepository;
        public async Task<IReadOnlyList<DoraemonItem>?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _imageDataRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<DoraemonItem> AddAsync(DoraemonItemCreateDto doraemonItemCreateDto,
            CancellationToken cancellationToken = default)
        {
            var newItem = doraemonItemCreateDto.ToDto();
            newItem = await _imageDataRepository.AddAsync(newItem, cancellationToken);
            await _imageDataRepository.SaveChangeAsync(cancellationToken);

            return newItem;
        }

        public async Task UpdateAsync(DoraemonItem item, CancellationToken cancellationToken = default)
        {
            await _imageDataRepository.UpdateAsync(item);
            await _imageDataRepository.SaveChangeAsync(cancellationToken);
        }
    }
}
