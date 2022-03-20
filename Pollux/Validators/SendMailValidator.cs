namespace Pollux.API.Validators
{
    using FluentValidation;
    using Pollux.Common.Application.Models.Request;

    public class SendMailValidator : AbstractValidator<SendEmailModel>
    {
        public SendMailValidator()
        {
            this.RuleFor(p => p.Name).NotEmpty().NotNull();
            this.RuleFor(p => p.To).EmailAddress().NotEmpty().NotNull();
            this.RuleFor(p => p.Type).NotEmpty().NotNull();
        }
    }
}
