namespace Pollux.Application
{
    using System;
    using System.Security.Policy;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;

    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;

    using Microsoft.AspNetCore.Authentication;
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
        void LogOutAsync();
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
        /// The mapper
        /// </summary>
        private readonly IMapper mapper;

        private readonly IIdentityServerInteractionService interaction;
        private readonly IClientStore clientStore;
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly IEventService events;


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
            IMapper mapper,
            IIdentityServerInteractionService iIdentityServerInteractionService,
            IClientStore clientStore,
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IEventService events)
        {
            this.usersRepository = usersRepository;
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
            this.mapper = mapper;
            this.interaction = iIdentityServerInteractionService;
            this.clientStore = clientStore;
            this.events = events;

        }

        /// <summary>
        /// Signs up a user.
        /// </summary>
        /// <param name="signUpModel">The create user model.</param>
        public async Task<IdentityResult> SignUp(SignUpModel signUpModel, CancellationToken cancellationToken)
        {
            var newUser = new User();
            this.mapper.Map(signUpModel, newUser);
            newUser.UserName = newUser.Email;
            newUser.NormalizedUserName = newUser.Email;

            return await this.userIdentityManager.CreateAsync(newUser, signUpModel.PassWord).ConfigureAwait(false);

        }

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="logInModel">The log in model.</param>
        /// <returns>
        /// Task.
        /// </returns>
        public async Task LogInAsync(LogInModel logInModel)
        {
            var context = await this.interaction.GetAuthorizationContextAsync(logInModel.ReturnUrl);



            var result = await userIdentitySignManager.PasswordSignInAsync(logInModel.Email, logInModel.Password, true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await this.usersRepository.GetAsync(p => p.Email.Equals(logInModel.Email));
                await this.events.RaiseAsync(new UserLoginSuccessEvent(user.Email, user.Id, "username", clientId: context?.Client.ClientId));

                if (context != null)
                {
                    //if (context.IsNativeClient())
                    //{
                    //    // The client is native, so this change in how to
                    //    // return the response is for better UX for the end user.
                    //    return this.LoadingPage("Redirect", model.ReturnUrl);
                    //}

                    // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null
                }
            }

            await events.RaiseAsync(new UserLoginFailureEvent(logInModel.Email, "invalid credentials", clientId: context?.Client.ClientId));

            // something went wrong
            //await this.userIdentitySignManager.SignInAsync(user, true);


        }

        /// <summary>
        /// Logs the out asynchronous.
        /// </summary>
        /// <returns>Task</returns>
        public void LogOutAsync()
        {

            //if (User?.Identity.IsAuthenticated == true)
            //{
            //    // delete local authentication cookie
            //    await _signInManager.SignOutAsync();

            //    // raise the logout event
            //    await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
            //}
            //return this.userIdentitySignManager.SignOutAsync();
        }
    }
}
