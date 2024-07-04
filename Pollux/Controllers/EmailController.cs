namespace Pollux.API.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Application.Serverless;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Api;

    [Authorize]
    public class EmailController : BaseController
    {
        private readonly ISendEmail sendEmail;

        public EmailController(ISendEmail sendEmail)
        {
            this.sendEmail = sendEmail;
        }

        [Route(ApiConstants.SendEmail)]
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post(SendEmailModel emailModel)
        {
            var response = await this.sendEmail.Send(emailModel);

            return this.Ok(response);
        }
    }
}