using System.Text;
using Mango.Services.ProductAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.ProductAPI.Extensions;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddAppAuthentication(this IServiceCollection serviceCollection, JwtOptions jwtOptions)
	{
		var issuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtOptions.Secret));

		serviceCollection.AddAuthentication(
				x =>
				{
					x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
			.AddJwtBearer(
				x =>
				{
					x.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						ValidateIssuer = true,
						ValidateAudience = true,
						IssuerSigningKey = issuerSigningKey,
						ValidIssuer = jwtOptions.Issuer,
						ValidAudience = jwtOptions.Audience
					};
				});

		return serviceCollection;
	}
}
