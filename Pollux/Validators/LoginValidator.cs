namespace Pollux.API.Validators
{
    using FluentValidation;
    using Pollux.Common.Application.Models.Request;

    public class LoginValidator : AbstractValidator<LogInModel>
    {
        public LoginValidator()
        {
            this.RuleFor(p => p.Password).NotEmpty().NotNull().MinimumLength(8);
            this.RuleFor(p => p.Email).EmailAddress().NotEmpty();
        }
    }
}
