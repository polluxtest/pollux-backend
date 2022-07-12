namespace Pollux.API.Validators
{
    using FluentValidation;

    using Pollux.Common.Application.Models.Request;

    public class PreferencesValidator : AbstractValidator<UserPreferencesPostModel>
    {
        public PreferencesValidator()
        {
            this.RuleFor(p => p.UserId).NotNull();
            this.RuleFor(p => p.Preferences).NotNull();
        }
    }
}