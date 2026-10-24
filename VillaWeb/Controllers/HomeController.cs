using AutoMapper;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using System.Diagnostics;
using VillaApi.Models;
using VillaWeb.Models;
using VillaWeb.Services.IServices;

namespace VillaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _map;
        private readonly IVillaService _villaService;

        public HomeController(ILogger<HomeController> logger , IVillaService villaService , IMapper map)
        {
            _logger = logger;
            _map = map;
            _villaService = villaService;
        }

        public async Task<IActionResult> Index()
        {

            List<VillaDTo> villaList = new List<VillaDTo>();

            try
            {
                var response = await _villaService.GetAllAsync<ApiResponse<List<VillaDTo>>>("");

                if (response != null && response.Success && response.Data != null)
                {
                    villaList = response.Data;
                }

            }
            catch(Exception ex)
            {
                TempData["Error"] = $"An Error Occured: {ex.Message}";

            }
           


            return View(villaList);
        }

        public async Task<IActionResult> CreateVilla()
        {
            var villa = new VillaCreateDTO() {

                Name = $"Test Villa NEW ONE hhh", // unique name avoids the duplicate check
                Details = "Created from HomeController.TestCreate",
                Rate = 100,
                Sqft = 500,
                Occupancy = 4,
                ImageUrl = "https://example.com/villa.jpg"

            };

            //here we will call the create async

            var resposne =await _villaService.CreateAsync<ApiResponse<VillaDTo>>(villa,"");

            return Json(resposne);


        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
