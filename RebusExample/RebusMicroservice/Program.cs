using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Routing.TypeBased;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args);

        builder.ConfigureServices((context, services) =>
        {
            var config = context.Configuration;

            var connectionString = config["AzureServiceBus:ConnectionString"];
            var inputQueue = config["AzureServiceBus:InputQueue"];
            var outputQueue = config["AzureServiceBus:OutputQueue"];

            services.AddRebus(
                configure => configure
                    .Transport(t => t.UseAzureServiceBus(connectionString, inputQueue))
                    .Routing(r => r.TypeBased().Map<ProcessedMessage>(outputQueue))
            );

            services.AddTransient<IHandleMessages<InputMessage>, RebusMessageProcessor>();

            services.AddHostedService<ServiceBusMessageProcessor>();

        });

        var app = builder.Build();

        app.Services.StartRebus();

        await app.RunAsync();
    }
}
