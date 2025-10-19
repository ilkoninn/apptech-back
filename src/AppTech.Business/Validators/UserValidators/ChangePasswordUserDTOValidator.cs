using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.UserValidators
{
    public class ChangePasswordUserDTOValidator : AbstractValidator<ChangePasswordUserDTO>
    {
        public ChangePasswordUserDTOValidator()
        {
            RuleFor(x => x.currentPassword).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one numeric digit.");

            RuleFor(x => x.newPassword)
                .Equal(x => x.newPassword).WithMessage("Passwords do not match.");
        }
    }
}
