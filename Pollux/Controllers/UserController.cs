namespace Pollux.API.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

    using Pollux.Application;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;

    [Route(ApiConstants.DefaultRoute)]
    [ApiController]
    [Authorize]
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
            //var token = await HttpContext.GetTokenAsync("access_token");
            //var refresh = await HttpContext.GetTokenAsync("refresh_token");
            //var cookie = HttpContext.Response.Cookies;
            var token = await this.userService.SetAuth(this.User);
            Console.WriteLine(token.ExpiresIn);
            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(
                    token.AccessToken,
                    validationParameters,
                    out SecurityToken validtedToken);

                // Or, you can return the ClaimsPrincipal
                // (which has the JWT properties automatically mapped to .NET claims)
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
            }
            catch (Exception e)
            {
            }

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

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return this.Content("authorizedr");
        }
    }
}
