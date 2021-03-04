namespace Pollux.API
{
    using Microsoft.Extensions.DependencyInjection;
    using Pollux.Application;
    using Pollux.Persistence.Repositories;

    /// <summary>
    /// Extension Methods for DI.
    /// </summary>
    public static class DIExtensionMethods
    {
        /// <summary>
        /// Adds the di repositories as an extension methods for the startup .
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddDIRepositories(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUsersRepository, UsersRepository>();
        }

        /// <summary>
        /// Adds the di services.
        /// </summary>
        /// <param name="serviceCollection">The service collection.</param>
        public static void AddDIServices(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<IUsersService, UsersService>();
        }
    }
}