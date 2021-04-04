namespace Pollux.Persistence.Services.Cache
{
    using System;
    using System.Threading.Tasks;
    using Pollux.Common.Application.Models.Auth;

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
        /// Sets the object asynchronous.
        /// </summary>
        /// <typeparam name="T">Generic Type to store.</typeparam>
        /// <param name="data">The data.</param>
        /// <returns>Task.</returns>
        Task<bool> SetObjectAsync<T>(string key, T data, TimeSpan? expiration = null);

        /// <summary>
        /// Gets the object asynchronous.
        /// </summary>
        /// <typeparam name="T">Generic Type object to seerialize.</typeparam>
        /// <param name="key">The key.</param>
        /// <returns>Deserialized object.</returns>
        Task<T> GetObjectAsync<T>(string key);

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

        /// <returns>Model Serialized.</returns>
    }
}
