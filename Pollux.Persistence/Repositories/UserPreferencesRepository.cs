namespace Pollux.Persistence.Repositories
{
    using Domain.Entities;

    public interface IUserPreferencesRepository : IRepository<UserPreferences>
    {
    }

    /// <summary>
    /// User Preferences Repository
    /// </summary>
    /// <seealso cref="Pollux.Persistence.RepositoryBase&lt;Pollux.Domain.Entities.UserPreferences&gt;" />
    /// <seealso cref="Pollux.Persistence.Repositories.IUserPreferencesRepository" />
    public class UserPreferencesRepository : RepositoryBase<UserPreferences>, IUserPreferencesRepository
    {
        public UserPreferencesRepository(PolluxDbContext context)
            : base(context)
        {
        }
    }
}
