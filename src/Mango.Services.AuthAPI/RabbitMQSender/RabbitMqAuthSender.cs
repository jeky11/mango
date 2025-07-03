using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Mango.Services.AuthAPI.RabbitMQSender;

public class RabbitMqAuthSender(string connectionString) : IRabbitMqAuthSender
{
	private readonly Uri _connectionString = new(connectionString);

	public async Task PublishMessageAsync(object message, string queueName)
	{
		var factory = new ConnectionFactory {Uri = _connectionString};

		var connection = await factory.CreateConnectionAsync();

		await using var channel = await connection.CreateChannelAsync();
		await channel.QueueDeclareAsync(queueName);
		var jsonMessage = JsonConvert.SerializeObject(message);
		var body = Encoding.UTF8.GetBytes(jsonMessage);
		await channel.BasicPublishAsync("", queueName, body);
	}
}
