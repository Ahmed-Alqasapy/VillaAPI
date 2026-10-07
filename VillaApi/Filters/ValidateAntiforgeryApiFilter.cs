using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VillaApi.Models;
namespace VillaApi.Filters
{
  

    public sealed class ValidateAntiforgeryApiFilter(
        IAntiforgery antiforgery,
        ILogger<ValidateAntiforgeryApiFilter> logger) : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var method = context.HttpContext.Request.Method;

            // Safe methods don't change data → no check
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) ||
                HttpMethods.IsOptions(method) || HttpMethods.IsTrace(method))
                return ;

            // Respect [IgnoreAntiforgeryToken] on the controller/action
            if (context.ActionDescriptor.EndpointMetadata.OfType<IgnoreAntiforgeryTokenAttribute>().Any())
                return;

            try
            {
                await antiforgery.ValidateRequestAsync(context.HttpContext);   // the same checks as before
            }
            catch (AntiforgeryValidationException ex)
            {
                logger.LogWarning(ex, "Antiforgery validation failed for {Path}", context.HttpContext.Request.Path);
                context.Result = new BadRequestObjectResult(new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Invalid or missing anti-forgery token."
                });
            }
        }
    }
}
