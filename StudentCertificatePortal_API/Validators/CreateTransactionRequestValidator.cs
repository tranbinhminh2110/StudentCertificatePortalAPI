using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
    {
        public CreateTransactionRequestValidator()
        {
            RuleFor(request => request.WalletId)
                .GreaterThan(0).WithMessage("Wallet ID must be greater than 0.");

            RuleFor(request => request.Point)
                .GreaterThan(0).WithMessage("Point must be greater than 0.");
        }
    }
}
