
using AppTech.Business.DTOs.GiftCardDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.GiftCardDTOs
{
    public class UpdateGiftCardDTOsValidator : BaseEntityValidator<UpdateGiftCardDTO>
    {
        public UpdateGiftCardDTOsValidator()
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
