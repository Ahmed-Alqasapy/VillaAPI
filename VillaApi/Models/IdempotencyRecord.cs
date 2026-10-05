namespace VillaApi.Models
{
    public class IdempotencyRecord
    {
        public int Id { get; set; }
        public string Key { get; set; } = default!;
        public string Endpoint { get; set; } = default!;     // e.g. "POST /api/VillaAPI"
        public string RequestHash { get; set; } = default!;
        public int? StatusCode { get; set; }                  // null = in progress
        public string? ResponseBody { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
