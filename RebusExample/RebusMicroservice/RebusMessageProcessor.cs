using Rebus.Handlers;
using Rebus.Bus;
using System.Threading.Tasks;

public class RebusMessageProcessor : IHandleMessages<InputMessage>
{
    private readonly IBus _bus;

    public RebusMessageProcessor(IBus bus)
    {
        _bus = bus;
    }

    public async Task Handle(InputMessage message)
    {
        var processed = new ProcessedMessage
        {
            Text = $"{message.Text}-processed"
        };

        await _bus.Send(processed);
    }
}
