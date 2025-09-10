using RabbitMQ.Client;

namespace RabbitMqExample;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Register RabbitMQ connection factory (will be overridden in tests)
        services.AddSingleton<IConnectionFactory>(new ConnectionFactory
        {
            HostName = "localhost",
            UserName = "guest",
            Password = "guest"
        });

        // Register RabbitMQ connection (synchronous)
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = sp.GetRequiredService<IConnectionFactory>();
            return factory.CreateConnectionAsync().GetAwaiter().GetResult();
        });

        // Register RabbitMqQueueClient
        services.AddSingleton(sp =>
        {
            var connection = sp.GetRequiredService<IConnection>();

            // Use the same connection string as the connection
            var factory = sp.GetRequiredService<IConnectionFactory>();
            var connectionFactory = factory as ConnectionFactory;
            var uri = connectionFactory?.Uri?.ToString() 
                ?? $"amqp://{connectionFactory?.UserName}:{connectionFactory?.Password}@{connectionFactory?.HostName}:{connectionFactory?.Port}";
            return new RabbitMqQueueClient(uri);
        });

        services.AddHostedService<RabbitMqMessageProcessor>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
