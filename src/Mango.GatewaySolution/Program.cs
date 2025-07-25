using Mango.GatewaySolution.Extensions;
using Mango.GatewaySolution.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
var jwtOptions = builder.Configuration.GetRequiredSection(nameof(JwtOptions)).Get<JwtOptions>() ?? new JwtOptions();

var ocelotFile = builder.Environment.EnvironmentName.ToLower().Equals("production") ? "ocelot.Production.json" : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, false, true);

builder.Services.AddAppAuthentication(jwtOptions);
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");
await app.UseOcelot();
app.Run();
