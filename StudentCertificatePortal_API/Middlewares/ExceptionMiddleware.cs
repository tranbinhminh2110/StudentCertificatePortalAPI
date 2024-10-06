using Microsoft.Extensions.Primitives;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Exceptions;
using System.Text.Json;

namespace StudentCertificatePortal_API.Middlewares
{
    public class ExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next.Invoke(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private readonly IDictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

        public ExceptionMiddleware()
        {
            _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
            {
                // Built-in exceptions
                { typeof(KeyNotFoundException), HandleKeyNotFoundExceptionAsync },
                { typeof(InvalidDataException), HandleInvalidDataExceptionAsync },

                // Custom exceptions
                { typeof(ConflictException), HandleConflictExceptionAsync },
                { typeof(RequestValidationException), HandleRequestValidationExceptionAsync },
                { typeof(TokenValidationException), HandleTokenValidationExceptionAsync },
                { typeof(UserAuthenticationException), HandleUserAuthenticationExceptionAsync },
                { typeof(UnsupportedMediaTypeException), HandleUnsupportedMediaTypeExceptionAsync }
            };
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var type = ex.GetType();
            if (_exceptionHandlers.ContainsKey(type))
            {
                await _exceptionHandlers[type].Invoke(context, ex);
                return;
            }

            // If no handler is defined, return 500 Internal Server Error
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteExceptionMessageAsync(context, ex);
        }

        // Handler for RequestValidationException
        private static async Task HandleRequestValidationExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

            var rve = (RequestValidationException)ex;
            var data = new Dictionary<string, StringValues>();
            foreach (var vf in rve.Errors)
            {
                var propName = vf.PropertyName.ToLower();
                if (!data.ContainsKey(propName))
                {
                    data.Add(propName, vf.ErrorMessage);
                }
                else
                {
                    data[propName] = StringValues.Concat(data[propName], vf.ErrorMessage);
                }
            }

            var result = Result<Dictionary<string, StringValues>>.Fail(ex) with
            {
                Data = data
            };

            await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(result));
        }

        // Handlers for other exception types
        private static async Task HandleKeyNotFoundExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async Task HandleInvalidDataExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async Task HandleConflictExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async Task HandleTokenValidationExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async Task HandleUserAuthenticationExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await WriteExceptionMessageAsync(context, ex);
        }

        private static async Task HandleUnsupportedMediaTypeExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
            await WriteExceptionMessageAsync(context, ex);
        }

        // Helper method to write the exception message as JSON
        private static async Task WriteExceptionMessageAsync(HttpContext context, Exception ex)
        {
            await context.Response.Body.WriteAsync(SerializeToUtf8BytesWeb(Result<string>.Fail(ex)));
        }

        // Serialize to UTF-8 JSON
        private static byte[] SerializeToUtf8BytesWeb<T>(T value)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }
    }
}
