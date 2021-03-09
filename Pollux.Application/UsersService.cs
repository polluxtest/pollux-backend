namespace Pollux.Application
{
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Identity;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;

    public interface IUsersService
    {
        /// <summary>
        /// Signs up the User.
        /// </summary>
        /// <param name="signUpModel">The create user model.</param>
        void SignUp(SignUpModel signUpModel);

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="logInModel">The log in model.</param>
        /// <returns>Task.</returns>
        Task LogInAsync(LogInModel logInModel);
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

        private readonly IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="usersRepository">The users repository.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userSignInManager">The user sign in manager.</param>
        /// <param name="mapper">The Auto mapper instance.</param>
        public UsersService(
            IUsersRepository usersRepository,
            UserManager<User> userManager,
            SignInManager<User> userSignInManager,
            IMapper mapper)
        {
            this.usersRepository = usersRepository;
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
            this.mapper = mapper;
        }

        /// <summary>
        /// Signs up a user.
        /// </summary>
        /// <param name="signUpModel">The create user model.</param>
        public async void SignUp(SignUpModel signUpModel)
        {
            var newUser = new User();
            this.mapper.Map(signUpModel, newUser);

            var identityResult = await this.userIdentityManager.CreateAsync(newUser).ConfigureAwait(false);
        }

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="logInModel">The log in model.</param>
        /// <returns>
        /// Task.
        /// </returns>
        public Task LogInAsync(LogInModel logInModel)
        {
            var user = new User();
            this.mapper.Map(logInModel, user);
            return this.userIdentitySignManager.SignInAsync(user, true);
        }
    }
}
