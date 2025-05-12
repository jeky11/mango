using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models;
using Microsoft.Extensions.Options;

namespace Mango.Services.EmailAPI.Messaging;

public interface IAzureServiceBusConsumer
{ }

public class AzureServiceBusConsumer
	: IAzureServiceBusConsumer
{
	private readonly string _connectionString;
	private readonly string _emailCartQueue;
	private ServiceBusProcessor _emailCartProcessor;

	public AzureServiceBusConsumer(IOptions<ConnectionStrings> connectionStrings, IOptions<TopicAndQueueNames> topicAndQueueNames)
	{
		_connectionString = connectionStrings.Value.MessageBusConnection;
		_emailCartQueue = topicAndQueueNames.Value.EmailShoppingCartQueue;

		var client = new ServiceBusClient(_connectionString);
		_emailCartProcessor = client.CreateProcessor(_emailCartQueue);
	}
}
