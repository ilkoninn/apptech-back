using AppTech.Business.DTOs.ExamPackageDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.ExamPackageValidator
{
    public class UpdateExamPackageDTOValidator : AbstractValidator<UpdateExamPackageDTO>
    {
        public UpdateExamPackageDTOValidator()
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a non-negative decimal.");
        }
    }
}
