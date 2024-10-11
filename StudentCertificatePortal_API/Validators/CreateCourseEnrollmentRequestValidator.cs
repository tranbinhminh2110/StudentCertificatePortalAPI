using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateCourseEnrollmentRequestValidator : AbstractValidator<CreateCourseEnrollmentRequest>
    {
        public CreateCourseEnrollmentRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.CourseEnrollmentDate)
            .NotEmpty().WithMessage("CourseEnrollmentDate is required.");
            RuleFor(x => x.CourseEnrollmentStatus)
                .NotEmpty().WithMessage("CourseEnrollmentStatus is required.");
            RuleFor(x => x.TotalPrice)
                .NotEmpty().WithMessage("TotalPrice is required.")
                .GreaterThanOrEqualTo(0).WithMessage("TotalPrice must be greater than 0.");
        }
    }
}
