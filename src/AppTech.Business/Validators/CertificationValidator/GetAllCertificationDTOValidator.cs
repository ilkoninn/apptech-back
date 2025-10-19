using AppTech.Business.DTOs.CertificationDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.CertificationValidator
{
    public class GetAllCertificationDTOValidator : AbstractValidator<GetAllCertificationDTO>
    {
        public GetAllCertificationDTOValidator()
        {
            RuleFor(x => x.isTrend)
               .NotNull().WithMessage("IsTrend field is required.")
               .Must(x => x == true || x == false).WithMessage("IsTrend field must be either true or false.");
        }
    }
}
