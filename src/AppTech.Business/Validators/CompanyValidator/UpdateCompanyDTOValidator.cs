using AppTech.Business.DTOs.CompanyDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators
{
    public class UpdateCompanyDTOValidator : BaseEntityValidator<UpdateCompanyDTO>
    {
        public UpdateCompanyDTOValidator()
        {
            RuleFor(dto => dto.Title).Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("Title field is required.");

            RuleFor(dto => dto.Image).Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("Image field is required.");

            RuleFor(dto => dto.IsTop).Cascade(CascadeMode.Stop)
               .NotNull().WithMessage("IsTop field is required.")
               .Must(x => x == true || x == false).WithMessage("IsTop field must be either true or false.");
        }
    }

    public class UpdateCompanyTranslationDTOValidator : BaseEntityValidator<UpdateCompanyTranslationDTO>
    {
        public UpdateCompanyTranslationDTOValidator()
        {
            RuleFor(dto => dto.CompanyId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("CompanyId is required.")
                .GreaterThan(0).WithMessage("CompanyId must be greater than 0.");

            RuleFor(dto => dto.SecTitle)
                 .NotEmpty().WithMessage("The SecTitle field is required.");

            RuleFor(dto => dto.SecDescription)
                .NotEmpty().WithMessage("The SecDescription field is required.");
        }
    }
}
