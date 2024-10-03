using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateMajorRequestValidator: AbstractValidator<UpdateMajorRequest>
    {
        public UpdateMajorRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.MajorCode)
                .NotEmpty().WithMessage("MajorCode is required.");
            RuleFor(x => x.MajorName)
                .NotEmpty().WithMessage("MajorName is required.");
        }
    }
}
