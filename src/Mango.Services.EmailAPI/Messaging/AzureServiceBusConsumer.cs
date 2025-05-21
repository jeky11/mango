using System.Text;
using Azure.Messaging.ServiceBus;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Models.Dto;
using Mango.Services.EmailAPI.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Mango.Services.EmailAPI.Messaging;

public class AzureServiceBusConsumer : IAzureServiceBusConsumer
{
	private readonly string _connectionString;
	private readonly string _emailCartQueue;
	private readonly ServiceBusProcessor _emailCartProcessor;
	private readonly IServiceScopeFactory _scopeFactory;

	public AzureServiceBusConsumer(
		IOptions<ConnectionStrings> connectionStrings,
		IOptions<TopicAndQueueNames> topicAndQueueNames,
		IServiceScopeFactory scopeFactory)
	{
		_connectionString = connectionStrings.Value.MessageBusConnection;
		_emailCartQueue = topicAndQueueNames.Value.EmailShoppingCartQueue;
		_scopeFactory = scopeFactory;

		var client = new ServiceBusClient(_connectionString);
		_emailCartProcessor = client.CreateProcessor(_emailCartQueue);
	}

	public async Task StartAsync()
	{
		_emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
		_emailCartProcessor.ProcessErrorAsync += ErrorHandler;
		await _emailCartProcessor.StartProcessingAsync();
	}

	public async Task StopAsync()
	{
		await _emailCartProcessor.StopProcessingAsync();
		await _emailCartProcessor.DisposeAsync();
	}

	private async Task OnEmailCartRequestReceived(ProcessMessageEventArgs arg)
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		var objMessage = JsonConvert.DeserializeObject<CartDto>(body) ?? throw new NullReferenceException();
		try
		{
			await using var scope = _scopeFactory.CreateAsyncScope();
			var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
			await emailService.EmailCartAndLogAsync(objMessage);

			await arg.CompleteMessageAsync(arg.Message);
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	private Task ErrorHandler(ProcessErrorEventArgs arg)
	{
		Console.WriteLine(arg.Exception.ToString());
		return Task.CompletedTask;
	}
}
