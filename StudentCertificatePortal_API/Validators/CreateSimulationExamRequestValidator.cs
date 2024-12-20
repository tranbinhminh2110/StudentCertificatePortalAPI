﻿using FluentValidation;
using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Validators
{
    public class CreateSimulationExamRequestValidator : AbstractValidator<CreateSimulationExamRequest>
    {
        public CreateSimulationExamRequestValidator()
        {
            RuleFor(x => x.ExamName)
                .NotEmpty().WithMessage("Exam Name is required.")
                .MaximumLength(255).WithMessage("Exam Name must not exceed 255 characters.");

            RuleFor(x => x.ExamCode)
                .NotEmpty().WithMessage("Exam Code is required.")
                .MaximumLength(100).WithMessage("Exam Code must not exceed 100 characters.");

            RuleFor(x => x.CertId)
                .NotNull().WithMessage("Certification ID is required.")
                .GreaterThan(0).WithMessage("Certification ID must be a positive integer.");

            RuleFor(x => x.ExamDescription)
                .MaximumLength(500).WithMessage("Exam Description must not exceed 500 characters.");

            RuleFor(x => x.ExamFee)
                .NotNull().WithMessage("Exam Fee is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Exam Fee must be a non-negative integer.");

       

            RuleFor(x => x.ExamImage)
                .Must(BeAValidImageUrl).WithMessage("Exam Image must be a valid URL.");
        }

        private bool BeAValidImageUrl(string? url)
        {
            return string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }
    }
}
