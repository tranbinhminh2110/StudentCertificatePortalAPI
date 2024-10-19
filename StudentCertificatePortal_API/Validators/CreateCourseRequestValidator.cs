using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateCourseRequestValidator: AbstractValidator<CreateCourseRequest>
    {
        public CreateCourseRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.CourseName)
                .NotEmpty().WithMessage("CourseName is required.");
            RuleFor(x => x.CourseCode)
                .NotEmpty().WithMessage("CourseCode is required.");
            RuleFor(x => x.CourseTime)
                .NotEmpty().WithMessage("CourseTime is required.");
            RuleFor(x => x.CertId)
                .NotNull().WithMessage("Certification ID is required.")
                .GreaterThan(0).WithMessage("Certification ID must be a positive integer.");
            RuleFor(x => x.CourseDescription)
                .MaximumLength(500).WithMessage("Course Description must not exceed 500 characters.");
            RuleFor(x => x.CourseFee)
                .GreaterThanOrEqualTo(0).WithMessage("CourseFee must be a positive number");
            RuleFor(x => x.CourseImage)
                .Must(BeAValidImageUrl).WithMessage("Course Image must be a valid URL.");
        }

        private bool BeAValidImageUrl(string? url)
        {
            return string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}
