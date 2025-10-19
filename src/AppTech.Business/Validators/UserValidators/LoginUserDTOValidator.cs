using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.AppUserValidator
{
    public class LoginUserDTOValidator : AbstractValidator<LoginUserDTO>
    {
        public LoginUserDTOValidator()
        {
            RuleFor(x => x.usernameOrEmail)
                .NotEmpty()
                .WithMessage("Please enter username or email address.");

            RuleFor(x => x.password)
                .NotEmpty()
                .WithMessage("Please enter your password.");
        }
    }
}

