using Infrastructure.Persistence;
using Infrastructure.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
	public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration config)
	{
		services.AddDbContext<AppDbContext>(options =>
			options.UseNpgsql(
				config.GetConnectionString("Postgres")));

		services.AddSingleton<RedisConnection>();

		return services;
	}
}