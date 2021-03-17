namespace Pollux.API.Controllers
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Application;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;

    [Route(ApiConstants.DefaultRoute)]
    [AllowAnonymous]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUsersService userService;

        public UserController(IUsersService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Logs the in.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>201.</returns>
        [HttpPost]
        [Route(ApiConstants.LogIn)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<Claim>>> LogIn([FromBody] LogInModel loginModel)
        {

            await this.userService.LogInAsync(loginModel);
            //var token = await HttpContext.GetTokenAsync("access_token");
            //var refresh = await HttpContext.GetTokenAsync("refresh_token");
            //var cookie = HttpContext.Response.Cookies;
            var token = await this.userService.SetAuth(this.User);
            return this.Created(string.Empty, token);
        }

        /// <summary>
        /// Signs up.
        /// </summary>
        /// <param name="signUpModel">The sign up model.</param>
        /// <returns>201.</returns>
        [HttpPost]
        [Route(ApiConstants.SignUp)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> SignUp([FromBody] SignUpModel signUpModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            var r = await this.userService.SignUp(signUpModel, cancellationToken).ConfigureAwait(false);
            return this.Created(string.Empty, r);
        }

        /// <summary>
        /// Logs the user out.
        /// </summary>
        /// <returns>204.</returns>
        //[HttpPost]
        //[Route(ApiConstants.LogOut)]
        //[ProducesResponseType(204)]
        //public Task<IActionResult> LogOut()
        //{
        //    this.userService.LogOutAsync();

        //    return this.NoContent();
        //}
    }
}
