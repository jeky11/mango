using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mango.Services.Infrastructure.Extensions;

public static class EntityFrameworkExtensions
{
	public static void ApplyMigrations<TContext>(this IServiceProvider serviceProvider)
		where TContext : DbContext
	{
		using var scope = serviceProvider.CreateScope();

		var db = scope.ServiceProvider.GetRequiredService<TContext>();
		if (db.Database.GetPendingMigrations().Any())
		{
			db.Database.Migrate();
		}
	}
}
