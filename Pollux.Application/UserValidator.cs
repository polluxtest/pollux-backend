namespace Pollux.Application
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using IdentityServer4.Models;
    using IdentityServer4.Validation;

    public class UserValidator : IResourceOwnerPasswordValidator
    {
        private readonly IUsersService userService;

        public UserValidator(
            IUsersService userService)
        {
            this.userService = userService;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            var username = context.UserName;

            if (!await this.userService.ExistUser(username)) //todo find a way to remove this.
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "Invalid Credentials");
            }
            else
            {
                context.Result = new GrantValidationResult(
                    subject: context.UserName,
                    authenticationMethod: "user_credentials",
                    claims: new Claim[] { new Claim(ClaimTypes.Email, username) });
            }
        }
    }
}
