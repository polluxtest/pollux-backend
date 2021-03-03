namespace Pollux.Controllers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.API.Controllers;

    /// <summary>
    /// Defines the <see cref="UserController" />.
    /// </summary>
    public class UserController : BaseController
    {
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
    }
}
