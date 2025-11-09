using StackExchange.Redis;
using System.Text.Json;

namespace Shared.Services;



public interface IDistributedLockService
{
    Task<IDisposable?> AcquireLockAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan? wait = null
    );
}



public class RedisDistributedLockService : IDistributedLockService
{


    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;


    public RedisDistributedLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
        _database = _redis.GetDatabase();
    }


    public async Task<IDisposable?> AcquireLockAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan? wait = null
    )
    {
        var lockKey = $"lock:{resource}";
        var lockValue = Guid.NewGuid().ToString();
        var waitTime = wait ?? TimeSpan.FromSeconds(5);
        var endTime = DateTime.UtcNow.Add(waitTime);


        while (DateTime.UtcNow < endTime)
        {
            var acquired = await _database.StringSetAsync(
                lockKey,

                lockValue,

                expiry,

                When.NotExists
            );

            if (acquired)
            {
                return new RedisLock(
                    _database,

                    lockKey,

                    lockValue
                );
            }


            await Task.Delay(100);
        }


        return null;
    }



    private class RedisLock: IDisposable
    {
        private readonly IDatabase _database;
        private readonly string _key;
        private readonly string _value;
        private bool _disposed;


        public RedisLock(
            IDatabase database,
            string key,
            string value
        )
        {
            _database = database;
            _key = key;
            _value = value;
        }


        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            var script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end
            ";

            _database.ScriptEvaluate(
                script,
                new RedisKey[] { _key },
                new RedisValue[] { _value }
            );

            _disposed = true;
        }
    }

}