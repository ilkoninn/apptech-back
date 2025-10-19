using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.UserValidators
{
    public class ClickToResendDTOValidator : AbstractValidator<ClickToResendDTO>
    {
        public ClickToResendDTOValidator()
        {
            RuleFor(x => x.email).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email address is required.")
                .EmailAddress()
                .WithMessage("Invalid email address.");
        }
    }
}
