using System.Collections.Specialized;

namespace Pollux.Application.Serverless
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Pitcher;
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
        public async Task<HttpResponseMessage> Send(SendEmailModel emailModel)
        {
            Throw.When(emailModel == null, new ArgumentException("Email object is null", nameof(emailModel)));

            UriBuilder uriBuilder = new UriBuilder(AzureFunctionConstants.SendMailUrlAddress);

            // todo use this to create the query string

            NameValueCollection queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("key1", "value1");
            queryString.Add("key2", "value2");

            queryString.Add("key1", "value1");
            queryString.Add("key2", "value2");
            uriBuilder.Query += $"{EmailParametersConstants.Name}={emailModel.Name}&" +
                                $"{EmailParametersConstants.Type}={emailModel.Type}&" +
                                $"{EmailParametersConstants.To}={emailModel.To}&" +
                                $"{EmailParametersConstants.Text}={emailModel.Text}&" +
                                $"{EmailParametersConstants.Topic}={emailModel.Topic}&" +
                                $"{EmailParametersConstants.FromEmail}={emailModel.FromEmail}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uriBuilder.Uri);
            var response = await this.httpClient.SendAsync(httpRequest);

            return response;
        }
    }
}
