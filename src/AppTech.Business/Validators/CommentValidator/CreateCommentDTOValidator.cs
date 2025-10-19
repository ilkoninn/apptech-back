using AppTech.Business.DTOs.CommentDTOs;
using FluentValidation;

namespace AppTech.Business.Validators.CommentValidator
{
    public class CreateCommentDTOValidator : AbstractValidator<CreateCommentDTO>
    {
        public CreateCommentDTOValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("AppUserId is required.");

            RuleFor(x => x.subject).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(500).WithMessage("Message must not exceed 500 characters.");
        }
    }
}
