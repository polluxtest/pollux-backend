namespace Pollux.Application
{
    using System;
    using System.Security.Claims;
    using System.Security.Policy;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;

    using IdentityModel.AspNetCore.AccessTokenManagement;
    using IdentityModel.Client;

    using IdentityServer4.Events;
    using IdentityServer4.Services;
    using IdentityServer4.Stores;
    using Pitcher;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Domain.Entities;
    using Pollux.Persistence.Repositories;
    using Pollux.Persistence.Services.Cache;
    using Pollux.Common.Application.Models.Auth;

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

        Task<TokenResponse> SetAuth(LogInModel loginModel);

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
        /// The user identity sign manager
        /// </summary>
        private readonly SignInManager<User> userIdentitySignManager;

        /// <summary>
        /// The mapper
        /// </summary>
        private readonly IMapper mapper;

        private readonly IIdentityServerInteractionService interaction;
        private readonly IEventService events;
        private readonly IRedisCacheService redisCacheService;
        private readonly ITokenEndpointService tokenServiceEndpoint;


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
            IEventService events,
            ITokenEndpointService tokenServiceEndpoint,
            IRedisCacheService redisCacheService)
        {
            this.usersRepository = usersRepository;
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
            this.mapper = mapper;
            this.interaction = iIdentityServerInteractionService;
            this.events = events;
            this.tokenServiceEndpoint = tokenServiceEndpoint;
            this.redisCacheService = redisCacheService;

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
        public async Task LogInAsync(LogInModel model)
        {
            var context = await this.interaction.GetAuthorizationContextAsync(model.ReturnUrl);
            var result = await this.userIdentitySignManager.PasswordSignInAsync(model.Email, model.Password, true, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                var user = await this.usersRepository.GetAsync(p => p.Email.Equals(model.Email));
                await this.events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName, clientId: context?.Client.ClientId));

            }
            else
            {
                await events.RaiseAsync(new UserLoginFailureEvent(model.Email, "invalid credentials", clientId: context?.Client.ClientId));
            }
        }

        public async Task<TokenResponse> SetAuth(LogInModel loginModel)
        {
            var token = await this.tokenServiceEndpoint.RequestClientAccessToken("client", loginModel); // todo client;
            var accessTokenExpirationDate = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
            var refreshTokenExpirationDate = DateTime.UtcNow.AddDays(7);

            var tokenCache = new TokenModel()
            {
                AccessToken = $"Bearer {token.AccessToken}",
                RefreshToken = token.RefreshToken,
                AccessTokenExpirationDate = accessTokenExpirationDate,
                RefreshTokenExpirationDate = refreshTokenExpirationDate,
            };

            var success = await this.redisCacheService.SetObjectAsync<TokenModel>(loginModel.Email, tokenCache, TimeSpan.FromHours(1)); // this must match expiration of token ??

            if (success)
            {
                return token;
            }

            throw new InvalidOperationException("Could Store key in redis data base");
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

        /// <summary>
        /// Exists the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>True if Exists.</returns>
        public Task<bool> ExistUser(string username)
        {
            return this.usersRepository.AnyAsync(p => p.UserName == username);
        }
    }
}
