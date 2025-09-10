using RabbitMQ.Client;
using System;
using System.Threading.Tasks;
using Testcontainers.RabbitMq;
using Xunit;

namespace RabbitMqExample.Tests;

public class RabbitMqTestContainerFixture : IAsyncLifetime
{
    public RabbitMqContainer Container { get; }
    public IConnection RabbitMqConnection { get; set; }
    public RabbitMqQueueClient QueueClient { get; private set; }

    public RabbitMqTestContainerFixture()
    {
        Container = new RabbitMqBuilder()
            .WithImage("rabbitmq:3-management")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await Container.StartAsync();

        var factory = new ConnectionFactory { Uri = new Uri(Container.GetConnectionString()) };
        RabbitMqConnection = await factory.CreateConnectionAsync();
 
        QueueClient = new RabbitMqQueueClient(Container.GetConnectionString());
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        RabbitMqConnection?.Dispose();
    }
}