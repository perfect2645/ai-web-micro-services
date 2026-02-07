using NetUtils.Aspnet.Repository;
using System.ComponentModel.DataAnnotations;

namespace repository.doraemon.Entities
{
    public record DoraemonItem(
        [Required]
        [MaxLength(64)]
        string UserId,

        [Required]
        Guid InputImageId,

        [Required]
        string InputImageUrl,

        [MaxLength(512)] 
        string? PromptText
    ) : IEntity, IEntityTiming
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime CreateTime { get; init; } = DateTime.Now;
        public DateTime? UpdateTime { get; set; }

        [Required]
        public ImageRecognitionStatus Status { get; set; } = ImageRecognitionStatus.Prompt;
        public string? ErrorMessage { get; set; } = string.Empty;
        public Guid? OutputImageId { get;set; }
        public string? OutputImageUrl { get;set; }
}
}
