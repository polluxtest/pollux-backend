namespace Pollux.API.Controllers
{
    using System.Threading.Tasks;
    using Application.Serverless;
    using Common.Constants.Strings.Api;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.Auth;

    [Authorize(AuthenticationSchemes = AuthConstants.TokenAuthenticationDefaultScheme)]
    public class EmailController : BaseController
    {
        private readonly ISendEmail sendEmail;

        public EmailController(ISendEmail sendEmail)
        {
            this.sendEmail = sendEmail;
        }

        /// <summary>
        /// Posts the specified email model.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <returns>200/500</returns>
        [Route(ApiConstants.SendEmail)]
        [HttpPost]
        [ProducesResponseType(200)]
        public async Task<ActionResult> Post(SendEmailModel emailModel)
        {
            try
            {
                var response = await this.sendEmail.Send(emailModel);
                return this.Ok(response);
            }
            catch
            {
                return this.Problem("Email Sent Failed", null, StatusCodes.Status500InternalServerError);
            }
        }
    }
}