using Mango.MessageBus.MessageBusConsumer;
using Mango.Services.EmailAPI.Message;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.EmailAPI.Messaging;

public class OrderCreatedEmailHandler(IOptions<TopicAndQueueNames> topicAndQueueNames, IEmailService emailService) : IMessageBusHandler
{
	private readonly IEmailService _emailService = emailService;

	public string QueueName => string.Empty;
	public string TopicName => topicAndQueueNames.Value.OrderCreatedTopic;
	public string SubscriptionName => topicAndQueueNames.Value.OrderCreatedEmailSubscription;

	public async Task HandleAsync(string message)
	{
		var rewardsMessage = JsonConvert.DeserializeObject<RewardsMessage>(message) ?? throw new NullReferenceException();
		await _emailService.LogOrderPlacedAsync(rewardsMessage);
	}
}
