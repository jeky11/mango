using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.RewardAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
	private readonly ServiceBusProcessor _rewardProcessor;

	private readonly IServiceScopeFactory _scopeFactory;

	public AzureServiceBusConsumer(
		IOptions<ConnectionStrings> connectionStrings,
		IOptions<TopicAndQueueNames> topicAndQueueNames,
		IServiceScopeFactory scopeFactory)
	{
		_scopeFactory = scopeFactory;

		var client = new ServiceBusClient(connectionStrings.Value.MessageBusConnection);
		_rewardProcessor = client.CreateProcessor(
			topicAndQueueNames.Value.OrderCreatedTopic, topicAndQueueNames.Value.OrderCreatedRewardsSubscription);
	}

	public async Task StartAsync()
	{
		_rewardProcessor.ProcessMessageAsync += OnNewOrderRewardsRequestReceived;
		_rewardProcessor.ProcessErrorAsync += ErrorHandler;
		await _rewardProcessor.StartProcessingAsync();
	}

	public async Task StopAsync()
	{
		await _rewardProcessor.StopProcessingAsync();
		await _rewardProcessor.DisposeAsync();
	}

	private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs arg) =>
		await HandleMessageAsync<RewardsMessage, IRewardService>(arg, (service, rewardsMessage) => service.UpdateRewardsAsync(rewardsMessage));

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
