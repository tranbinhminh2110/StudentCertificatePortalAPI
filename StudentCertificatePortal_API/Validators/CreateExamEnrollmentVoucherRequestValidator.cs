using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateExamEnrollmentVoucherRequestValidator : AbstractValidator<CreateExamEnrollmentVoucherRequest>
    {
        public CreateExamEnrollmentVoucherRequestValidator() 
        {
            RuleLevelCascadeMode = CascadeMode.Stop;

            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                .GreaterThan(0).WithMessage("UserId must be greater than 0.");

            RuleFor(x => x.Simulation_Exams)
                .NotNull().WithMessage("Simulation_Exams cannot be null.")
                .NotEmpty().WithMessage("At least one Simulation Exam must be provided.");
        }
    }
}
