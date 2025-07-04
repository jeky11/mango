using Mango.MessageBus;
using Mango.MessageBus.MessageBusSender;
using Mango.Services.Infrastructure;
using Mango.Services.Infrastructure.Extensions;
using Mango.Services.Infrastructure.Models;
using Mango.Services.OrderAPI;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.EntityFrameworkCore;
using Stripe;
using ProductService = Mango.Services.OrderAPI.Service.ProductService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var serviceUrls = builder.Configuration.GetRequiredSection(nameof(ServiceUrls)).Get<ServiceUrls>() ?? throw new NullReferenceException();
var jwtOptions = builder.Configuration.GetRequiredSection(nameof(JwtOptions)).Get<JwtOptions>() ?? new JwtOptions();
var stripeOptions = builder.Configuration.GetRequiredSection(nameof(StripeOptions)).Get<StripeOptions>() ?? throw new NullReferenceException();
builder.Services.Configure<MessageBusConnectionStrings>(builder.Configuration.GetRequiredSection("ConnectionStrings"));
builder.Services.Configure<TopicAndQueueNames>(builder.Configuration.GetRequiredSection(nameof(TopicAndQueueNames)));

builder.Services.AddDbContext<AppDbContext>(options =>
{
	options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

var mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddSingleton<IMessageBusSenderFactory, MessageBusSenderFactory>();
builder.Services.AddSingleton<IMessageBusSender>(sp => sp.GetRequiredService<IMessageBusSenderFactory>().CreateMessageBusSender());
builder.Services.AddHttpClient("Product", client => client.BaseAddress = new Uri(serviceUrls.ProductApi))
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
	c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API");
	c.RoutePrefix = string.Empty;
});

StripeConfiguration.ApiKey = stripeOptions.SecretKey;

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Services.ApplyMigrations<AppDbContext>();

app.Run();
