namespace Pollux.API
{
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

        }
    }
}