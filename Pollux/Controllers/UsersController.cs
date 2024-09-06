namespace Pollux.API.Controllers
{
    using System;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;

    using Pollux.Application.Services;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Constants.Strings.Api;
    using Pollux.Common.Constants.Strings.Auth;
    using Pollux.Common.Utilities;

    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUsersService userService;
        private readonly IAuthService authService;
        private readonly CookieOptionsConfig cookieConfiguration;
        private readonly ILogger logger;

        public UsersController(
            IUsersService userService,
            IAuthService authService,
            ILogger logger,
            CookieOptionsConfig cookieConfiguration)
        {
            this.userService = userService;
            this.authService = authService;
            this.logger = logger;
            this.cookieConfiguration = cookieConfiguration;
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
            if (succeed != null)
            {
                loginModel.UserId = succeed.UserId;
                var token = await this.authService.SetAuth(loginModel);
                this.HttpContext.Response.Cookies.Append(
                        CookiesConstants.CookieAccessTokenName,
                        token.AccessToken,
                        this.cookieConfiguration.GetOptions());

                var cookies = this.Response.Headers["set-cookie"];

                return this.Ok(new { succeed, cookies });
            }

            return this.NotFound();
        }

        /// <summary>
        /// Logout from app
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns>Task.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.LogOut)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> LogOut(string userId)
        {
            await this.SignOut(userId);

            return this.Ok();
        }

        /// <summary>
        /// Resets the password.
        /// </summary>
        /// <param name="resetPasswordModel">The reset password model.</param>
        /// <returns>200/404</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.ResetPassword)]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel resetPasswordModel)
        {
            try
            {
                var email = TokenUtility.GetUserIdFromToken(resetPasswordModel.Token);
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
        [AllowAnonymous]
        [HttpGet]
        [Route(ApiConstants.Exist)]
        public async Task<IActionResult> Exist([FromQuery] string email)
        {
            var exists = await this.userService.ExistUser(email);
            return this.Ok(exists);
        }


        /// <summary>
        /// Verifies if the tokens have not experired , other wise session must be closed or new access token is provdied
        /// </summary>
        /// <returns>Redirect to correct path.</returns>
        [Authorize(AuthenticationSchemes = AuthConstants.TokenAuthenticationDefaultScheme)]
        [HttpGet]
        [Route(ApiConstants.VerifyAuthetication)]
        public async Task<IActionResult> VerifyAuthetication()
        {
            var cookies = this.Response.Headers["set-cookie"];

            return this.Ok(cookies);
        }

        /// <summary>
        /// Represents an event that is raised when the sign-out operation is complete.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Task</returns>
        private async Task SignOut(string userId)
        {
            await this.HttpContext.SignOutAsync();
            await this.userService.LogOutAsync();
            await this.authService.RemoveAuth(userId);
        }
    }
}
