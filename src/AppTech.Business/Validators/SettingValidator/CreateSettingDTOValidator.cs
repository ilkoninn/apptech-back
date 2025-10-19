using AppTech.Business.DTOs.SettingDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.SettingValidator
{
    public class CreateSettingDTOValidator : AbstractValidator<CreateSettingDTO>
    {
        public CreateSettingDTOValidator()
        {
            RuleFor(dto => dto.Key).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Key field is required.");


        }
    }
    public class CreateSettingTranslationDTOValidator : AbstractValidator<CreateSettingTranslationDTO>
    {
        public CreateSettingTranslationDTOValidator()
        {
            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value is required");
        }
    }
}
