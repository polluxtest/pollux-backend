namespace Pollux.API.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Application;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;

    public class UserController : BaseController
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
        public async Task<ActionResult> LogIn([FromBody] LogInModel loginModel)
        {
            await this.userService.LogInAsync(loginModel);
            return this.Created(string.Empty, new object());
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
        public async Task<ActionResult> SignUp([FromBody] SignUpModel signUpModel)
        {
            this.userService.SignUp(signUpModel);
            return this.Created(string.Empty, new object());
        }
    }
}
