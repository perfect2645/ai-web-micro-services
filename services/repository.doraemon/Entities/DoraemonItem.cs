using NetUtils.Aspnet.Repository;
using System.ComponentModel.DataAnnotations;

namespace repository.doraemon.Entities
{
    public record DoraemonItem(
        [property: Required]
        [property: MaxLength(64)]
        string UserId,

        [property: Required]
        Guid InputImageId,

        [property: MaxLength(512)] 
        string? PromptText
    ) : IEntity, IEntityTiming
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime CreateTime { get; init; } = DateTime.Now;
        public DateTime? UpdateTime { get; private set; }

        [Required]
        public ImageRecognitionStatus Status { get; set; } = ImageRecognitionStatus.Prompt;
        public Guid? OutputImageId { get;set; }
    }
}
