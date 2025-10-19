using AppTech.Business.DTOs.PartnerDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.PartnerValidator
{
    public class UpdatePartnerDTOValidator : BaseEntityValidator<UpdatePartnerDTO>
    {
        public UpdatePartnerDTOValidator()
        {
            RuleFor(x => x.Image)
                .NotEmpty().WithMessage("The Image field is required.");

            RuleFor(x => x.Company)
                .NotEmpty().WithMessage("The Title field is required.");

            RuleFor(dto => dto.Url).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Url field is required.")
                .MaximumLength(200).WithMessage("URL must not exceed 200 characters.")
                .Matches(@"^(https?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$")
                .WithMessage("Invalid URL format.");
        }
    }
}
