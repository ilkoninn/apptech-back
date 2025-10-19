using AppTech.Business.DTOs.CertificationDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.CertificationValidator
{
    public class CreateCertificationDTOValidator : AbstractValidator<CreateCertificationDTO>
    {

        public CreateCertificationDTOValidator()
        {
            RuleFor(dto => dto.CompanyId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("CompanyId is required.")
                .GreaterThan(0).WithMessage("CompanyId must be greater than 0.");

            RuleFor(dto => dto.Code).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Code field is required.")
                .MaximumLength(100).WithMessage("Code must not exceed 100 characters.");

            RuleFor(dto => dto.Price).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Price is required.")
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(dto => dto.Image).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Image field is required.");
        }
    }
    public class CreateCertificationTranslationDTOValidator : AbstractValidator<CreateCertificationTranslationDTO>
    {

        public CreateCertificationTranslationDTOValidator()
        {
            RuleFor(dto => dto.CertificationId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("CompanyId is required.")
                .GreaterThan(0).WithMessage("CompanyId must be greater than 0.");

            RuleFor(dto => dto.Description).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Detail field is required.")
                .MaximumLength(1000).WithMessage("Detail must not exceed 1000 characters.");
        }
    }
}
