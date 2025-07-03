using System.Text;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mango.Services.EmailAPI.Messaging;

public class RabbitMqConsumer(
	IOptions<ConnectionStrings> connectionStrings,
	IOptions<TopicAndQueueNames> topicAndQueueNames,
	IServiceScopeFactory scopeFactory)
	: IRabbitMqConsumer
{
	private readonly Uri _connectionString = new(connectionStrings.Value.RabbitMQConnection);
	private readonly TopicAndQueueNames _topicAndQueueNames = topicAndQueueNames.Value;
	private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
	private IChannel? _channel;

	private async Task<IChannel> CreateChannelAsync(string queueName, CancellationToken cancellationToken)
	{
		var factory = new ConnectionFactory {Uri = _connectionString};

		var connection = await factory.CreateConnectionAsync(cancellationToken);

		var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
		await channel.QueueDeclareAsync(queueName, false, false, false, cancellationToken: cancellationToken);
		return channel;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_channel = await CreateChannelAsync(_topicAndQueueNames.RegisterUserQueue, cancellationToken);

		var consumer = new AsyncEventingBasicConsumer(_channel);
		consumer.ReceivedAsync += async (_, eventArgs) => await OnUserRegisterRequestReceivedAsync(_channel, eventArgs);

		await _channel.BasicConsumeAsync(_topicAndQueueNames.RegisterUserQueue, false, consumer, cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	private async Task OnUserRegisterRequestReceivedAsync(IChannel channel, BasicDeliverEventArgs arg) =>
		await HandleMessageAsync<string, IEmailService>(channel, arg, (service, email) => service.RegisterUserEmailAndLogAsync(email));

	private async Task HandleMessageAsync<TMessage, TService>(
		IChannel channel,
		BasicDeliverEventArgs eventArgs,
		Func<TService, TMessage, Task> handler)
		where TService : notnull
	{
		var content = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
		var objMessage = JsonConvert.DeserializeObject<TMessage>(content) ?? throw new NullReferenceException();
		try
		{
			await using var scope = _scopeFactory.CreateAsyncScope();
			var service = scope.ServiceProvider.GetRequiredService<TService>();
			await handler(service, objMessage);

			await channel.BasicAckAsync(eventArgs.DeliveryTag, false);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}
}
