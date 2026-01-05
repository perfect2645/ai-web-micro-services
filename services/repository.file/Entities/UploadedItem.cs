using NetUtils.Aspnet.Repository;
using System.ComponentModel.DataAnnotations;

namespace repository.file.Repositories.Entities
{
    public record UploadedItem : IEntity, IEntityTiming
    {
        public Guid Id { get; init; }
        public required DateTime CreateTime {  get; init; }
        public DateTime? UpdateTime { get; private set; }

        [MaxLength(512)]
        public required string FileName { get; init; }
        public required long FileSize { get; init; }
        /// <summary>
        /// SHA256 hash of the file content, represented as a hexadecimal string.
        /// </summary>
        public required string FileHash { get; init; }
        public required string BackupUrl { get; init; }
        public required string RemoteUrl { get; init; }
        public string? Description { get; init; }

        public UploadedItem(string fileName, long fileSize, string fileHash, string backupUrl, string remoteUrl, string? description)
        {
            Id = Guid.NewGuid();
            CreateTime = DateTime.Now;
            FileName = fileName;
            FileSize = fileSize;
            FileHash = fileHash;
            BackupUrl = backupUrl;
            RemoteUrl = remoteUrl;
            Description = description;
        }
    }
}
