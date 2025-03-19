using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mango.Services.ProductAPI.Extensions;

public static class SwaggerGenOptionsExtensions
{
	public static SwaggerGenOptions AddAddSecurity(this SwaggerGenOptions swaggerGenOptions)
	{
		swaggerGenOptions.AddSecurityDefinition(
			JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
				In = ParameterLocation.Header,
				Type = SecuritySchemeType.ApiKey,
				Scheme = JwtBearerDefaults.AuthenticationScheme
			});
		swaggerGenOptions.AddSecurityRequirement(
			new OpenApiSecurityRequirement
			{
				{
					new OpenApiSecurityScheme
					{
						Reference = new OpenApiReference
						{
							Type = ReferenceType.SecurityScheme,
							Id = JwtBearerDefaults.AuthenticationScheme
						}
					},
					[]
				}
			});

		return swaggerGenOptions;
	}
}
