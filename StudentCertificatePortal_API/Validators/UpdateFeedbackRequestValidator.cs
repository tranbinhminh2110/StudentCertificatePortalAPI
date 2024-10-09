using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateFeedbackRequestValidator : AbstractValidator<UpdateFeedbackRequest>
    {
        public UpdateFeedbackRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.FeedbackDescription)
                .NotEmpty().WithMessage("FeedbackDescription is required.");
        }
    }
}
