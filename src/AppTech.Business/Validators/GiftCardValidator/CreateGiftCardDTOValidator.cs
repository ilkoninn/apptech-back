
using AppTech.Business.DTOs.GiftCardDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.GiftCardDTOs
{
    public class CreateGiftCardDTOsValidator : AbstractValidator<CreateGiftCardDTO>
    {
        public CreateGiftCardDTOsValidator()
        {
            RuleFor(x => x.Type)
                .IsInEnum().WithMessage("Gift card type is invalid.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.");

            RuleFor(dto => dto.Image).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The Image field is required.");
        }
    }
}
