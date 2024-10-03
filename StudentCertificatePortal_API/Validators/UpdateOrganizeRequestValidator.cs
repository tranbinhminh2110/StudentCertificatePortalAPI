using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateOrganizeRequestValidator: AbstractValidator<UpdateOrganizeRequest>
    {
        public UpdateOrganizeRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.OrganizeName)
                .NotEmpty().WithMessage("OrganizeName is required.");
            RuleFor(x => x.OrganizeContact)
                .NotEmpty().WithMessage("OrganizeContact is required.");
        }
    }
}
