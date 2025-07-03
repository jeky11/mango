namespace Mango.Services.ShoppingCartAPI.RabbitMQSender;

public interface IRabbitMqSender
{
	Task PublishMessageAsync(object message, string queueName, CancellationToken cancellationToken = default);
}
