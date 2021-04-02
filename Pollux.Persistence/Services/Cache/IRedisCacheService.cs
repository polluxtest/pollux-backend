namespace Pollux.Persistence.Services.Cache
{
    using System;
    using System.Threading.Tasks;

    public interface IRedisCacheService
    {
        /// <summary>
        /// Sets the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expiration">The expiration key time.</param>
        /// <returns>True if success.</returns>
        Task<bool> SetKeyAsync(string key, string value, TimeSpan? expiration);

        /// <summary>
        /// Gets the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if success.</returns>
        Task<string> GetKeyAsync(string key);

        /// <summary>
        /// Keys the exists asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if success.</returns>
        public Task<bool> KeyExistsAsync(string key);

        /// <summary>
        /// Deletes the key asynchronous.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if success.</returns>
        public Task<bool> DeleteKeyAsync(string key);
    }
}
