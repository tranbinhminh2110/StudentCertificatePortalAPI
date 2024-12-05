using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
    {
        public CreateFeedbackRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.FeedbackDescription)
                .NotEmpty().WithMessage("FeedbackDescription is required.");
        }
    }
}
