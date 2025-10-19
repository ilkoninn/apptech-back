using AppTech.Business.DTOs.ExamDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.ExamValidator
{
    public class UpdateExamDTOValidator : BaseEntityValidator<UpdateExamDTO>
    {
        public UpdateExamDTOValidator()
        {
            RuleFor(dto => dto.CertificationId).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("CertificationId is required.")
                .GreaterThan(0).WithMessage("CertificationId must be greater than 0.");

            RuleFor(x => x.Code).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(20).WithMessage("Code must not exceed 20 characters.");

            RuleFor(x => x.MaxScore)
                .GreaterThanOrEqualTo(0).WithMessage("Score must be a non-negative integer.");


            RuleFor(x => x.Duration)
                .GreaterThan(0).WithMessage("Duration must be a positive integer.");

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a non-negative decimal.");
        }
    }
    public class UpdateExamTranslationDTOValidator : BaseEntityValidator<UpdateExamTranslationDTO>
    {
        public UpdateExamTranslationDTOValidator()
        {
            RuleFor(x => x.ExamId)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("ExamId is required.")
                .GreaterThan(0).WithMessage("ExamId must be greater than 0.");

            RuleFor(x => x.Description).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Description is required.");
        }
    }
}
