namespace Mango.MessageBus;

public interface IMessageBus
{
	Task PublishMessageAsync(object message, string topicOrQueueName, CancellationToken cancellationToken = default);
}
