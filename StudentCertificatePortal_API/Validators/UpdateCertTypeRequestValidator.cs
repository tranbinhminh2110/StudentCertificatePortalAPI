using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateCertTypeRequestValidator : AbstractValidator<UpdateCertTypeRequest>
    {
        public UpdateCertTypeRequestValidator()
        {
            RuleFor(x => x.TypeCode)
                .NotEmpty().WithMessage("TypeCode is required.")
                .Length(3, 10).WithMessage("TypeCode must be between 3 and 10 characters.");

            RuleFor(x => x.TypeName)
                .NotEmpty().WithMessage("TypeName is required.")
                .MaximumLength(100).WithMessage("TypeName must not exceed 100 characters.");
        }
    }
}
