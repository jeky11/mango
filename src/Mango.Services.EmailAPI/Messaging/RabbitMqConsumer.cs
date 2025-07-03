using Mango.Services.EmailAPI.Models;
using Microsoft.Extensions.Options;

namespace Mango.Services.EmailAPI.Messaging;

public class RabbitMqConsumer : IRabbitMqConsumer
{
	private readonly IServiceScopeFactory _scopeFactory;

	public RabbitMqConsumer(
		IOptions<ConnectionStrings> connectionStrings,
		IOptions<TopicAndQueueNames> topicAndQueueNames,
		IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}
}
