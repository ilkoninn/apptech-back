using AppTech.Business.DTOs.GiftCardDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.GiftCardValidator
{
    public class GiftcardRecipientDTOValidator : AbstractValidator<GiftcardRecipientDTO>
    {
        public GiftcardRecipientDTOValidator()
        {
            RuleFor(x => x.giftId)
                .NotEmpty().WithMessage("Gift card id is invalid.");

            RuleFor(x => x.description)
                .NotEmpty().WithMessage("The description field is required.");

            RuleFor(dto => dto.email).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The email field is required.");
        }
    }
}
