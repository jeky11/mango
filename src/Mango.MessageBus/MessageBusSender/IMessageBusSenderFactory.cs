namespace Mango.MessageBus.MessageBusSender;

public interface IMessageBusSenderFactory
{
	IMessageBusSender CreateMessageBusSender();
}
