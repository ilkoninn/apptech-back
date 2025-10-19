using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.UserValidators
{
    public class ResetPasswordUserDTOValidator : AbstractValidator<ResetPasswordUserDTO>
    {
        public ResetPasswordUserDTOValidator()
        {
            RuleFor(x => x.newPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one numeric digit.");

            RuleFor(x => x.confirmNewPassword)
                .Equal(x => x.newPassword).WithMessage("Passwords do not match.");

            RuleFor(x => x.email).Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage("Email address is required.")
               .EmailAddress()
               .WithMessage("Invalid email address.");

            RuleFor(x => x.token)
               .Cascade(CascadeMode.Stop)
               .NotEmpty()
               .WithMessage("Token is required.");
        }
    }
}
