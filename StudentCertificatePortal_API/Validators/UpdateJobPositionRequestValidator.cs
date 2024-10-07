using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateJobPositionRequestValidator : AbstractValidator<UpdateJobPositionRequest>
    {
        public UpdateJobPositionRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.JobPositionCode)
                .NotEmpty().WithMessage("JobPositionCode is required.");
            RuleFor(x => x.JobPositionName)
                .NotEmpty().WithMessage("JobPositionName is required.");
        }
    }
}
