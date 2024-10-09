using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateExamSessionRequestValidator : AbstractValidator<UpdateExamSessionRequest>
    {
        public UpdateExamSessionRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SessionName)
                .NotEmpty().WithMessage("SessionName is required.");
            RuleFor(x => x.SessionCode)
                .NotEmpty().WithMessage("SessionCode is required.");
            RuleFor(x => x.SessionDate)
                .NotEmpty().WithMessage("SessionDate is required.")
                .GreaterThan(DateTime.Now.Date.AddDays(-1)).WithMessage("SessionDate must be in the future.");
            RuleFor(x => x.SessionAddress)
                .NotEmpty().WithMessage("SessionAddress is required.");
        }
    }
}
