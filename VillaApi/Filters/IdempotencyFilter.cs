using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using VillaApi.Data;
using VillaApi.Models;

namespace VillaApi.Filters
{
    public class IdempotencyFilter : IAsyncActionFilter
    {
        private const string HeaderName = "Idempotency-Key";
        private readonly ApplicationDbContext _db;

        public IdempotencyFilter(ApplicationDbContext db) => _db = db;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var http = context.HttpContext;

            if (!http.Request.Headers.TryGetValue(HeaderName, out var keyValues) ||
                string.IsNullOrWhiteSpace(keyValues))
            {
                context.Result = new BadRequestObjectResult($"{HeaderName} header is required.");
                return;
            }

            var key = keyValues.ToString();
            var endpoint = $"{http.Request.Method} {http.Request.Path}";
            var requestHash = Hash(JsonSerializer.Serialize(context.ActionArguments));

            var existing = await _db.IdempotencyRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Key == key && r.Endpoint == endpoint);

            if (existing is not null)
            {
                if (existing.RequestHash != requestHash)
                {
                    context.Result = new UnprocessableEntityObjectResult(
                        "This Idempotency-Key was already used with a different request body.");
                    return;
                }

                if (existing.StatusCode is null)
                {
                    context.Result = new ConflictObjectResult("A request with this key is still being processed.");
                    return;
                }

                // Replay the first result
                http.Response.Headers["Idempotent-Replayed"] = "true";
                context.Result = new ContentResult
                {
                    StatusCode = existing.StatusCode,
                    Content = existing.ResponseBody,
                    ContentType = "application/json"
                };
                return;
            }

            // Reserve the key (unique index stops two parallel requests)
            var record = new IdempotencyRecord { Key = key, Endpoint = endpoint, RequestHash = requestHash };
            _db.IdempotencyRecords.Add(record);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                context.Result = new ConflictObjectResult("A request with this key is already being processed.");
                return;
            }

            var executed = await next();

            if (executed.Exception is null &&
                executed.Result is ObjectResult obj &&
                (obj.StatusCode ?? 200) is >= 200 and < 300)
            {
                record.StatusCode = obj.StatusCode ?? 200;
                record.ResponseBody = JsonSerializer.Serialize(obj.Value,
                    new JsonSerializerOptions(JsonSerializerDefaults.Web));
            }
            else
            {
                // Failed: release the key so the client can retry
                _db.IdempotencyRecords.Remove(record);
            }

            await _db.SaveChangesAsync();
        }

        private static string Hash(string input) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(input)));
    }
}
