using AppTech.Business.DTOs.FAQDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.FAQDTO
{
    public class CreateFAQDTOValidator : AbstractValidator<CreateFAQDTO>
    {
        public CreateFAQDTOValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("The Question field is required.");

            RuleFor(x => x.Answer)
                .NotEmpty().WithMessage("The Answer field is required.");
        }
    }
}
