namespace Pollux.Persistence.Repositories
{
    using Pollux.Domain.Entities;

    public interface IUserPreferencesRepository : IRepository<UserPreferences>
    {
    }

    public class UserPreferencesRepository : RepositoryBase<UserPreferences>, IUserPreferencesRepository
    {
        public UserPreferencesRepository(PolluxDbContext context)
            : base(context)
        {
        }
    }
}
