namespace Pollux.Persistence.Services.Cache
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Pollux.Common.Constants;
    using Polly;
    using StackExchange.Redis;
    using StackExchange.Redis.Extensions.Core.Configuration;

    public class RedisCacheService : IRedisCacheService
    {
        private static readonly Lazy<ConnectionMultiplexer> Connection;
        private static readonly IDatabase RedisDatabase;

        static RedisCacheService()
        {
            var connectionString = "127.0.0.1:6379,127.0.0.1:6380,syncTimeout =3000,abortOnConnectFail=false";
            var options = ConfigurationOptions.Parse(connectionString);
            Connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(options));
            RedisDatabase = Connection.Value.GetDatabase();
        }

        private RedisConfiguration GetRedisConfiguration(IConfiguration configuration)
        {
            return new RedisConfiguration()
            {
                AbortOnConnectFail = false,
                Hosts = new RedisHost[] {
                    new RedisHost()
                    {
                        Host = configuration.GetSection("AppSettings")["RedisUrl"],
                        Port = 6379,
                    },
                    new RedisHost()
                    {
                        Host = "redis",
                        Port = 6380,
                    },
                },
                Database = 0,
                Ssl = false,
                SyncTimeout = 10000,
                ServerEnumerationStrategy = new ServerEnumerationStrategy()
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw,
                },
                PoolSize = 50,

            };
        }

        /// <summary>
        /// Sets the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// True if success.
        /// </returns>
        public async Task<bool> SetKeyAsync(string key, string value)
        {
            var expiration = TimeSpan.FromSeconds(ExpirationConstants.RedisCacheExpirationSeconds);

            return await RedisDatabase.StringSetAsync(key, value, expiration);
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
                var redisValue = await RedisDatabase.StringGetAsync(key);
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
            return RedisDatabase.KeyExistsAsync(key);
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
            return RedisDatabase.KeyDeleteAsync(key);
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
