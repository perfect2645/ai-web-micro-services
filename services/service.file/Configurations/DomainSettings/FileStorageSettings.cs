namespace service.file.Configurations.DomainSettings
{
    public record FileStorageSettings(string RemotePath, string BackupPath, string CloudPath, double MaxFileSizeMB);
}
