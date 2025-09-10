namespace RabbitMqExample;

public static class RabbitMqQueueNames
{
    public static readonly string InputQueue;
    public static readonly string OutputQueue;
    public static readonly string ApiQueue;

    static RabbitMqQueueNames()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        InputQueue = config["RabbitMq:InputQueue"] ?? "input-queue";
        OutputQueue = config["RabbitMq:OutputQueue"] ?? "output-queue";
        ApiQueue = config["RabbitMq:ApiQueue"] ?? "api-queue";
    }
}
