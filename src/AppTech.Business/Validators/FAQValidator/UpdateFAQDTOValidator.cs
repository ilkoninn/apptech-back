using AppTech.Business.DTOs.FAQDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.FAQDTO
{
    public class UpdateFAQDTOValidator : BaseEntityValidator<UpdateFAQDTO>
    {
        public UpdateFAQDTOValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("The Question field is required.");

            RuleFor(x => x.Answer)
                .NotEmpty().WithMessage("The Answer field is required.");
        }
    }
}
