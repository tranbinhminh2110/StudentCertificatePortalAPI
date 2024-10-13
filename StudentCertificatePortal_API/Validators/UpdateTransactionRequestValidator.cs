using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
    {
        public UpdateTransactionRequestValidator()
        {
            RuleFor(request => request.WalletId)
                .GreaterThan(0).WithMessage("Wallet ID must be greater than 0.");

            RuleFor(request => request.Point)
                .GreaterThan(0).WithMessage("Point must be greater than 0.");

            RuleFor(request => request.TransDesription)
                .MaximumLength(255).WithMessage("Transaction description cannot exceed 255 characters.");

            RuleFor(request => request.TransStatus)
                .NotEmpty().WithMessage("Transaction status is required.")
                .Must(BeValidStatus).WithMessage("Transaction status must be either 'Pending', 'Success', 'Failed', or 'Cancelled'.");
        }
        private bool BeValidStatus(string status)
        {
            return status == "Pending" || status == "Success" || status == "Failed" || status == "Cancelled";
        }
    }
    
}
