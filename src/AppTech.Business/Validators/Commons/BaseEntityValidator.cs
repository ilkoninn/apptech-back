using AppTech.Business.DTOs.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.Commons
{
    public class BaseEntityValidator<T> : AbstractValidator<T> where T : BaseEntityDTO
    {
        public BaseEntityValidator()
        {
            RuleFor(entity => entity.Id)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Id is required.")
                .GreaterThan(0).WithMessage("Id must be greater than 0.");
        }
    }
}
