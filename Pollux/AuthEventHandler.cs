using System.Collections.Generic;
using Pollux.Common.Exceptions;

namespace Pollux.API
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityModel.AspNetCore.AccessTokenManagement;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Common.Application.Models.Auth;
    using Pollux.Persistence.Services.Cache;

    public class AuthEventHandler
    {
        private readonly IServiceCollection services;
        private readonly List<string> anonymousRoutes;

        public AuthEventHandler(IServiceCollection services)
        {
            this.services = services;
            this.anonymousRoutes = new List<string>() { "SignUp", "LogIn", "ResetPassword", "LogOut", "Exist" };
        }

        public async Task<TokenResponse> Handle(CookieValidatePrincipalContext context)
        {
            foreach (var anonymousRoute in this.anonymousRoutes)
            {
                if (context.Request.Path.Value.EndsWith(anonymousRoute))
                {
                    return null;
                }
            }


            if (!context.Principal.Identity.IsAuthenticated)
            {
                context.RejectPrincipal();
                return null;
            }

            var serviceProvider = this.services.BuildServiceProvider();
            var redisCacheService = serviceProvider.GetService<IRedisCacheService>();
            var tokenEndpointService = serviceProvider.GetService<ITokenIdentityService>();
            var emailClaim = context.Principal.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Email);

            if (emailClaim == null)
            {
                throw new NotAuthenticatedException("not authenticated");
            }

            var token = await redisCacheService.GetObjectAsync<TokenModel>(emailClaim.Value);
            TokenResponse newAccessToken = null;

            context.Request.Headers.TryGetValue("Authorization", out var authValues);

            if (!authValues.Any())
            {
                throw new NotAuthenticatedException("not authenticated");
            }

            var authorizationToken = authValues.First();

            if (!token.AccessToken.Equals(authorizationToken) ||
                    !context.Principal.Identity.IsAuthenticated ||
                    string.IsNullOrEmpty(token.AccessToken) ||
                    DateTime.UtcNow > token.RefreshTokenExpirationDate)
            {
                context.RejectPrincipal();
                throw new NotAuthenticatedException("not authenticated");
            }

            if (DateTime.UtcNow > token.AccessTokenExpirationDate)
            {
                newAccessToken = await tokenEndpointService.RefreshUserAccessTokenAsync(token.RefreshToken);
                token.AccessToken = newAccessToken.AccessToken;
                token.AccessTokenExpirationDate = DateTime.UtcNow.AddSeconds(60);
                var success = await redisCacheService.SetObjectAsync<TokenModel>(emailClaim.Value, token, TimeSpan.FromDays(7));
            }

            return newAccessToken;
        }
    }
}
