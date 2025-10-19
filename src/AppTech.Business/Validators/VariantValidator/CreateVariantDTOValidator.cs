using AppTech.Business.DTOs.VariantDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.VariantValidator
{
    public class CreateVariantDTOValidator : AbstractValidator<CreateVariantDTO>
    {
        public CreateVariantDTOValidator()
        {

            RuleFor(dto => dto.QuestionId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("QuestionId is required.")
                .GreaterThan(0).WithMessage("QuestionId must be greater than 0.");

            RuleFor(dto => dto.Text).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Text is required.");
        }
    }
}
