using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateVoucherRequestValidator : AbstractValidator<CreateVoucherRequest>
    {
        public CreateVoucherRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.VoucherName)
                .NotEmpty().WithMessage("Voucher name is required.");
            RuleFor(x => x.Percentage)
                .InclusiveBetween(0, 100).WithMessage("Percentage must be between 0 and 100.");
            RuleFor(x => x.CreationDate)
               .NotEmpty().WithMessage("Creation date is required.")
               .GreaterThan(DateTime.Now.Date.AddMinutes(-5)).WithMessage("Creation date must be in the present or future.")
               .LessThanOrEqualTo(x => x.ExpiryDate).WithMessage("Creation date must be before or equal to expiry date.");
            RuleFor(x => x.ExpiryDate)
                .NotEmpty().WithMessage("Expiry date is required.")
                .GreaterThan(DateTime.Now.Date.AddMinutes(-5)).WithMessage("ExpiryDate must be in the future.")
                .GreaterThanOrEqualTo(x => x.CreationDate).WithMessage("Expiry date must be after or equal to the creation date.");
        }
    }
}
