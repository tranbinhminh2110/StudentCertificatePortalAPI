using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateCourseEnrollmentRequestValidator : AbstractValidator<CreateCourseEnrollmentRequest>
    {
        public CreateCourseEnrollmentRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.Courses)
                .NotNull().WithMessage("Course cannot be null.")
                .NotEmpty().WithMessage("At least one Course must be provided.");
        }
    }
}
