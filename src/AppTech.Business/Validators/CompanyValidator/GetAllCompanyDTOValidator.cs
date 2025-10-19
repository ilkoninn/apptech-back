using AppTech.Business.DTOs.CompanyDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.CompanyValidator
{
    public class GetAllCompanyDTOValidator : AbstractValidator<GetAllCompanyDTO>
    {
        public GetAllCompanyDTOValidator()
        {
            RuleFor(x => x.isTop)
               .NotNull().WithMessage("IsTop field is required.")
               .Must(x => x == true || x == false).WithMessage("IsTop field must be either true or false.");
        }
    }

}
