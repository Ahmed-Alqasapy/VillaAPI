using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VillaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {

        string _storagePath = "C:\\D\\.Net\\API_Project\\MyVilla\\Villa\\VillaApi\\Storage\\";

        [HttpPost("api/cases/{caseId:int}/documents")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> Upload(int caseId, [FromForm] string description, IFormFile file, CancellationToken ct)
        {
            var allowed = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return BadRequest("File type not allowed");
            if (file.Length == 0) return BadRequest("Empty file");

            var safeName = $"{Guid.NewGuid()}{ext}";
            await using var stream = System.IO.File.Create(Path.Combine(_storagePath, safeName));
            await file.CopyToAsync(stream, ct);
            return Ok();
        }
    }
}
