using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VillaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AntiforgeryController : ControllerBase
    {

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public IActionResult GetToken(IAntiforgery antiforgery)
        {
            var tokens = antiforgery.GetAndStoreTokens(HttpContext);
            return Ok (new {tokens.RequestToken});  
        }
    }
}
