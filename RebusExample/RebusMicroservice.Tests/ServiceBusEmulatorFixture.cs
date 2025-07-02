using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testcontainers.ServiceBus;
using Xunit;


public class ServiceBusEmulatorFixture : IAsyncLifetime
{
    public ServiceBusContainer ServiceBusContainer { get; private set; }
    public ServiceBusClient ServiceBusClient { get; private set; }

    public string InputQueue = "input-queue";
    public string OutputQueue = "output-queue";

    private IHost _host;

    private string _connectionString => ServiceBusContainer?.GetConnectionString()
        ?? throw new InvalidOperationException("Service Bus container is not started yet.");

    public async Task InitializeAsync()
    {
        ServiceBusContainer = new ServiceBusBuilder()
            .WithImage("mcr.microsoft.com/azure-messaging/servicebus-emulator:latest")
            .WithAcceptLicenseAgreement(true)
            .WithConfig("config.json")
            .Build();

        await ServiceBusContainer.StartAsync();

        ServiceBusClient = new ServiceBusClient(_connectionString);

        _host = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(config =>
            {
                var testSettings = new Dictionary<string, string>
                {
                    ["ServiceBus:ConnectionString"] = _connectionString,
                    ["ServiceBus:InputQueue"] = InputQueue,
                    ["ServiceBus:OutputQueue"] = OutputQueue,
                };
                config.AddInMemoryCollection(testSettings);
            })
            .ConfigureServices((context, services) =>
            {
                services.Configure<ServiceBusSettings>(context.Configuration.GetSection("ServiceBus"));
                services.AddHostedService<ServiceBusMessageProcessor>();
            })
            .Build();

        await _host.StartAsync();
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        if (ServiceBusContainer != null)
        {
            await ServiceBusContainer.StopAsync();
            await ServiceBusContainer.DisposeAsync();
        }
    }
}
