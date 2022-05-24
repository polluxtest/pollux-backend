namespace Pollux.API.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Pollux.Application;
    using Pollux.Application.Services;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Constants.Strings.Api;
    using Pollux.Common.Exceptions;
    using Pollux.Common.Factories;

    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUsersService userService;
        private readonly IAuthService authService;
        private readonly ILogger logger;

        public UsersController(IUsersService userService, IAuthService authService, ILogger logger)
        {
            this.userService = userService;
            this.authService = authService;
            this.logger = logger;
        }

        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="signUpModel">The sign up model.</param>
        /// <returns>201.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.SignUp)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> SignUp([FromBody] SignUpModel signUpModel)
        {
            var identityResponse = await this.userService.SignUp(signUpModel);
            if (identityResponse.Succeeded)
            {
                return this.Created(string.Empty, identityResponse);
            }

            return this.BadRequest();
        }

        /// <summary>
        /// Logs the in.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>201.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.LogIn)]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> LogIn([FromBody] LogInModel loginModel)
        {
            var succeed = await this.userService.LogInAsync(loginModel);
            this.logger.LogInformation($"logged in response information {succeed?.Name} {succeed?.UserId}");
            if (succeed != null)
            {
                var token = await this.authService.SetAuth(loginModel);
                this.logger.LogInformation($"logged in response token {token}");
                this.logger.LogInformation($"logged in response token {token.AccessToken}");

                this.HttpContext.Response.Cookies.Append(CookiesConstants.CookieAccessTokenName, token.AccessToken);

                return this.Ok();
            }

            return this.NotFound();
        }

        /// <summary>
        /// Logs the out.
        /// </summary>
        /// <returns>No Content (204).</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.LogOut)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> LogOut()
        {
            var username = this.User.Claims.First(p => p.Type.Equals(ClaimTypes.Email)).Value;

            await this.SignOut(username);

            return this.NoContent();
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="resetPasswordModel">The reset password model.</param>
        /// <returns>200.Ok.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.ResetPassword)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            try
            {
                var email = TokenFactory.DecodeToken(resetPasswordModel.Token);
                var userExists = await this.userService.ExistUser(email);
                if (!userExists)
                {
                    return this.NotFound();
                }

                await this.userService.ResetPassword(email, resetPasswordModel.NewPassword);
                return this.Ok();
            }
            catch (ArgumentException)
            {
                return this.NotFound();
            }
        }

        /// <summary>
        /// Check if the user Exists.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Redirect to correct path.</returns>
        [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
        [AllowAnonymous]
        [HttpGet]
        [Route(ApiConstants.Exist)]

        public async Task<IActionResult> Exist([FromQuery] string email)
        {
            var exists = await this.userService.ExistUser(email);
            return this.Ok(exists);
        }

        /// <summary>
        /// Determines whether [is user authenticated].
        /// </summary>
        /// <returns>No Content.</returns>
        [Authorize]
        [HttpGet]
        [Route(ApiConstants.IsUserAuthenticated)]
        [ProducesResponseType(204)]
        public async Task<ActionResult<string>> IsUserAuthenticated()
        {
            return new EmptyResult();
        }

        /// <summary>
        /// Represents an event that is raised when the sign-out operation is complete.
        /// </summary>
        /// <param name="username">The username.</param>
        private async Task SignOut(string username)
        {
            await this.HttpContext.SignOutAsync();
            await this.userService.LogOutAsync();
            await this.authService.RemoveAuth(username);
        }
    }
}
