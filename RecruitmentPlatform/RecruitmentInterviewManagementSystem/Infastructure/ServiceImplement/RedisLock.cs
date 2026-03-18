using RecruitmentInterviewManagementSystem.Applications.DistributeLock;
using StackExchange.Redis;

namespace RecruitmentInterviewManagementSystem.Infastructure.ServiceImplement
{
    public class RedisLock : IRedisLock
    {
        public readonly IDatabase _db;
        public RedisLock(IConnectionMultiplexer connectionMultiplexer)
        {
            _db = connectionMultiplexer.GetDatabase(1);
        }

        // lock
        public async Task<bool> AcquireAsync(string key, string value, TimeSpan expiry)
        {
            return await _db.StringSetAsync(
                key,
                value,
                expiry,
                When.NotExists
              );
        }
        // unlock
        public async Task ReleaseAsync(string key, string value)
        {
            const string lua = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

            await _db.ScriptEvaluateAsync(
                lua,
                new RedisKey[] { key },
                new RedisValue[] { value }
            );
        }
    }
}
