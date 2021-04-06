namespace Pollux.API
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.AspNetCore.AccessTokenManagement;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Persistence.Services.Cache;

    public class AuthEventHandler
    {
        private readonly IServiceCollection services;

        public AuthEventHandler(IServiceCollection services)
        {
            this.services = services;
        }

        public async Task Handle(CookieValidatePrincipalContext context)
        {
            if (context.Request.Path.Equals("/api/pollux/User/LogIn"))
            {
                return;
            }

            if (!context.Principal.Identity.IsAuthenticated)
            {
                context.RejectPrincipal();
                return;
            }

            ServiceProvider serviceProvider = this.services.BuildServiceProvider();
            var redisCacheService = serviceProvider.GetService<IRedisCacheService>();
            var tokenEndpointServie = serviceProvider.GetService<ITokenIdentityService>();
            var loggedUserEmail = context.Principal.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email).Value;
            var token = await redisCacheService.GetObjectAsync<TokenModel>(loggedUserEmail);

            context.Request.Headers.TryGetValue("Authorization", out var authValues);
            var authotizationToken = authValues.First();

            if (!token.AccessToken.Equals(authotizationToken) ||
                    !context.Principal.Identity.IsAuthenticated ||
                    string.IsNullOrEmpty(token.AccessToken) ||
                    DateTime.UtcNow > token.RefreshTokenExpirationDate)
            {
                context.RejectPrincipal();
                return;
            }

            if (DateTime.UtcNow > token.AccessTokenExpirationDate)
            {
                var newAccesstokenResponse = await tokenEndpointServie.RefreshUserAccessTokenAsync(token.RefreshToken);
                token.AccessToken = newAccesstokenResponse.AccessToken;
                token.AccessTokenExpirationDate = DateTime.UtcNow.AddSeconds(5); // todo definw expiration and join code
                var success = await redisCacheService.SetObjectAsync<TokenModel>(loggedUserEmail, token, TimeSpan.FromDays(7));
                if (success)
                {

                }
            }

            return;
        }
    }
}
