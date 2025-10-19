using AppTech.Business.DTOs.ContactUsDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.ContactUsValidator
{
    public class CreateContactUsDTOValidator : AbstractValidator<CreateContactUsDTO>
    {
        public CreateContactUsDTOValidator()
        {
            RuleFor(x => x.fullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters.");

            RuleFor(x => x.email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.message)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.");
        }
    }
}
