namespace Pollux.Application
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Identity;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Models.Request;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;

    public interface IUsersService
    {
        void LogIn(LogInModel logInModel);
    }

    public class UsersService : IUsersService
    {
        /// <summary>
        /// The users repository.
        /// </summary>
        private readonly IUsersRepository usersRepository;

        /// <summary>
        /// The user identity manager.
        /// </summary>
        private readonly UserManager<User> userIdentityManager;

        /// <summary>
        /// The user identity sign manager
        /// </summary>
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
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
        }

        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="createUserModel">The create user model.</param>
        public async void SignUp(CreateUserModel createUserModel)
        {
            var identityResult = await this.userIdentityManager.CreateAsync(new User(), "abc123");
        }

        /// <summary>
        /// Logs the in.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        public async void LogIn(LogInModel loginModel)
        {
            await this.userIdentitySignManager.SignInAsync(new User(), true);
        }
    }
}
