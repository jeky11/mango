using RabbitMQ.Client;

namespace Mango.Services.AuthAPI.RabbitMQSender;

public class RabbitMqAuthSender(string connectionString) : IRabbitMqAuthSender
{
	private readonly Uri _connectionString = new Uri(connectionString);

	public Task SendMessageAsync(object message, string queueName)
	{
		var factory = new ConnectionFactory {Uri = _connectionString};

		throw new NotImplementedException();
	}
}
