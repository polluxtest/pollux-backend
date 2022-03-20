namespace Pollux.API.Validators
{
    using FluentValidation;
    using Pollux.Common.Application.Models.Request;

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordModel>
    {
        public ResetPasswordValidator()
        {
            this.RuleFor(p => p.NewPassword).NotEmpty().NotNull().MinimumLength(8);
            this.RuleFor(p => p.Token).NotEmpty().NotNull();
        }
    }
}
