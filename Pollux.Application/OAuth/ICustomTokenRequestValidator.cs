using System;
using System.Collections.Generic;
using System.Text;

namespace Pollux.Application.OAuth
{
    using System.Threading.Tasks;

    using IdentityServer4.Validation;

    public class CustomTokenRequestValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            var c = context;
            return Task.CompletedTask;
        }
    }
}
