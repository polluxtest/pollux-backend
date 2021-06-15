namespace Pollux.API.Middlewares
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Pollux.Common.Exceptions;

    public class NotAuthenticatedMiddleware
    {
        private readonly RequestDelegate nextDelegate;

        public NotAuthenticatedMiddleware(RequestDelegate nextDelegate)
        {
            this.nextDelegate = nextDelegate;
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
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync("Not Authenticated");
            }
        }
    }
}
