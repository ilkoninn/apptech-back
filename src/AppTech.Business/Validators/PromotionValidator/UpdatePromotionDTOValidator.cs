using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.CertificationValidator
{
    public class UpdatePromotionDTOValidator : BaseEntityValidator<UpdatePromotionDTO>
    {
        public UpdatePromotionDTOValidator()
        {
            RuleFor(x => x.Code).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(50).WithMessage("Code must not exceed 50 characters.");

            RuleFor(x => x.Percentage)
                .NotEmpty().WithMessage("Percantage is required.");
        }
    }
}
