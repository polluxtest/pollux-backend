namespace Pollux.API.Validators
{
    using FluentValidation;
    using Pollux.Common.Application.Models.Request;

    public class SignUpValidator : AbstractValidator<SignUpModel>
    {
        public SignUpValidator()
        {
            this.RuleFor(p => p.Email).EmailAddress().NotEmpty().NotNull();
            this.RuleFor(p => p.Password).NotEmpty().NotNull().MinimumLength(8);
            this.RuleFor(p => p.Name).NotEmpty().NotNull();
        }
    }
}
