using AppTech.Business.DTOs.PartnerDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.PartnerValidator
{
    public class CreatePartnerOnlineDTOValidator : AbstractValidator<CreatePartnerOnlineDTO>
    {
        public CreatePartnerOnlineDTOValidator()
        {
            RuleFor(dto => dto.name).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The name field is required.");

            RuleFor(dto => dto.surName).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The surname field is required.");

            RuleFor(dto => dto.phone).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The phone field is required.");

            RuleFor(dto => dto.email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The email field is required.");

            RuleFor(dto => dto.position).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The position field is required.");

            RuleFor(dto => dto.company).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The company field is required.");
        }
    }
}
