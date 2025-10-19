using AppTech.Business.DTOs.ContactUsDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.ContactUsValidator
{
    public class UpdateContactUsDTOValidator : BaseEntityValidator<UpdateContactUsDTO>
    {
        public UpdateContactUsDTOValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(100).WithMessage("Full Name must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.AppUserId)
                .NotEmpty().WithMessage("App User Id is required.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(1000).WithMessage("Message must not exceed 1000 characters.");
        }
    }
}
