using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Pollux.API.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate nextDelegate;

        public ExceptionMiddleware(RequestDelegate nextDelegate)
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
            catch (Exception)
            {
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await httpContext.Response.WriteAsync("Unexpected Error");
            }
        }
    }
}
