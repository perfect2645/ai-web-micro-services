namespace service.file.Configurations.DomainSettings
{
    public record FileStorageSettings(string StoragePath, string CloudPath, double MaxFileSizeMB);
}
