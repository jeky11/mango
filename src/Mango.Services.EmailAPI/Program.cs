using Mango.MessageBus;
using Mango.MessageBus.Extensions;
using Mango.MessageBus.MessageBusConsumer;
using Mango.Services.EmailAPI.Data;
using Mango.Services.EmailAPI.Messaging;
using Mango.Services.EmailAPI.Models;
using Mango.Services.EmailAPI.Services;
using Mango.Services.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MessageBusConnectionStrings>(builder.Configuration.GetRequiredSection("ConnectionStrings"));
builder.Services.Configure<TopicAndQueueNames>(builder.Configuration.GetRequiredSection(nameof(TopicAndQueueNames)));

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<EmailShoppingCartHandler>();
builder.Services.AddScoped<OrderCreatedEmailHandler>();
builder.Services.AddScoped<RegisterUserEmailHandler>();
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
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Email API");
	c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Services.RegisterMessageHandler<EmailShoppingCartHandler>();
app.Services.RegisterMessageHandler<OrderCreatedEmailHandler>();
app.Services.RegisterMessageHandler<RegisterUserEmailHandler>();

app.Services.ApplyMigrations<AppDbContext>();

app.Run();
