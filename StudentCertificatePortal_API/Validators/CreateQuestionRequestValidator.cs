using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateQuestionRequestValidator : AbstractValidator<CreateQuestionRequest>
    {
        public CreateQuestionRequestValidator()
        {
            RuleFor(request => request.QuestionName)
                .NotNull()
                .WithMessage("Question name is required.")
                .NotEmpty()
                .WithMessage("Question name cannot be empty.");

            RuleFor(request => request.ExamId)
                .GreaterThan(0)
                .WithMessage("Exam ID must be greater than zero.");
            RuleFor(request => request.Answers)
                .NotNull()
                .WithMessage("At least one answer is required.")
                .Must(answers => answers.Count > 0)
                .WithMessage("At least one answer must be provided.");

            RuleForEach(request => request.Answers).ChildRules(answers =>
            {
                answers.RuleFor(answer => answer.Text)
                    .NotNull()
                    .WithMessage("Answer text is required.")
                    .NotEmpty()
                    .WithMessage("Answer text cannot be empty.");

                answers.RuleFor(answer => answer.IsCorrect)
                    .NotNull()
                    .WithMessage("IsCorrect must be specified for each answer.");
            });
        }
    }
}
