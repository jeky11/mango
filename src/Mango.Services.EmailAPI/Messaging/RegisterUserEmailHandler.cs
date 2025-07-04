using Mango.MessageBus.MessageBusConsumer;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.EmailAPI.Messaging;

public class RegisterUserEmailHandler(IOptions<TopicAndQueueNames> topicAndQueueNames, IEmailService emailService) : IMessageBusHandler
{
	private readonly IEmailService _emailService = emailService;

	public string QueueName => topicAndQueueNames.Value.RegisterUserQueue;
	public string TopicName => string.Empty;
	public string SubscriptionName => string.Empty;

	public async Task HandleAsync(string message)
	{
		var email = JsonConvert.DeserializeObject<string>(message) ?? throw new NullReferenceException();
		await _emailService.RegisterUserEmailAndLogAsync(email);
	}
}
