using Mango.MessageBus.MessageBusConsumer;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.EmailAPI.Messaging;

public class EmailShoppingCartHandler(IOptions<TopicAndQueueNames> topicAndQueueNames, IEmailService emailService) : IMessageBusHandler
{
	private readonly IEmailService _emailService = emailService;

	public string QueueName => topicAndQueueNames.Value.EmailShoppingCartQueue;
	public string TopicName => string.Empty;
	public string SubscriptionName => string.Empty;

	public async Task HandleAsync(string message)
	{
		var cart = JsonConvert.DeserializeObject<CartDto>(message) ?? throw new NullReferenceException();
		await _emailService.EmailCartAndLogAsync(cart);
	}
}
