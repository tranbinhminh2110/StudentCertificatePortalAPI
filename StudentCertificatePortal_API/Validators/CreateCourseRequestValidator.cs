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
/*                .Matches(@"^[1-9]\d* times?$").WithMessage("CourseTime must be in the format 'X time', where X is a number.");*/
        }
    }
}
