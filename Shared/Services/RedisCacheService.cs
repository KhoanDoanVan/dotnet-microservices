using StackExchange.Redis;
using System.Text.Json;



namespace Shared.Services;


public interface IRedisCacheService
{
    Task<T?> GetAsync<T>(string key);
    Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key, long value = 1);
    Task<long> DecrementAsync(string key, long value = 1);
    Task<bool> SetAddAsync(string key, string value);
    Task<string[]> SetMembersAsync(string key);
}


public class RedisCacheService: IRedisCacheService
{

    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;


    public RedisCacheService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
    }


    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
            return default;


        return JsonSerializer.Deserialize<T>(value!);
    }


    public async Task<bool> SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiry = null
    )
    {
        var serialized = JsonSerializer.Serialize(value);

        return await _database.StringSetAsync(key, serialized, expiry);
    }


    public async Task<bool> RemoveAsync(string key)
    {
        return await _database.KeyDeleteAsync(key);
    }


    public async Task<bool> ExistsAsync(string key)
    {
        return await _database.KeyExistsAsync(key);
    }


    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        return await _database.StringIncrementAsync(key, value);
    }



    public async Task<long> DecrementAsync(string key, long value = 1)
    {
        return await _database.StringDecrementAsync(key, value);
    }


    public async Task<bool> SetAddAsync(string key, string value)
    {
        return await _database.SetAddAsync(key, value);
    }


    public async Task<string[]> SetMembersAsync(string key)
    {
        var members = await _database.SetMembersAsync(key);
        return members.Select(m => m.ToString()).ToArray();
    }

}