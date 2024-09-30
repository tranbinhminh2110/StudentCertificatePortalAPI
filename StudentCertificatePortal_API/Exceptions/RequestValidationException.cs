using FluentValidation.Results;

namespace StudentCertificatePortal_API.Exceptions
{
    public class RequestValidationException : Exception
    {
        private const string ErrorMessage = "User input validation failed.";

        public List<ValidationFailure> Errors { get; init; }

        public RequestValidationException(List<ValidationFailure> errors) : base(ErrorMessage)
        {
            Errors = errors;
        }

        public RequestValidationException(string paramName, string message) : base(ErrorMessage)
        {
            Errors = new()
        {
            new ValidationFailure(paramName, message),
        };
        }
    }
}
