using AppTech.Business.DTOs.NewsDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.NewsValidator
{
    public class CreateNewsDTOValidator : AbstractValidator<CreateNewsDTO>
    {
        public CreateNewsDTOValidator()
        {
            RuleFor(dto => dto.Image).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The Image field is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("The Title field is required.");

            RuleFor(dto => dto.Url).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("The Url field is required.")
                .MaximumLength(200).WithMessage("URL must not exceed 200 characters.")
                .Matches(@"^(https?://)?([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?$")
                .WithMessage("Invalid URL format.");
        }
    }
    public class CreateNewsTranslationDTOValidator : AbstractValidator<CreateNewsTranslationDTO>
    {
        public CreateNewsTranslationDTOValidator()
        {
            RuleFor(x => x.NewsId)
                .NotEmpty().WithMessage("NewsId is required")
                .GreaterThan(0).WithMessage("NewsId must be greater than 0.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");
        }
    }
}
