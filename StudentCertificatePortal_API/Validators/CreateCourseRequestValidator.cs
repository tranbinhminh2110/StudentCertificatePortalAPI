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
            RuleFor(x => x.CourseFee)
                .GreaterThanOrEqualTo(0).WithMessage("CourseFee must be a positive number");


        }
    }
}
