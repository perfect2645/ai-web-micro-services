using System.ComponentModel.DataAnnotations;

namespace service.domain.Models
{
    public record DoraemonItemCreateDto(
        [Required] string UserId,
        [Required] Guid InputImageId,
        [Required] string InputImageUrl,
        string? PropmtText
        );
}
