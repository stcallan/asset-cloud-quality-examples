using System.Linq;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace RabbitMqExample.Tests
{
    public class CustomWebApplicationFactory<TProgram>(RabbitMqTestContainerFixture fixture) : WebApplicationFactory<TProgram> where TProgram : class
    {
        private readonly RabbitMqTestContainerFixture _fixture = fixture;

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove existing RabbitMQ connection registration
                var connectionDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(RabbitMQ.Client.IConnection));
                if (connectionDescriptor != null)
                    services.Remove(connectionDescriptor);

                // Register the RabbitMQ connection from the test fixture
                services.AddSingleton(_fixture.RabbitMqConnection);

                // Remove and register RabbitMqQueueClient
                var clientDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(RabbitMqQueueClient));
                if (clientDescriptor != null)
                    services.Remove(clientDescriptor);

                // Register RabbitMqQueueClient with the connection string from the test container
                services.AddSingleton(new RabbitMqQueueClient(_fixture.Container.GetConnectionString()));
            });
        }
    }
}
