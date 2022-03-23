namespace Pollux.API.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Application;
    using Pollux.Application.Services;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Constants.Strings.Api;
    using Pollux.Common.Factories;

    [Authorize]
    public class UsersController : BaseController
    {
        private readonly IUsersService userService;
        private readonly IAuthService authService;

        public UsersController(IUsersService userService, IAuthService authService)
        {
            this.userService = userService;
            this.authService = authService;
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
                var token = await this.authService.SetAuth(loginModel);
                this.HttpContext.Response.Cookies.Append(CookiesConstants.CookieAccessTokenName, token.AccessToken);

                return this.Ok(succeed);
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
            await this.HttpContext.SignOutAsync();
            await this.userService.LogOutAsync();
            await this.authService.RemoveAuth(username);

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
        /// Check if the user Exists.
        /// </summary>
        /// <returns>Redirect to correct path.</returns>
        [Authorize]
        [HttpGet]
        [Route(ApiConstants.Test)]
        public async Task<ActionResult<string>> Test()
        {
            return this.Ok("authorized");
        }
    }
}
