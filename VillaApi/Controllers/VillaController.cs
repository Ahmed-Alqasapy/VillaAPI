using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VillaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        [HttpGet]
        [Route("/Villas")]
        public string Get_Villas()
        {
            return "This is a GET request for Villas";
        }
    }
}
