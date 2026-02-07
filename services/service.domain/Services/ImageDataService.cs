using Logging;
using repository.doraemon.Entities;
using repository.doraemon.Repositories.Domain;
using service.domain.Models;
using service.messaging.Clients.Producer;
using service.shared.Models;
using Utils.Ioc;

namespace service.domain.Services
{
    [Register(ServiceType = typeof(IImageDataService))]
    public class ImageDataService(
        [FromKeyedServices(repository.doraemon.Constants.ImageDataRepositoryIocKey)] IImageDataRepository imageDataRepository,
        [FromKeyedServices(DomainConstants.Ioc_RabbitMq_DoraemonData_TopicMode)] MqProducerTopicMode rabbitMqProducer
        ) : IImageDataService
    {
        private readonly IImageDataRepository _imageDataRepository = imageDataRepository;
        private readonly MqProducerTopicMode _rabbitMqProducer = rabbitMqProducer;
        public async Task<IReadOnlyList<DoraemonItem>?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _imageDataRepository.GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<DoraemonItem> AddAsync(DoraemonItemCreateDto doraemonItemCreateDto,
            CancellationToken cancellationToken = default)
        {
            var newItem = doraemonItemCreateDto.ToDto();
            newItem = await _imageDataRepository.AddAsync(newItem, cancellationToken);

            try
            {
                await PublishMqAsync(newItem);
            }
            catch (Exception ex)
            {
                Log4Logger.Logger.Error($"Failed to publish message to MQ for item {newItem.Id}", ex);
                throw;
            }

            await _imageDataRepository.SaveChangeAsync(cancellationToken);

            return newItem;
        }

        public async Task UpdateAsync(DoraemonItem item, CancellationToken cancellationToken = default)
        {
            await _imageDataRepository.UpdateAsync(item);
            await _imageDataRepository.SaveChangeAsync(cancellationToken);
        }

        public async Task PublishMqAsync(DoraemonItem item, CancellationToken cancellationToken = default)
        {
            var doraemonMqData = new DoraemonMessage
            (
                Topic : DomainConstants.RabbitMq_Topic_DoraemonData,
                DoraemonItem : item,
                Source : DomainConstants.RabbitMq_Source_DomainService
            );
            await _rabbitMqProducer.ProduceAsync(doraemonMqData, cancellationToken);
        }
    }
}
