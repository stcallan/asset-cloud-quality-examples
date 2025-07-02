
public class ServiceBusSettings
{
    public required string ConnectionString { get; init; }
    public required string InputQueue { get; init; }
    public required string OutputQueue { get; init; }
}

