using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.UserValidators
{
    public class ConfirmForgotPasswordDTOValidator : AbstractValidator<ForgotPasswordConfirmationDTO>
    {
        public ConfirmForgotPasswordDTOValidator()
        {
            RuleFor(x => x.email).Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage("Email address is required.")
               .EmailAddress()
               .WithMessage("Invalid email address.");

            RuleFor(x => x.number)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage("Confirmation code is required.")
               .Must(number => number >= 100000 && number <= 999999)
               .WithMessage("Confirmation code must be a 6-digit number.");
        }
    }
}
