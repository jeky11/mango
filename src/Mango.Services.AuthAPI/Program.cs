using Mango.MessageBus;
using Mango.MessageBus.MessageBusSender;
using Mango.Services.AuthAPI.Data;
using Mango.Services.AuthAPI.Models;
using Mango.Services.AuthAPI.Service;
using Mango.Services.AuthAPI.Service.IService;
using Mango.Services.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<MessageBusConnectionStrings>(builder.Configuration.GetRequiredSection("ConnectionStrings"));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetRequiredSection("ApiSettings:JwtOptions"));
builder.Services.Configure<TopicAndQueueNames>(builder.Configuration.GetRequiredSection(nameof(TopicAndQueueNames)));

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
	.AddEntityFrameworkStores<AppDbContext>()
	.AddDefaultTokenProviders();

builder.Services.AddControllers();
builder.Services.AddSingleton<IMessageBusSenderFactory, MessageBusSenderFactory>();
builder.Services.AddSingleton<IMessageBusSender>(sp => sp.GetRequiredService<IMessageBusSenderFactory>().CreateMessageBusSender());
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API");
	c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Services.ApplyMigrations<AppDbContext>();

app.Run();
