namespace Pollux.API
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using IdentityServer4.Models;
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

        }
    }

    public class ResourceStore : IResourceStore
    {
        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            throw new System.NotImplementedException();
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