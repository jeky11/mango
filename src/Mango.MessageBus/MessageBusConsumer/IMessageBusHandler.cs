namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageBusHandler
{
	string QueueName { get; }
	string TopicName { get; }
	string SubscriptionName { get; }

	Task HandleAsync(string message);
}
