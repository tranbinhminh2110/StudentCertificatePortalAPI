
using StackExchange.Redis;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class RedisService : IRedisService
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public RedisService(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
        }

        public async Task SaveResetTokenAsync(int userId, string token, TimeSpan expiration)
        {
            var key = $"password_reset_token:{userId}";
            await _database.StringSetAsync(key, token, expiration);
        } 
        public async Task SaveTokenAsync(int userId, string accessToken, string refreshToken)
        {
            var keyAccess = $"access_token:{userId}";
            var keyRefresh = $"refresh_token:{userId}";

            await _database.StringSetAsync(keyAccess, accessToken, TimeSpan.FromHours(1)); 
            await _database.StringSetAsync(keyRefresh, refreshToken, TimeSpan.FromDays(30));
        }


        public async Task<string> GetResetTokenAsync(int userId)
        {
            var key = $"password_reset_token:{userId}";
            return await _database.StringGetAsync(key);
        }

        public async Task DeleteResetTokenAsync(int userId)
        {
            var key = $"password_reset_token:{userId}";
            await _database.KeyDeleteAsync(key);
        }
    }
}
