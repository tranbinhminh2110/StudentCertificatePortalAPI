using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class UpdateExamEnrollmentRequestValidator : AbstractValidator<UpdateExamEnrollmentRequest>
    {
        public UpdateExamEnrollmentRequestValidator()
        {
        }
    }
}
