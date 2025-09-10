using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

public class RabbitMqQueueClient
{
    private readonly string _connectionString;

    public RabbitMqQueueClient(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<string?> ConsumeMessageAsync(string queueName)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queueName, false, false, false, null);

        var result = await channel.BasicGetAsync(queueName, autoAck: true);
        if (result == null) return null;

        var body = result.Body.ToArray();
        return Encoding.UTF8.GetString(body);
    }

    public async Task PublishAsync(string queueName, string message)
    {
        var factory = new ConnectionFactory { Uri = new Uri(_connectionString) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync();

        await channel.QueueDeclareAsync(queueName, false, false, false, null);

        var bytes = Encoding.UTF8.GetBytes(message);
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body: bytes.AsMemory(),
            cancellationToken: default
        );
    }
}