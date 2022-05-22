namespace Pollux.API.Middlewares
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Pollux.Common.Constants.Strings;
    using Pollux.Common.Exceptions;

    public class ExceptionMiddleware
    {
        private readonly RequestDelegate nextDelegate;
        private ILogger logger;

        public ExceptionMiddleware(RequestDelegate nextDelegate, ILogger logger)
        {
            this.nextDelegate = nextDelegate;
            this.logger = logger;
        }

        /// <summary>
        /// Invokes the specified HTTP context and checks if the refresh token or authentication is invalid.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <returns>Task.</returns>
        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                await this.nextDelegate.Invoke(httpContext);
            }
            catch (NotAuthenticatedException)
            {
                httpContext.Response.StatusCode = 440;
                await httpContext.Response.WriteAsync(MessagesConstants.NotAuthenticated);
            }
            catch (Exception ex)
            {
                this.logger.LogError("UnExpected Fall Back 500 Error");
                this.logger.LogError(ex.Message);
                this.logger.LogError(ex.StackTrace);
                this.logger.LogError(ex.InnerException?.Message);
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync(MessagesConstants.UnExpectedError);
            }
        }
    }
}
