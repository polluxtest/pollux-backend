namespace Pollux.Application.Serverless
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Pitcher;
    using Pollux.Common.Application.Models.Request;
    using Pollux.Common.Constants.Strings.ServerLess;
    using Pollux.Common.Constants.Strings;

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
        private readonly AzureServerlessSettings serverlessSettings;

        public SendEmail(HttpClient httpClient, AzureServerlessSettings serverlessSettings)
        {
            this.httpClient = httpClient;
            this.serverlessSettings = serverlessSettings;
        }

        /// <summary>
        /// Sends the specified email model to a server less function hosted on azurre.
        /// </summary>
        /// <param name="emailModel">The email model.</param>
        /// <returns>HttpResponseMessage.</returns>
        public async Task<HttpResponseMessage> Send(SendEmailModel emailModel)
        {
            Throw.When(
                emailModel == null,
                new ArgumentException(MessagesConstants.EmailEmptyError, nameof(emailModel)));

            var emailParameters = this.AddQueryStringParams(emailModel);

            var httpRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"{this.serverlessSettings.EmailAzureFunctionUrl}{emailParameters}");

            var response = await this.httpClient.SendAsync(httpRequest);
            return response;
        }

        /// <summary>
        /// Adds the query string parameters.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>Query String Parameters</returns>
        private string AddQueryStringParams(SendEmailModel email)
        {
            var queryString = System.Web.HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(EmailParametersConstants.Name, email.Name);
            queryString.Add(EmailParametersConstants.Type, email.Type);
            queryString.Add(EmailParametersConstants.To, email.To);
            queryString.Add(EmailParametersConstants.Text, email.Text);
            queryString.Add(EmailParametersConstants.Topic, email.Topic);
            queryString.Add(EmailParametersConstants.FromEmail, email.FromEmail);

            return queryString.ToString();
        }
    }
}
