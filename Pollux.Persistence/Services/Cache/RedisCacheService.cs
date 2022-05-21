namespace Pollux.Persistence.Services.Cache
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Pollux.Common.Constants;
    using StackExchange.Redis;

    public class RedisCacheService : IRedisCacheService
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase redisDatabase;

        public RedisCacheService(IConfiguration configuration)
        {
            var urlRedisServer = configuration.GetSection("AppSettings")["RedisUrl"];
            var redisConfiguration = new ConfigurationOptions
            {
                AbortOnConnectFail = false,
                EndPoints = { urlRedisServer },
            };
            this.connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfiguration);
            this.redisDatabase = this.connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Sets the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// True if success.
        /// </returns>
        public Task<bool> SetKeyAsync(string key, string value)
        {
            var expiration = TimeSpan.FromSeconds(ExpirationConstants.RedisCacheExpirationSeconds);

            return this.redisDatabase.StringSetAsync(key, value, expiration);
        }

        /// <summary>
        /// Gets the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public async Task<string> GetKeyAsync(string key)
        {
            if (await this.KeyExistsAsync(key))
            {
                var redisValue = await this.redisDatabase.StringGetAsync(key);
                return redisValue;
            }

            return null;
        }

        /// <summary>
        /// Keys the exists asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if success.
        /// </returns>
        public Task<bool> KeyExistsAsync(string key)
        {
            return this.redisDatabase.KeyExistsAsync(key);
        }

        /// <summary>
        /// Deletes the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>
        /// True if success.
        /// </returns>
        public Task<bool> DeleteKeyAsync(string key)
        {
            return this.redisDatabase.KeyDeleteAsync(key);
        }

        /// <summary>
        /// Sets the object asynchronous.
        /// </summary>
        /// <typeparam name="T">Generic Type object to Serialize</typeparam>
        /// <param name="key">The key string</param>
        /// <param name="data">The data.</param>
        /// <returns>Model Serialized.</returns>
        public async Task<bool> SetObjectAsync<T>(string key, T data)
        {
            var dataStr = JsonSerializer.Serialize<T>(data);
            return await this.SetKeyAsync(key, dataStr);
        }

        /// <summary>
        /// Gets the object asynchronous.
        /// </summary>
        /// <typeparam name="T">Data object.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>Model Serialized.</returns>
        public async Task<T> GetObjectAsync<T>(string key)
        {
            var data = await this.GetKeyAsync(key);
            return JsonSerializer.Deserialize<T>(data);
        }
    }
}
