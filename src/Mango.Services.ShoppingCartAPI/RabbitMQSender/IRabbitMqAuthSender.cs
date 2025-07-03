namespace Mango.Services.ShoppingCartAPI.RabbitMQSender;

public interface IRabbitMqAuthSender
{
	Task PublishMessageAsync(object message, string queueName, CancellationToken cancellationToken = default);
}
