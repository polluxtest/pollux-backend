namespace Pollux.Application
{
    using Microsoft.AspNetCore.Identity;
    using Pollux.Common.Models.Request;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;

    public interface IUsersService
    {
    }

    public class UsersService : IUsersService
    {
        private readonly IUsersRepository usersRepository;

        private readonly UserManager<User> userIdentityMananger;

        private readonly SignInManager<User> userIdentitySignManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="usersRepository">The users repository.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userSignInManager">The user sign in manager.</param>
        public UsersService(
            IUsersRepository usersRepository,
            UserManager<User> userManager,
            SignInManager<User> userSignInManager)
        {
            this.usersRepository = usersRepository;
            this.userIdentityMananger = userManager;
            this.userIdentitySignManager = userSignInManager;
        }

        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="createUserModel">The create user model.</param>
        public async void SignUp(CreateUserModel createUserModel)
        {
            var identityResult = await this.userIdentityMananger.CreateAsync(new User(), "abc123");
        }

        /// <summary>
        /// Logs the in.
        /// </summary>
        public async void LogIn()
        {
            await this.userIdentitySignManager.SignInAsync(new User(), true);
        }
    }
}
