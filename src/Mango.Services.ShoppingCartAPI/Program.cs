using Mango.MessageBus;
using Mango.Services.Infrastructure;
using Mango.Services.Infrastructure.Extensions;
using Mango.Services.Infrastructure.Models;
using Mango.Services.ShoppingCartAPI;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.RabbitMQSender;
using Mango.Services.ShoppingCartAPI.Service;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var messageBusConnection = builder.Configuration.GetConnectionString("MessageBusConnection") ?? throw new NullReferenceException();
var rabbitMqConnection = builder.Configuration.GetConnectionString("RabbitMQConnection") ?? throw new NullReferenceException();
var serviceUrls = builder.Configuration.GetRequiredSection(nameof(ServiceUrls)).Get<ServiceUrls>() ?? throw new NullReferenceException();
var jwtOptions = builder.Configuration.GetRequiredSection(nameof(JwtOptions)).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.Configure<TopicAndQueueNames>(builder.Configuration.GetRequiredSection(nameof(TopicAndQueueNames)));

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IMessageBus, MessageBus>(_ => new MessageBus(messageBusConnection));
builder.Services.AddSingleton<IRabbitMqAuthSender, RabbitMqAuthSender>(_ => new RabbitMqAuthSender(rabbitMqConnection));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddHttpClient("Product", client => client.BaseAddress = new Uri(serviceUrls.ProductApi))
	.AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddHttpClient("Coupon", client => client.BaseAddress = new Uri(serviceUrls.CouponApi))
	.AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option => option.AddSecurity());

builder.Services.AddAppAuthentication(jwtOptions);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Shopping Cart API");
	c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Services.ApplyMigrations<AppDbContext>();

app.Run();
