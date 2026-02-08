namespace service.file.Configurations.DomainSettings
{
    public record FileStorageSettings()
    {
        public required string LocalRootPath { get; set; }
        public required string RemotePath { get; set; }
        public required string BackupPath { get; set; }
        public required string CloudPath { get; set; }
        public required string RequestPath { get; set; }
        public required string ApiBaseUrl { get; set; }
        public required double MaxFileSizeMB { get; set; }
    }
}
