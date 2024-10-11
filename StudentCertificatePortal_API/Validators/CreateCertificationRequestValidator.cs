using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateCertificationRequestValidator : AbstractValidator<CreateCertificationRequest>
    {
        public CreateCertificationRequestValidator()
        { 
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(cert => cert.CertName)
                .NotEmpty().WithMessage("Certification name is required")
                .Length(1, 100).WithMessage("Certification name must be between 1 and 100 characters");

            RuleFor(cert => cert.CertCode)
                .NotEmpty().WithMessage("Certification code is required")
                .Length(1, 10).WithMessage("Certification code must be between 1 and 10 characters");

            RuleFor(cert => cert.CertDescription)
                .MaximumLength(500).WithMessage("Certification description cannot exceed 500 characters");

            RuleFor(cert => cert.CertCost)
                .GreaterThanOrEqualTo(0).WithMessage("Certification cost must be a positive number");

            RuleFor(cert => cert.CertPointSystem)
                .MaximumLength(50).WithMessage("Certification point system cannot exceed 50 characters");

            RuleFor(cert => cert.CertImage)
                .Must(uri => Uri.IsWellFormedUriString(uri, UriKind.RelativeOrAbsolute))
                .When(cert => !string.IsNullOrEmpty(cert.CertImage))
                .WithMessage("Certification image must be a valid URL");

           /* RuleFor(cert => cert.CertPrerequisite)
                .MaximumLength(200).WithMessage("Certification prerequisite cannot exceed 200 characters");*/

            /*RuleFor(cert => cert.ExpiryDate)
                .NotEmpty().WithMessage("Expiry date is required")
                .GreaterThan(DateTime.Now).WithMessage("Expiry date must be in the future");*/
        }
    }
}
