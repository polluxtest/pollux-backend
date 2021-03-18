namespace Pollux.API
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using IdentityServer4;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;

    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Application;
    using Pollux.Persistence.Repositories;
    using Pollux.Domain;
    using Pollux.Domain.Entities;
    using Pollux.Persistence;

    /// <summary>
    /// Extension Methods for DI.
    /// </summary>
    public static class DIExtensionMethods
    {
        /// <summary>
        /// Adds the di repositories as an extension methods for the startup .
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddDIRepositories(this IServiceCollection service)
        {
            service.AddScoped<IUsersRepository, UsersRepository>();
        }

        /// <summary>
        /// Adds the di services.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddDIServices(this IServiceCollection service)
        {
            service.AddIdentity<User, Role>(
                options =>
                    {
                        options.User.RequireUniqueEmail = false;
                    });
            service.AddScoped<DbContext, PolluxDbContext>();
            service.AddScoped<IRoleStore<Role>, RoleStore<Role>>();
            service.AddScoped<IUserStore<User>, UserStore<User>>();
            service.AddScoped<IUsersService, UsersService>();

            service.AddScoped<IdentityServer4.Stores.IResourceStore, ResourceStore>();
            service.AddTransient<IClientStore, ClientStore>();
            service.AddTransient<ICorsPolicyService, CorsPolicyService>();
            service.AddTransient<IProfileService, ProfileService>();


        }
    }

    public class CorsPolicyService : ICorsPolicyService
    {
        public async Task<bool> IsOriginAllowedAsync(string origin)
        {
            return true;
        }
    }

    public class ClientStore : IClientStore
    {
        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            return new Client()
            {
                ClientId = "client",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowOfflineAccess = true,
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                AllowedScopes = new List<string>() { "api", "api/pollux", "offline_access" }
            };
        }
    }

    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            return Task.CompletedTask;

        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = true;
            return Task.CompletedTask;
        }
    }

    public class ResourceStore : IResourceStore
    {
        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return new List<IdentityResource>();
        }

        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            return new List<ApiScope>() { new ApiScope() { Name = "api" }, new ApiScope() { Name = "api/pollux" } , new ApiScope() { Name =                 IdentityServerConstants.StandardScopes.OfflineAccess
                                            } };
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            return new List<ApiResource>();
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            throw new System.NotImplementedException();
        }

        public Task<Resources> GetAllResourcesAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}