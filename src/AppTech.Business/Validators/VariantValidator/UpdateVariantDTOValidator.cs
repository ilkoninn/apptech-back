using AppTech.Business.DTOs.VariantDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.VariantValidator
{
    public class UpdateVariantDTOValidator : BaseEntityValidator<UpdateVariantDTO>
    {
        public UpdateVariantDTOValidator()
        {
            RuleFor(dto => dto.QuestionId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("QuestionId is required.")
                .GreaterThan(0).WithMessage("QuestionId must be greater than 0.");

            RuleFor(dto => dto.Text).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Text is required.");
        }
    }
}
