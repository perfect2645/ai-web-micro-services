using NetUtils.Aspnet.Repository;
using System.ComponentModel.DataAnnotations;

namespace repository.file.Repositories.Entities
{
    public record UploadedItem(
        [property: MaxLength(512)] string FileName,
        long FileSize,
        string FileHash,
        string BackupUrl,
        string RemoteUrl,
        string? Description
    ) : IEntity, IEntityTiming
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public DateTime CreateTime { get; init; } = DateTime.Now;
        public DateTime? UpdateTime { get; private set; }
    }
}
