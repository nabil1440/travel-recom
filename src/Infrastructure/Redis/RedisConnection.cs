using StackExchange.Redis;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Redis;

public class RedisConnection
{
	public IConnectionMultiplexer Connection { get; }

	public RedisConnection(IConfiguration config)
	{
		Connection = ConnectionMultiplexer.Connect(
			config.GetConnectionString("Redis")!);
	}
}