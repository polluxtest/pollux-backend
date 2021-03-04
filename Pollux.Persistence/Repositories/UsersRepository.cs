namespace Pollux.Persistence.Repositories
{
    using Pollux.Domain.Entities;

    /// <summary>
    /// Users Repository contract.
    /// </summary>
    public interface IUsersRepository : IRepository<User>
    {
    }

    /// <summary>
    /// Users Repository Data.
    /// </summary>
    public class UsersRepository : RepositoryBase<User>, IUsersRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UsersRepository"/> class.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public UsersRepository(PolluxDbContext dbContext)
            : base(dbContext)
        {
        }
    }
}
