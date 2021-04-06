namespace Pollux.Application
{
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Identity;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;

    public interface IUsersService
    {
        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="signUpModel">The sign up model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>IdentityResult.</returns>
        Task<IdentityResult> SignUp(SignUpModel signUpModel, CancellationToken cancellationToken);

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="logInModel">The log in model.</param>
        /// <returns>Task.</returns>
        Task LogInAsync(LogInModel logInModel);

        /// <summary>
        /// Logs the out asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        Task LogOutAsync();

        /// <summary>
        /// Exists the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>True if exists.</returns>
        Task<bool> ExistUser(string username);
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
        /// The user identity sign manager.
        /// </summary>
        private readonly SignInManager<User> userIdentitySignManager;

        /// <summary>
        /// The mapper.
        /// </summary>
        private readonly IMapper mapper;

        /// <summary>
        /// The interaction.
        /// </summary>
        private readonly IIdentityServerInteractionService interaction;

        /// <summary>
        /// The events service to log and raise events on auth.
        /// </summary>
        private readonly IEventService events;


        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="usersRepository">The users repository.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userSignInManager">The user sign in manager.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="iIdentityServerInteractionService">The i identity server interaction service.</param>
        /// <param name="eventsService">The events.</param>
        public UsersService(
            IUsersRepository usersRepository,
            UserManager<User> userManager,
            SignInManager<User> userSignInManager,
            IMapper mapper,
            IIdentityServerInteractionService iIdentityServerInteractionService,
            IEventService eventsService)
        {
            this.usersRepository = usersRepository;
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
            this.mapper = mapper;
            this.interaction = iIdentityServerInteractionService;
            this.events = eventsService;
        }

        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="signUpModel">The sign up model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// IdentityResult.
        /// </returns>
        public async Task<IdentityResult> SignUp(SignUpModel signUpModel, CancellationToken cancellationToken)
        {
            var newUser = new User();
            this.mapper.Map(signUpModel, newUser);
            newUser.UserName = newUser.Email;
            newUser.NormalizedUserName = newUser.Email;

            return await this.userIdentityManager.CreateAsync(newUser, signUpModel.PassWord);
        }

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="loginModel">The log in model.</param>
        /// <returns>
        /// Task.
        /// </returns>
        public async Task LogInAsync(LogInModel loginModel)
        {
            var context = await this.interaction.GetAuthorizationContextAsync(loginModel.ReturnUrl);
            var result = await this.userIdentitySignManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                await this.events.RaiseAsync(new UserLoginSuccessEvent(loginModel.Email, string.Empty, loginModel.Email, clientId: context?.Client.ClientId));
            }
            else
            {
                await this.events.RaiseAsync(new UserLoginFailureEvent(loginModel.Email, "invalid credentials", clientId: context?.Client.ClientId));
            }
        }

        /// <summary>
        /// Logs the out asynchronous.
        /// </summary>
        /// <returns>Task.</returns>
        public Task LogOutAsync()
        {
            return this.userIdentitySignManager.SignOutAsync();
        }

        /// <summary>
        /// Exists the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>True if Exists.</returns>
        public Task<bool> ExistUser(string username)
        {
            return this.usersRepository.AnyAsync(p => p.UserName == username);
        }
    }
}
