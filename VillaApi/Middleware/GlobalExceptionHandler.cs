using Microsoft.AspNetCore.Diagnostics;
using VillaApi.Models;

namespace VillaApi.Middleware
{
    public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
        {
            logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", context.TraceIdentifier);

            var response = ApiResponse<object>.Error(StatusCodes.Status500InternalServerError,
                "An unexpected error occurred. Please contact support with this reference."
                ,null);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(response, ct);
            return true;  
        }
        
    
    }
}
