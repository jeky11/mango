namespace Mango.Services.AuthAPI.RabbitMQSender;

public interface IRabbitMqAuthSender
{
	Task SendMessageAsync(object message, string queueName);
}
