namespace Mango.Services.OrderAPI.RabbitMQSender;

public interface IRabbitMqSender
{
	Task PublishMessageAsync(object message, string exhangeName, CancellationToken cancellationToken = default);
}
