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
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase redisDatabase;
        private readonly ILogger logger;

        public RedisCacheService(IConfiguration configuration, ILogger logger)
        {
            this.logger = logger;
            this.logger.LogInformation("Connecting Redis Engine");
            var host = "localhost:6379";
            var redisConfiguration = $"{host},connectRetry=3,connectTimeout=3000,abortConnect=false";
            this.connectionMultiplexer = ConnectionMultiplexer.ConnectAsync(redisConfiguration).Result;
            this.logger.LogInformation($"Redis engine connected = {this.connectionMultiplexer.IsConnected}");
            this.logger.LogInformation($"Redis engine is connecting = {this.connectionMultiplexer.IsConnecting}");
            this.redisDatabase = this.connectionMultiplexer.GetDatabase();

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
            this.logger.LogInformation("Setting Key Redis Engine");
            bool result = false;
            var maxRetryAttempts = 3;
            var pauseBetweenFailures = TimeSpan.FromSeconds(2);
            var expiration = TimeSpan.FromSeconds(ExpirationConstants.RedisCacheExpirationSeconds);

            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(maxRetryAttempts, i => pauseBetweenFailures);

            await retryPolicy.ExecuteAsync(async () =>
            {
                var result = await this.redisDatabase.StringSetAsync(key, value, expiration);
                if (result)
                {
                    this.logger.LogInformation("Redis Key Set");
                }
                else
                {
                    this.logger.LogInformation("Redis Key Not Set , retrying...");
                }

            });

            this.logger.LogInformation("Finishing Key Redis Engine with false result");

            return result;

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
