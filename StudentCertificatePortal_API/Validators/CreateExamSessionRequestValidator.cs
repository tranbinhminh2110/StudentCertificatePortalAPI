using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateExamSessionRequestValidator : AbstractValidator<CreateExamSessionRequest>
    {
        public CreateExamSessionRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SessionName)
                .NotEmpty().WithMessage("SessionName is required.");
            RuleFor(x => x.SessionCode)
                .NotEmpty().WithMessage("SessionCode is required.");
            RuleFor(x => x.SessionDate)
                .NotEmpty().WithMessage("SessionDate is required.")
                .GreaterThan(DateTime.Now.Date.AddMinutes(-5)).WithMessage("SessionDate must be in the future.");
            RuleFor(x => x.SessionAddress)
                .NotEmpty().WithMessage("SessionAddress is required.");
        }
    }
}
