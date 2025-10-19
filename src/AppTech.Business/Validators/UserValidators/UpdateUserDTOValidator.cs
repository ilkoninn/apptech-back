using AppTech.Business.DTOs.UserDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.UserValidators
{
    public class UpdateUserDTOValidator : AbstractValidator<UpdateUserDTO>
    {
        public UpdateUserDTOValidator()
        {
            RuleFor(x => x.email).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Please enter a valid email address.");

            RuleFor(x => x.username).Cascade(CascadeMode.Stop)
                 .NotEmpty()
                 .WithMessage("Username is required.")
                 .MinimumLength(3)
                 .WithMessage("Username must be at least 3 characters long.");

            RuleFor(x => x.fullName).Cascade(CascadeMode.Stop)
                 .MinimumLength(5)
                 .WithMessage("Full name must be at least 5 characters long.")
                 .MaximumLength(40)
                 .WithMessage("Full name must be at least 40 characters long.");
        }
    }
}
