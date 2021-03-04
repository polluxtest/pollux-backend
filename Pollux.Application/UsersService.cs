namespace Pollux.Application
{
    using Pollux.Persistence.Repositories;

    /// <summary>
    /// Users Service Contract.
    /// </summary>
    public interface IUsersService { }

    /// <summary>
    /// Users Service Implementation.
    /// </summary>
    /// <seealso cref="Pollux.Application.IUsersService" />
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="usersRepository">The users repository.</param>
        public UsersService(IUsersRepository usersRepository)
        {
            this.usersRepository = usersRepository;
        }
    }
}
