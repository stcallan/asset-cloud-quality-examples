using RabbitMQ.Client;

namespace RabbitMqExample;

public interface IRabbitMqConnectionFactory
{
    ConnectionFactory CreateFactory();
}

public class RabbitMqConnectionFactory : IRabbitMqConnectionFactory
{
    private readonly IConfiguration _config;
    public RabbitMqConnectionFactory(IConfiguration config) => _config = config;
    public ConnectionFactory CreateFactory() =>
        new ConnectionFactory
        {
            HostName = _config["RabbitMq:HostName"] ?? "localhost",
            Port = int.TryParse(_config["RabbitMq:Port"], out var port) ? port : 5672,
            UserName = _config["RabbitMq:UserName"] ?? "guest",
            Password = _config["RabbitMq:Password"] ?? "guest"
        };
}
