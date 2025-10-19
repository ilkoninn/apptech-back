using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.UserValidators
{
    public class ForgotPasswordUserDTOValidator : AbstractValidator<ForgotPasswordUserDTO>
    {
        public ForgotPasswordUserDTOValidator()
        {
            RuleFor(x => x.email).Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage("Email is required.")
               .EmailAddress()
               .WithMessage("Please enter a valid email address.");
        }
    }
}
