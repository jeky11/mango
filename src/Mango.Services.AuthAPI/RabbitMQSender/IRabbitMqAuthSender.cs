namespace Mango.Services.AuthAPI.RabbitMQSender;

public interface IRabbitMqAuthSender
{
	Task PublishMessageAsync(object message, string queueName);
}
