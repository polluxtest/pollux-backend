namespace Pollux.API
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class CookieOptionsConfig
    {
        private readonly IConfiguration configuration;

        public CookieOptionsConfig(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        /// <summary>
        /// Gets the options to set for all cookies.
        /// </summary>
        /// <returns>CookieOptions</returns>
        public CookieOptions GetOptions()
        {
            var frontEndUrl = this.configuration.GetSection("AppSettings")["FrontEndUrl"];

            var cookieOptions = new CookieOptions
            {
                // Set the secure flag, which Chrome's changes will require for SameSite none.
                // Note this will also require you to be running on HTTPS.
                Secure = true,

                // Set the cookie to HTTP only which is good practice unless you really do need
                // to access it client side in scripts.
                HttpOnly = true,

                // Add the SameSite attribute, this will emit the attribute with a value of none.
                // To not emit the attribute at all set
                // SameSite = (SameSiteMode)(-1)
                SameSite = SameSiteMode.Unspecified,

                IsEssential = true,
            };

            return cookieOptions;
        }
    }
}
