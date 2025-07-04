namespace Mango.MessageBus.MessageBusConsumer;

public interface IMessageHandler
{
	string QueueName { get; }
	string TopicName { get; }
	string SubscriptionName { get; }

	Task HandleAsync(string message);
}
