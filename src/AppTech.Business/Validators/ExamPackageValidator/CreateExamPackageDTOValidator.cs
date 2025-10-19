using AppTech.Business.DTOs.ExamPackageDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.ExamPackageValidator
{
    public class CreateExamPackageDTOValidator : AbstractValidator<CreateExamPackageDTO>
    {
        public CreateExamPackageDTOValidator()
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Price must be a non-negative decimal.");
        }
    }
}
