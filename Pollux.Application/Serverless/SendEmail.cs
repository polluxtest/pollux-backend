namespace Pollux.Application.Serverless
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.ServerLess;


    public interface ISendEmail
    {
        /// <summary>
        /// Sends the specified email model.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <returns>HttpResponseMessage.</returns>
        Task<HttpResponseMessage> Send(SendEmailModel emailModel = null);
    }

    public class SendEmail : ISendEmail
    {
        private readonly HttpClient httpClient;

        public SendEmail(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        /// <summary>
        /// Sends the specified email model to a server less function hosted on azurre.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <returns>HttpResponseMessage.</returns>
        public async Task<HttpResponseMessage> Send(SendEmailModel emailModel = null)
        {
            UriBuilder uriBuilder = new UriBuilder(AzureFunctionConstants.SendMailUrlAddress);
            uriBuilder.Query += $"/?Name={emailModel.Name}&Type={emailModel.Type}&To={emailModel.To}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);

            var response = await this.httpClient.SendAsync(httpRequest);

            return response;
        }
    }
}
