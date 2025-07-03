using System.Text;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;

namespace Mango.MessageBus;

public class MessageBus(string connectionString) : IMessageBus
{
	private readonly string _connectionString = connectionString;

	public async Task PublishMessageAsync(object message, string topicOrQueueName, CancellationToken cancellationToken = default)
	{
		await using var client = new ServiceBusClient(_connectionString);

		await using var sender = client.CreateSender(topicOrQueueName);

		var jsonMessage = JsonConvert.SerializeObject(message);
		var finalMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(jsonMessage)) {CorrelationId = Guid.NewGuid().ToString()};

		await sender.SendMessageAsync(finalMessage, cancellationToken);
	}
}
