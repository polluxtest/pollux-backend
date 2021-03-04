public static class DIExtensionMethods
{
	public static void AddDIRepositories(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IUsersRepository, UsersRepository>();

	}
}