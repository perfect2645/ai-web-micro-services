namespace service.messaging.Configurations
{
    public record SignalRSettings
    {
        public required string Group { get; init; }
    }
}
