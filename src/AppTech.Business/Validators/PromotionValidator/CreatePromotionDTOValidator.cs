using AppTech.Business.DTOs.PromotionDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.PromotionValidator
{
    public class CreatePromotionDTOValidator : AbstractValidator<CreatePromotionDTO>
    {
        public CreatePromotionDTOValidator()
        {
            RuleFor(x => x.Code).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

            RuleFor(x => x.Percentage)
                .NotEmpty().WithMessage("Percentage is required.");



            //RuleFor(x => x.Certifications).Cascade(CascadeMode.Stop)
            //    .NotNull().WithMessage("Certifications list cannot be null.")
            //    .NotEmpty().WithMessage("Certifications list cannot be empty.")
            //    .Must(x => x != null && x.Count > 0).WithMessage("Certifications list cannot be empty.");

        }
    }

    public class UsePromotionDTOValidator : AbstractValidator<UsePromotionDTO>
    {
        public UsePromotionDTOValidator()
        {
            RuleFor(x => x.Code).Cascade(CascadeMode.Stop)
              .NotEmpty().WithMessage("Code is required.")
              .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

            RuleFor(x => x.CertificationId)
                .NotEmpty().WithMessage("CertificationId is required");
        }
    }
}
