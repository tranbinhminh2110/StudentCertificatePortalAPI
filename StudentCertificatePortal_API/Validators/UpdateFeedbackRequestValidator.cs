using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateFeedbackRequestValidator : AbstractValidator<UpdateFeedbackRequest>
    {
        public UpdateFeedbackRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.FeedbackRatingvalue)
                .InclusiveBetween(0, 5)
                .WithMessage("Feedback rating must be between 1 and 5.");
            RuleFor(x => x.FeedbackDescription)
                .NotEmpty()
                .When(x => x.FeedbackRatingvalue == 0)
                .WithMessage("FeedbackDescription is required.");
        }
    }
}
