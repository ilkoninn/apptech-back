using AppTech.Business.DTOs.QuestionDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.QuestionValidator
{
    public class UpdateQuestionDTOValidator : BaseEntityValidator<UpdateQuestionDTO>
    {
        public UpdateQuestionDTOValidator()
        {
            RuleFor(dto => dto.Type)
               .IsInEnum().WithMessage("Invalid question type.");

            RuleFor(dto => dto.CertificationId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("CertificationId is required.")
                .GreaterThan(0).WithMessage("CertificationId must be greater than 0.");

            RuleFor(dto => dto.Content)
                .NotEmpty().WithMessage("Title is required.");

            RuleFor(dto => dto.Point)
                .GreaterThanOrEqualTo(0).WithMessage("Point must be greater than or equal to 0.");
        }
    }
}
