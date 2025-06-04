using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
	private readonly ServiceBusProcessor _emailCartProcessor;
	private readonly ServiceBusProcessor _registerUserProcessor;
	private readonly ServiceBusProcessor _emailOrderPlacedProcessor;

	private readonly IServiceScopeFactory _scopeFactory;

	public AzureServiceBusConsumer(
		IOptions<ConnectionStrings> connectionStrings,
		IOptions<TopicAndQueueNames> topicAndQueueNames,
		IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;

		var client = new ServiceBusClient(connectionStrings.Value.MessageBusConnection);
		_emailCartProcessor = client.CreateProcessor(topicAndQueueNames.Value.EmailShoppingCartQueue);
		_registerUserProcessor = client.CreateProcessor(topicAndQueueNames.Value.RegisterUserQueue);
		_emailOrderPlacedProcessor = client.CreateProcessor(
			topicAndQueueNames.Value.OrderCreatedTopic, topicAndQueueNames.Value.OrderCreatedEmailSubscription);
	}

	public async Task StartAsync()
	{
		_emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
		_emailCartProcessor.ProcessErrorAsync += ErrorHandler;
		await _emailCartProcessor.StartProcessingAsync();

		_registerUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
		_registerUserProcessor.ProcessErrorAsync += ErrorHandler;
		await _registerUserProcessor.StartProcessingAsync();

		_emailOrderPlacedProcessor.ProcessMessageAsync += OnOrderPlacedRequestReceived;
		_emailOrderPlacedProcessor.ProcessErrorAsync += ErrorHandler;
		await _emailOrderPlacedProcessor.StartProcessingAsync();
	}

	public async Task StopAsync()
	{
		await _emailCartProcessor.StopProcessingAsync();
		await _emailCartProcessor.DisposeAsync();

		await _registerUserProcessor.StopProcessingAsync();
		await _registerUserProcessor.DisposeAsync();

		await _emailOrderPlacedProcessor.StopProcessingAsync();
		await _emailOrderPlacedProcessor.DisposeAsync();
	}

	private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs arg) =>
		await HandleMessageAsync<CartDto, IEmailService>(arg, (service, cart) => service.EmailCartAndLogAsync(cart));

	private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs arg) =>
		await HandleMessageAsync<string, IEmailService>(arg, (service, email) => service.RegisterUserEmailAndLogAsync(email));

	private async Task OnOrderPlacedRequestReceived(ProcessMessageEventArgs arg) =>
		await HandleMessageAsync<RewardsMessage, IEmailService>(arg, (service, rewardsMessage) => service.LogOrderPlacedAsync(rewardsMessage));

	private async Task HandleMessageAsync<TMessage, TService>(ProcessMessageEventArgs arg, Func<TService, TMessage, Task> handler)
		where TService : notnull
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		var objMessage = JsonConvert.DeserializeObject<TMessage>(body) ?? throw new NullReferenceException();
		try
		{
			await using var scope = _scopeFactory.CreateAsyncScope();
			var service = scope.ServiceProvider.GetRequiredService<TService>();
			await handler(service, objMessage);

			await arg.CompleteMessageAsync(arg.Message);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	private Task ErrorHandler(ProcessErrorEventArgs arg)
	{
		Console.WriteLine(arg.Exception.ToString());
		return Task.CompletedTask;
	}
}
