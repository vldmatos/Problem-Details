using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace API
{
    public class ProblemException(string error, string message) : Exception(message)
    {
        public string Error { get; } = error;

        public override string Message { get; } = message;
    }

    public class ProblemExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is not ProblemException problemException)
            {
                return true;
            }

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = problemException.Error,
                Detail = problemException.Message,
                Type = "Bad Request",

            };

            httpContext.Response.StatusCode = problemDetails.Status.Value;
            return await _problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails
                });
        }
    }
}
