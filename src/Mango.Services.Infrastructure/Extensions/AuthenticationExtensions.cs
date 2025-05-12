using System.Text;
using Mango.Services.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Mango.Services.Infrastructure.Extensions;

public static class AuthenticationExtensions
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
