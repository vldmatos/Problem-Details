
using Microsoft.AspNetCore.Http.Features;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization();
            builder.Services.AddOpenApi();


            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Instance =
                     $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                    context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                    var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                    context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
                };
            });

            builder.Services.AddExceptionHandler<ProblemExceptionHandler>();

            var application = builder.Build();
            application.MapOpenApi();
            application.UseHttpsRedirection();
            application.UseAuthorization();
            application.UseExceptionHandler();


            //sample endpoint
            application.MapGet("/unlock", (string code) =>
            {
                if (code == "1000")
                    return Results.Ok("Unlocked");

                throw new ProblemException("not access", "The code is invalid");

            })
            .WithName("Unlock");

            application.Run();
        }
    }
}
