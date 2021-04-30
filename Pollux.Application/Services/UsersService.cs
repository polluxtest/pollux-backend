﻿namespace Pollux.Application
{
    using System;
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
        /// <returns>IdentityResult.</returns>
        Task<IdentityResult> SignUp(SignUpModel signUpModel);

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="logInModel">The log in model.</param>
        /// <returns>Task.</returns>
        Task<bool> LogInAsync(LogInModel logInModel);

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

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>Task.</returns>
        Task ResetPassword(string email, string newPassword);

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
        /// The password hasher
        /// </summary>
        private readonly IPasswordHasher<User> passwordHasher;


        /// <summary>
        /// Initializes a new instance of the <see cref="UsersService"/> class.
        /// </summary>
        /// <param name="usersRepository">The users repository.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userSignInManager">The user sign in manager.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="iIdentityServerInteractionService">The i identity server interaction service.</param>
        /// <param name="eventsService">The events.</param>
        /// <param name="passwordHasher">The Password hasher.</param>

        public UsersService(
            IUsersRepository usersRepository,
            UserManager<User> userManager,
            SignInManager<User> userSignInManager,
            IMapper mapper,
            IIdentityServerInteractionService iIdentityServerInteractionService,
            IEventService eventsService,
            IPasswordHasher<User> passwordHasher)
        {
            this.usersRepository = usersRepository;
            this.userIdentityManager = userManager;
            this.userIdentitySignManager = userSignInManager;
            this.mapper = mapper;
            this.interaction = iIdentityServerInteractionService;
            this.events = eventsService;
            this.passwordHasher = passwordHasher;
        }

        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="signUpModel">The sign up model.</param>
        /// <returns>
        /// IdentityResult.
        /// </returns>
        public async Task<IdentityResult> SignUp(SignUpModel signUpModel)
        {
            var existsUser = await this.ExistUser(signUpModel.Email);
            if (existsUser)
            {
                throw new ArgumentException("user already exists", signUpModel.Email);
            }

            var newUser = new User();
            this.mapper.Map(signUpModel, newUser);
            newUser.UserName = newUser.Email;
            newUser.NormalizedUserName = newUser.Email;

            return await this.userIdentityManager.CreateAsync(newUser, signUpModel.Password);
        }

        /// <summary>
        /// Logs In the User.
        /// </summary>
        /// <param name="loginModel">The log in model.</param>
        /// <returns>
        /// True if logged in correctly.
        /// </returns>
        public async Task<bool> LogInAsync(LogInModel loginModel)
        {
            var signInResult = await this.userIdentitySignManager.PasswordSignInAsync(loginModel.Email, loginModel.Password, true, lockoutOnFailure: true);
            return signInResult.Succeeded;
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

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>
        /// Task.
        /// </returns>
        public async Task ResetPassword(string email, string newPassword)
        {
            var user = await this.usersRepository.GetAsync(p => p.Email == email);
            var hashedPassword = this.passwordHasher.HashPassword(user, newPassword);
            user.PasswordHash = hashedPassword;
            this.usersRepository.Update(user);
            this.usersRepository.Save();
        }
    }
}