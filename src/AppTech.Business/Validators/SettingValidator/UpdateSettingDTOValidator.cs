using AppTech.Business.DTOs.SettingDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.SettingValidator
{
    public class UpdateSettingDTOValidator : BaseEntityValidator<UpdateSettingDTO>
    {
        public UpdateSettingDTOValidator()
        {
            RuleFor(dto => dto.Key).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Key field is required.");
        }
    }
    public class UpdateSettingTranslationDTOValidator : BaseEntityValidator<UpdateSettingTranslationDTO>
    {
        public UpdateSettingTranslationDTOValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id field is required");


            RuleFor(x => x.Value)
                .NotEmpty().WithMessage("Value field is required");
        }
    }
}
