namespace Mango.MessageBus.MessageBusSender;

public interface IMessageBusSender
{
	Task PublishMessageToQueueAsync(object message, string queueName, CancellationToken cancellationToken = default);
	Task PublishMessageToTopicAsync(object message, string topicName, CancellationToken cancellationToken = default);
}
