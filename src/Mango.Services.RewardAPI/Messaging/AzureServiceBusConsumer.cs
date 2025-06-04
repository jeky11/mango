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
	private readonly string _connectionString;
	private readonly string _orderCreatedTopic;
	private readonly string _orderCreatedRewardSubscription;

	private readonly ServiceBusProcessor _rewardProcessor;

	private readonly IServiceScopeFactory _scopeFactory;

	public AzureServiceBusConsumer(
		IOptions<ConnectionStrings> connectionStrings,
		IOptions<TopicAndQueueNames> topicAndQueueNames,
		IServiceScopeFactory scopeFactory)
	{
		_connectionString = connectionStrings.Value.MessageBusConnection;
		_orderCreatedTopic = topicAndQueueNames.Value.OrderCreatedTopic;
		_orderCreatedRewardSubscription = topicAndQueueNames.Value.OrderCreatedRewardsSubscription;
		_scopeFactory = scopeFactory;

		var client = new ServiceBusClient(_connectionString);
		_rewardProcessor = client.CreateProcessor(_orderCreatedTopic, _orderCreatedRewardSubscription);
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

	private async Task OnNewOrderRewardsRequestReceived(ProcessMessageEventArgs arg)
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		var objMessage = JsonConvert.DeserializeObject<RewardsMessage>(body) ?? throw new NullReferenceException();
		try
		{
			await using var scope = _scopeFactory.CreateAsyncScope();
			var rewardService = scope.ServiceProvider.GetRequiredService<IRewardService>();
			await rewardService.UpdateRewardsAsync(objMessage);

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
