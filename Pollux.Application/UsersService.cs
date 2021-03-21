﻿namespace Pollux.Application
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

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    using Pollux.API.OAuth.Interfaces;
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

        Task<TokenResponse> SetAuth(ClaimsPrincipal user);
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
        private readonly IUserAccessTokenManagementService userAccessTokenManagementService;

        private readonly ITokenEndpointService tokenServiceEndpoint;
        private readonly IUserTokenStore userTokeStore;


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
            IEventService events,
            IUserAccessTokenManagementService userAccessTokenManagementService,
            IUserTokenStore userTokeStore,
            ITokenEndpointService tokenServiceEndpoint)
        {
            this.usersRepository = usersRepository;
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
            this.mapper = mapper;
            this.interaction = iIdentityServerInteractionService;
            this.events = events;
            this.userAccessTokenManagementService = userAccessTokenManagementService;
            this.tokenServiceEndpoint = tokenServiceEndpoint;
            this.userTokeStore = userTokeStore;

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

        public async Task<TokenResponse> SetAuth(ClaimsPrincipal user)
        {
            //var token = await this.userAccessTokenManagementService.GetUserAccessTokenAsync(user);
            //var atparams = new ClientAccessTokenParameters();
            var token = await this.tokenServiceEndpoint.RequestClientAccessToken("client");
            return token;
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
