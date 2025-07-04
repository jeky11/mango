using Mango.MessageBus;
using Mango.MessageBus.Extensions;
using Mango.MessageBus.MessageBusConsumer;
using Mango.Services.Infrastructure.Extensions;
using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MessageBusConnectionStrings>(builder.Configuration.GetRequiredSection("ConnectionStrings"));
builder.Services.Configure<TopicAndQueueNames>(builder.Configuration.GetRequiredSection(nameof(TopicAndQueueNames)));

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<OrderCreatedRewardsHandler>();
builder.Services.AddSingleton<IMessageBusConsumerFactory, MessageBusConsumerFactory>();
builder.Services.AddSingleton<IMessageBusConsumer>(sp => sp.GetRequiredService<IMessageBusConsumerFactory>().CreateMessageBusConsumer());
builder.Services.AddHostedService<MessageBusConsumerHostedService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reward API");
	c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Services.RegisterMessageHandler<OrderCreatedRewardsHandler>();
app.Services.ApplyMigrations<AppDbContext>();

app.Run();
