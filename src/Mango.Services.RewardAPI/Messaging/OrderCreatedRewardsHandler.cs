using Mango.MessageBus.MessageBusConsumer;
using Mango.Services.RewardAPI.Message;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.RewardAPI.Messaging;

public class OrderCreatedRewardsHandler(IOptions<TopicAndQueueNames> topicAndQueueNames, IRewardService rewardService) : IMessageBusHandler
{
	private readonly IRewardService _rewardService = rewardService;

	public string QueueName => string.Empty;
	public string TopicName => topicAndQueueNames.Value.OrderCreatedTopic;
	public string SubscriptionName => topicAndQueueNames.Value.OrderCreatedRewardsSubscription;

	public async Task HandleAsync(string message)
	{
		var rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(message) ?? throw new NullReferenceException();
		await _rewardService.UpdateRewardsAsync(rewardsMessage);
	}
}
