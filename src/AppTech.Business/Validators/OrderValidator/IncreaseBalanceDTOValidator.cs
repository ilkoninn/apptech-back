using AppTech.Business.DTOs.TransactionDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.OrderValidator
{
    public class IncreaseBalanceDTOValidator : AbstractValidator<IncreaseBalanceDTO>
    {
        public IncreaseBalanceDTOValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty()
                .WithMessage("Please enter username or email address.");

            RuleFor(x => x.amount)
                .NotEmpty()
                .WithMessage("Please enter your password.")
                .GreaterThan(4)
                .WithMessage("Minimum amount must be 5 azn.");
        }
    }
}
