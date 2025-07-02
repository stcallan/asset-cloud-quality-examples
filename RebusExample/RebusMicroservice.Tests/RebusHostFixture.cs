using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using Rebus.Transport.InMem;
using System.Threading.Tasks;
using Xunit;

public class RebusHostFixture : IAsyncLifetime
{
    public InMemNetwork Network { get; private set; } = null!;
    public IHost host { get; private set; } = null!;
    public IBus HostBus { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Network = new InMemNetwork();

        host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddRebus(cfg => cfg
                    .Transport(t => t.UseInMemoryTransport(Network, "input-queue"))
                    .Routing(r => r.TypeBased()
                        .Map<InputMessage>("input-queue")
                        .Map<ProcessedMessage>("output-queue"))
                );
                services.AddTransient<IHandleMessages<InputMessage>, RebusMessageProcessor>();
            })
            .Build();

        await host.StartAsync();
        HostBus = host.Services.GetRequiredService<IBus>();
    }

    public async Task DisposeAsync()
    {
        if (host != null)
        {
            await host.StopAsync();
            host.Dispose();
        }
    }
}
