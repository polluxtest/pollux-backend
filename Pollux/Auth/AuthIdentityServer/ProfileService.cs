namespace Pollux.API.Auth.AuthIdentityServer
{
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Services;

    public class ProfileService : IProfileService
    {
        /// <summary>
        /// This method is called whenever claims about the user are requested (e.g. during token creation or via the userinfo endpoint)
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var userId = context.Subject.Identities.First().Claims.First(p => p.Type.Equals("sub")).Value;
            context.IssuedClaims.Add(new Claim("Id", userId));
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method gets called whenever identity server needs to determine if the user is valid or active (e.g. if the user's account has been deactivated since they logged in).
        /// (e.g. during token issuance or validation).
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.FromResult(context.IsActive);
        }
    }
}
