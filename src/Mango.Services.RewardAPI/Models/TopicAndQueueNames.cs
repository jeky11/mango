namespace Mango.Services.RewardAPI.Models;

public class TopicAndQueueNames
{
	public required string OrderCreatedTopic { get; set; }
	public required string OrderCreatedRewardsSubscription { get; set; }
}
