namespace Pollux.API.Auth.AuthIdentityServer
{
    using System.Threading.Tasks;
    using IdentityServer4.Services;

    public class CorsPolicyService : ICorsPolicyService
    {
        /// <summary>
        /// Determines whether origin is allowed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <returns></returns>
        public async Task<bool> IsOriginAllowedAsync(string origin)
        {
            return true;
        }
    }
}