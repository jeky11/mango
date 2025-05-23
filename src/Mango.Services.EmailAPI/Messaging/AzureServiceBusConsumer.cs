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
	private readonly string _registerUserQueue;

	private readonly ServiceBusProcessor _emailCartProcessor;
	private readonly ServiceBusProcessor _registerUserProcessor;

	private readonly IServiceScopeFactory _scopeFactory;

	public AzureServiceBusConsumer(
		IOptions<ConnectionStrings> connectionStrings,
		IOptions<TopicAndQueueNames> topicAndQueueNames,
		IServiceScopeFactory scopeFactory)
	{
		_connectionString = connectionStrings.Value.MessageBusConnection;
		_emailCartQueue = topicAndQueueNames.Value.EmailShoppingCartQueue;
		_registerUserQueue = topicAndQueueNames.Value.RegisterUserQueue;
		_scopeFactory = scopeFactory;

		var client = new ServiceBusClient(_connectionString);
		_emailCartProcessor = client.CreateProcessor(_emailCartQueue);
		_registerUserProcessor = client.CreateProcessor(_registerUserQueue);
	}

	public async Task StartAsync()
	{
		_emailCartProcessor.ProcessMessageAsync += OnEmailCartRequestReceived;
		_emailCartProcessor.ProcessErrorAsync += ErrorHandler;
		await _emailCartProcessor.StartProcessingAsync();

		_registerUserProcessor.ProcessMessageAsync += OnUserRegisterRequestReceived;
		_registerUserProcessor.ProcessErrorAsync += ErrorHandler;
		await _registerUserProcessor.StartProcessingAsync();
	}

	public async Task StopAsync()
	{
		await _emailCartProcessor.StopProcessingAsync();
		await _emailCartProcessor.DisposeAsync();

		await _registerUserProcessor.StopProcessingAsync();
		await _registerUserProcessor.DisposeAsync();
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

	private async Task OnUserRegisterRequestReceived(ProcessMessageEventArgs arg)
	{
		var message = arg.Message;
		var body = Encoding.UTF8.GetString(message.Body);

		var email = JsonConvert.DeserializeObject<string>(body) ?? throw new NullReferenceException();
		try
		{
			await using var scope = _scopeFactory.CreateAsyncScope();
			var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
			await emailService.RegisterUserEmailAndLogAsync(email);

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
