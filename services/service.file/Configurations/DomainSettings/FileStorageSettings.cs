namespace service.file.Configurations.DomainSettings
{
    public record FileStorageSettings()
    {
        public required string RemotePath { get; set; }
        public required string BackupPath { get; set; }
        public required string CloudPath { get; set; }
        public required double MaxFileSizeMB { get; set; }
    }
}
