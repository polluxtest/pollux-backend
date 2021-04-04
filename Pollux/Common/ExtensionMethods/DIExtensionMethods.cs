namespace Pollux.API
{
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.API.AuthIdentityServer;
    using Pollux.Application;
    using Pollux.Domain.Entities;
    using Pollux.Persistence;
    using Pollux.Persistence.Repositories;

    /// <summary>
    /// Extension Methods for DI.
    /// </summary>
    public static class DIExtensionMethods
    {
        /// <summary>
        /// Adds the di repositories as an extension methods for the startup .
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void AddDIRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUsersRepository, UsersRepository>();
        }

        /// <summary>
        /// Adds the di services.
        /// </summary>
        /// <param name="services">The service collection.</param>
        public static void AddDIServices(this IServiceCollection services)
        {
            services.AddIdentity<User, Role>(
                options =>
                    {
                        options.User.RequireUniqueEmail = false;
                    });
            services.AddScoped<DbContext, PolluxDbContext>();
            services.AddScoped<IRoleStore<Role>, RoleStore<Role>>();
            services.AddScoped<IUserStore<User>, UserStore<User>>();
            services.AddScoped<IUsersService, UsersService>();
        }

        /// <summary>
        /// Adds the identity server services.
        /// </summary>
        /// <param name="services">The services.</param>
        public static void AddIdentityServerServices(this IServiceCollection services)
        {
            services.AddScoped<IResourceStore, ResourceStore>();
            services.AddTransient<IClientStore, ClientStore>();
            services.AddTransient<ICorsPolicyService, CorsPolicyService>();
            services.AddTransient<IProfileService, ProfileService>();
        }
    }
}
