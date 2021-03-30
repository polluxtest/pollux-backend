using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pollux.Application.OAuth
{
    using System.Collections.Specialized;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using IdentityServer4.Models;
    using IdentityServer4.Validation;


    public class TokenValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class UserValidator : IResourceOwnerPasswordValidator
    {
        private readonly Dictionary<string, string> users;


        public UserValidator()
        {
            this.users = new Dictionary<string, string>() {
                                                                  { "octa@gmail.com", "apolo100" },
                                                   { "octavio.diaz@gmail.com", "apolo100" }
                                                              };
        }

        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var username = context.UserName;
            var password = context.Password;

            if (this.users.Any(x => x.Key == username && x.Value == password))
            {
                // context set to success
                context.Result = new GrantValidationResult(
                    subject: context.UserName,
                    authenticationMethod: "user_credentials",
                    claims: new Claim[] { new Claim(ClaimTypes.Email, username) });

                return Task.FromResult(0);
            }

            // context set to Failure        
            context.Result = new GrantValidationResult(
                TokenRequestErrors.UnauthorizedClient, "Invalid Crdentials");

            return Task.FromResult(0);
        }
    }
}
