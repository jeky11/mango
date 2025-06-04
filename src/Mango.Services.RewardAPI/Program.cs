using Mango.Services.Infrastructure.Extensions;
using Mango.Services.RewardAPI.Data;
using Mango.Services.RewardAPI.Messaging;
using Mango.Services.RewardAPI.Models;
using Mango.Services.RewardAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetRequiredSection(nameof(ConnectionStrings)));
builder.Services.Configure<TopicAndQueueNames>(builder.Configuration.GetRequiredSection(nameof(TopicAndQueueNames)));

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddSingleton<IAzureServiceBusConsumer, AzureServiceBusConsumer>();
builder.Services.AddHostedService<AzureServiceBusHostedService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Services.ApplyMigrations<AppDbContext>();

app.Run();
