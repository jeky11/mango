using System.Text;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
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
	private IConnection? _connection;
	private readonly List<IChannel> _channels = new();

	private async Task RegisterQueueHandlerAsync(
		IConnection connection,
		string exchangeName,
		string queueName,
		Func<IChannel, BasicDeliverEventArgs, Task> handler,
		CancellationToken cancellationToken)
	{
		var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);
		await channel.QueueDeclareAsync(queueName, false, false, false, cancellationToken: cancellationToken);

		if (!string.IsNullOrEmpty(exchangeName))
		{
			await channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, false, false, cancellationToken: cancellationToken);
			await channel.QueueBindAsync(queueName, exchangeName, "", cancellationToken: cancellationToken);
		}

		var consumer = new AsyncEventingBasicConsumer(channel);
		consumer.ReceivedAsync += async (_, eventArgs) => await handler(channel, eventArgs);

		await channel.BasicConsumeAsync(queueName, false, consumer, cancellationToken);

		_channels.Add(channel);
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		var factory = new ConnectionFactory {Uri = _connectionString};
		_connection = factory.CreateConnectionAsync(cancellationToken).GetAwaiter().GetResult();

		await RegisterQueueHandlerAsync(
			_connection, "", _topicAndQueueNames.RegisterUserQueue, OnUserRegisterRequestReceivedAsync, cancellationToken);
		await RegisterQueueHandlerAsync(
			_connection, "", _topicAndQueueNames.EmailShoppingCartQueue, OnEmailCartRequestReceived, cancellationToken);
		await RegisterQueueHandlerAsync(
			_connection, _topicAndQueueNames.OrderCreatedTopic, _topicAndQueueNames.OrderCreatedEmailSubscription,
			OnOrderPlacedRequestReceived, cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		foreach (var channel in _channels)
		{
			await channel.DisposeAsync();
		}

		_channels.Clear();

		if (_connection != null)
		{
			await _connection.DisposeAsync();
		}
	}

	private async Task OnEmailCartRequestReceived(IChannel channel, BasicDeliverEventArgs arg) =>
		await HandleMessageAsync<CartDto, IEmailService>(channel, arg, (service, cart) => service.EmailCartAndLogAsync(cart));

	private async Task OnUserRegisterRequestReceivedAsync(IChannel channel, BasicDeliverEventArgs arg) =>
		await HandleMessageAsync<string, IEmailService>(channel, arg, (service, email) => service.RegisterUserEmailAndLogAsync(email));

	private async Task OnOrderPlacedRequestReceived(IChannel channel, BasicDeliverEventArgs arg) =>
		await HandleMessageAsync<RewardsMessage, IEmailService>(
			channel, arg, (service, rewardsMessage) => service.LogOrderPlacedAsync(rewardsMessage));

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
			await using var scope = scopeFactory.CreateAsyncScope();
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
