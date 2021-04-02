namespace Pollux.Persistence.Services.Cache
{
    using System;
    using System.Threading.Tasks;
    using StackExchange.Redis;

    public class RedisCacheService : IRedisCacheService
    {
        private readonly ConnectionMultiplexer connectionMultiplexer;
        private readonly IDatabase redisDatabase;

        public RedisCacheService()
        {
            this.connectionMultiplexer = ConnectionMultiplexer.Connect("localhost:6379"); // todo change host to app settigs
            this.redisDatabase = this.connectionMultiplexer.GetDatabase();
        }

        /// <summary>
        /// Sets the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The Expiration key.</param>
        /// <returns>
        /// True if success.
        /// </returns>
        public Task<bool> SetKeyAsync(string key, string value, TimeSpan? expiration = null)
        {
            if (expiration == null)
            {
                expiration = TimeSpan.FromHours(1);
            }

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
    }
}
