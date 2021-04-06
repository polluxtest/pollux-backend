namespace Pollux.API.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Application;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;

    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUsersService userService;
        private readonly IAuthService authService;

        public UserController(IUsersService userService, IAuthService authService)
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
        public async Task<ActionResult> SignUp([FromBody] SignUpModel signUpModel, CancellationToken cancellationToken = default(CancellationToken))
        {
            var identityResponse = await this.userService.SignUp(signUpModel, cancellationToken);
            return this.Created(string.Empty, identityResponse);
        }

        /// <summary>
        /// Logs the in.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns>201.</returns>
        [HttpPost]
        [AllowAnonymous]
        [Route(ApiConstants.LogIn)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<Claim>>> LogIn([FromBody] LogInModel loginModel)
        {
            await this.userService.LogInAsync(loginModel);
            var token = await this.authService.SetAuth(loginModel);
            return this.Created(string.Empty, token);
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
            await this.userService.LogOutAsync();
            await this.authService.RemoveAuth(username);

            return this.NoContent();
        }

        [HttpPost]
        [Route("Test")]
        public async Task<IActionResult> Test()
        {
            return this.Content(this.User.Claims.First(p => p.Type.Equals("email")).Value);
        }
    }
}
