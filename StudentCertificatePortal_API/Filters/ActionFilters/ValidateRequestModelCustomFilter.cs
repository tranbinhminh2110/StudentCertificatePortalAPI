using StudentCertificatePortal_API.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Reflection;

namespace StudentCertificatePortal_API.Filters.ActionFilters
{


    public class ValidateRequestAttribute : TypeFilterAttribute {
        public ValidateRequestAttribute(Type requestType)
        : base(typeof(ValidateRequestFilter<,>).MakeGenericType(requestType, ValidateRequestHelper.GetValidatorType(requestType)))
        {
        }
    }

    public class ValidateRequestFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var errors = ValidateRequestHelper.GetModelStateErrors(context);
            if (errors.Any())
                throw new RequestValidationException(errors);

            await next.Invoke();
        }
    }

    public class ValidateRequestFilter<TRequest, TValidator> : IAsyncActionFilter
    where TRequest : class
    where TValidator : AbstractValidator<TRequest>, new()
    {
        private readonly TValidator _validator;

        public ValidateRequestFilter(TValidator validator)
        {
            _validator = validator;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.ActionArguments.SingleOrDefault(e => e.Value is TRequest).Value is TRequest request)
            {
                var errors = ValidateRequestHelper.GetModelStateErrors(context);

                var validationResult = await _validator.ValidateAsync(request);
                errors.AddRange(validationResult.Errors);

                if (errors.Any())
                    throw new RequestValidationException(errors);
            }

            await next.Invoke();
        }
    }

    internal static class ValidateRequestHelper
    {
        public static Type GetValidatorType(Type requestType)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var validatorType = assembly.GetTypes().FirstOrDefault(t =>
                !t.IsGenericType
                && t.BaseType != null
                && t.BaseType.IsGenericType
                && t.BaseType.GetGenericTypeDefinition() == typeof(AbstractValidator<>)
                && t.BaseType.GenericTypeArguments.Contains(requestType))
                ?? throw new NotSupportedException("No validator for the given request model was found.");
            return validatorType;
        }

        public static List<ValidationFailure> GetModelStateErrors(ActionExecutingContext context) =>
            context.ModelState.SelectMany(
                e => e.Value?.Errors ?? Enumerable.Empty<ModelError>(),
                (entry, err) => new ValidationFailure(entry.Key, err.ErrorMessage)).ToList();
    }

}
