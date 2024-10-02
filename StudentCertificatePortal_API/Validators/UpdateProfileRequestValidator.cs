using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
    {
        public UpdateProfileRequestValidator()
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required.")
                .MaximumLength(64).WithMessage("Username length must not exceed 64 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Address)
                .MaximumLength(256).WithMessage("Address length must not exceed 256 characters.");

            RuleFor(x => x.Fullname)
                .MaximumLength(256).WithMessage("Full name length must not exceed 256 characters.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^[0-9]{0,10}$").WithMessage("Invalid phone number format.");
        }
    }
}
