using AppTech.Business.DTOs.CommentDTOs;
using AppTech.Business.Validators.Commons;
using FluentValidation;

namespace AppTech.Business.Validators.CommentValidator
{
    public class UpdateCommentDTOValidator : BaseEntityValidator<UpdateCommentDTO>
    {
        public UpdateCommentDTOValidator()
        {
            RuleFor(x => x.userId)
                .NotEmpty().WithMessage("AppUserId is required.");

            RuleFor(x => x.subject).Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Message is required.")
                .MaximumLength(500).WithMessage("Message must not exceed 500 characters.");
        }
    }
}
