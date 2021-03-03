namespace Pollux.API.Controllers
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Common.Constants.Strings.Api;

    /// <summary>
    /// Defines the Base Controllers attributes for the core domain <see cref="BaseController" />.
    /// </summary>
    [Route(ApiConstants.DefaultRoute)]
    // [Authorize]
    [ApiController]
    public class BaseController : ControllerBase
    {
    }
}
