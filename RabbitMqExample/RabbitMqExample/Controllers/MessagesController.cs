using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Text;

namespace RabbitMqExample.Controllers
{
    [ApiController]
    [Route("api/[controller]")] 
    public class MessagesController(IConnection connection) : ControllerBase
    {
        private readonly IConnection _connection = connection;

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] string message)
        { 
            await using var channel = await _connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: RabbitMqQueueNames.ApiQueue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: RabbitMqQueueNames.ApiQueue,
                body: body.AsMemory() 
            );

            return Ok();
        }
    }
}
