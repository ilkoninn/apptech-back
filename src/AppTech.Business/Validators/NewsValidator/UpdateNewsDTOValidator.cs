using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.NewsValidator
{
    public class UpdateNewsDTOValidator : BaseEntityValidator<UpdateNewsDTO>
    {
        public UpdateNewsDTOValidator()
        {
            RuleFor(x => x.Image)
                .NotEmpty().WithMessage("The Image field is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("The Title field is required.");

            RuleFor(dto => dto.Url).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Url field is required.")
                .MaximumLength(200).WithMessage("URL must not exceed 200 characters.")
                .Matches(@"^(https?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$")
                .WithMessage("Invalid URL format.");
        }
    }
    public class UpdateNewsTranslationDTOValidator : BaseEntityValidator<UpdateNewsTranslationDTO>
    {
        public UpdateNewsTranslationDTOValidator()
        {
            RuleFor(x => x.NewsId)
                .NotEmpty().WithMessage("NewsId is required")
                .GreaterThan(0).WithMessage("NewsId must be greater than 0.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("The Description field is required.");
        }
    }
}
