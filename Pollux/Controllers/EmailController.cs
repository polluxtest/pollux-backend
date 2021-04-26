namespace Pollux.API.Controllers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.ServerLess;

    [Authorize]
    public class EmailController : BaseController
    {
        private readonly HttpClient httpClient;

        public EmailController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Post(SendMailModel emailModel)
        {
            UriBuilder uriBuilder = new UriBuilder(AzureFunctionConstants.SendMailUrlAddress);
            uriBuilder.Query += $"/?Name={emailModel.Name}&Type={emailModel.Type}&To={emailModel.To}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            var response = await this.httpClient.SendAsync(httpRequest);

            return this.Ok(response);
        }
    }
}
