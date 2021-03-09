namespace Pollux.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.API.Controllers;
    using Pollux.Application;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;

    /// <summary>
    /// Defines the <see cref="UserController" />.
    /// </summary>
    public class UserController : BaseController
    {
        private readonly IUsersService userService;
        public UserController(IUsersService userService)
        {
            this.userService = userService;
        }

        /// <summary>
        /// Gets the specified cancellation token.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Test Content</returns>
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<ActionResult> Get(CancellationToken cancellationToken = default)
        {
            return this.Content("Api Working Fine");
        }

        /// <summary>
        /// Logs the in.
        /// </summary>
        /// <param name="loginModel">The login model.</param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route(ApiConstants.LogIn)]
        public async Task<ActionResult> LogIn([FromBody] LogInModel loginModel)
        {
            this.userService.LogIn(loginModel);
            return this.Created(string.Empty, new object());
        }
    }
}
