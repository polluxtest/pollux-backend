namespace Pollux.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

    using Pollux.Application;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;
    using Microsoft.AspNetCore.Authentication.Cookies;

    [Route(ApiConstants.DefaultRoute)]
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
        [AllowAnonymous]
        [Route(ApiConstants.LogIn)]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<IEnumerable<Claim>>> LogIn([FromBody] LogInModel loginModel)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateIssuerSigningKey = false
            };
            await this.userService.LogInAsync(loginModel);
            var isAuth = this.User.Identity.IsAuthenticated;

            var token = await this.userService.SetAuth(loginModel);
            var isAuth3 = this.User.Identity.IsAuthenticated;




            return this.Created(string.Empty, token);
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
            var r = await this.userService.SignUp(signUpModel, cancellationToken).ConfigureAwait(false);
            return this.Created(string.Empty, r);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return this.Content("authorized");
        }

        [Authorize]
        [HttpPatch]
        public async Task<IActionResult> Patch()
        {
            return this.Content("authorized patch");
        }

        [Authorize]
        [HttpPost]
        [Route("Denied")]
        public async Task<IActionResult> Denied()
        {
            return this.Content(this.User.Claims.First(p => p.Type.Equals("email")).Value);
        }


    }
}
