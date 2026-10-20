using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoyalVilla.DTO;
using VillaApi.Data;
using VillaApi.Models;
using VillaApi.Models.DTOs.AuthDTOs;

namespace VillaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaAmenityController : ControllerBase
    {
        ApplicationDbContext _db;
        IMapper _map;


        public VillaAmenityController(ApplicationDbContext db, IMapper map)
        {
            _db = db;
            _map = map;

        }
        [HttpGet()]
        [Route("/GetVillaAmenitites")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AmenityDTo>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<AmenityDTo>>>> GetVillaAmenities(int villaId)
        {

            try
            {
                var amenities = await _db.VillaAmenities.Where(v => v.Villa!.Id == villaId).ToListAsync();

                var amenitiesDto = _map.Map<List<AmenityDTo>>(amenities);

                return ApiResponse<IEnumerable<AmenityDTo>>
                 .Ok(amenitiesDto, amenitiesDto.Count == 0 ? "here is no Amentites For This Villa Right Now" : $"The Amentites Of The Villa With id: {villaId}", null)
                 .ToActionResult();


            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, "There is Somthing happend While Retreiving the Amentities", ex.Message).ToActionResult();

            }

        }


        [ProducesResponseType(typeof(ApiResponse<AmenityDTo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<AmenityDTo>>>> GetVillaAmenitiyById(int id)
        {

            try
            {

                var amenity = await _db.VillaAmenities.Where(a => a.Id == id).FirstOrDefaultAsync();
                if (amenity == null)
                {
                    return ApiResponse<AmenityDTo>.NotFound("Amenitty Not Found").ToActionResult();
                }


                var amenityDto = _map.Map<AmenityDTo>(amenity);

                var response = ApiResponse<AmenityDTo>.Ok(amenityDto, "Amentity Found Sucessfully", null).ToActionResult();

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, "There is Somthing happend While Retreiving the Amentities", ex.Message).ToActionResult();

            }

        }

        [HttpPost]
        [Route("/AddVillaAmenitites")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<AmenityDTo>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<ApiResponse<AmenityDTo>>> AddVillaAmeniy(VillaAmenityCreateDTo villaRequestAmentity)
        {

            try
            {
                if (villaRequestAmentity == null)
                {
                    return ApiResponse<object>.BadRequest("Please Enter Valid Data", "There is somthing with the inserted Data").ToActionResult();
                }

                var villa = _db.Villas.Where(e => e.Id == villaRequestAmentity.VillaId).FirstOrDefault();

                if (villa == null)
                {
                    return ApiResponse<object>.NotFound("Villa Not Found").ToActionResult();

                }


                //For Villa i will use its id so i can get to the Created At Action.
                // and this entity will be saved in the Database.
                var amenitiyForVilla = _map.Map<VillaAmenities>(villaRequestAmentity);

                amenitiyForVilla.CreatedDate = DateTime.Now;
                await _db.VillaAmenities.AddAsync(amenitiyForVilla);

                await _db.SaveChangesAsync();

                var amentityDto = _map.Map<AmenityDTo>(amenitiyForVilla);

                var response = ApiResponse<AmenityDTo>
                 .Created(amentityDto, "Amenity Added Sucessfylly")
                 .ToActionResult();


                return CreatedAtAction(nameof(GetVillaAmenitiyById), new { amenitiyForVilla.Id }, response);
            }

            catch (Exception ex)
            {

                return ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, "There is Somthing happend", ex.Message).ToActionResult();
            }


        }


        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AmenityDTo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<AmenityDTo>>> UpdateVillaAmenity(int id, VillaAmenityUpdateDTo villaAmenityDto)
        {
            try
            {
                if (id <= 0)
                {
                    return ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero ").ToActionResult();
                }

                if (id != villaAmenityDto.Id)
                {
                    return ApiResponse<object>.BadRequest("Bad Request", "id dosen`t match ").ToActionResult();
                }

                if (villaAmenityDto == null) return ApiResponse<object>.BadRequest("Bad Request", "there is no data in the body to be updated").ToActionResult();


                var dbAmenity = await _db.VillaAmenities.FirstOrDefaultAsync(v => v.Id == id);

                if (dbAmenity == null) return ApiResponse<object>.NotFound("villa not found").ToActionResult();

                var villaExist = await _db.Villas.AnyAsync(v => v.Id == villaAmenityDto.VillaId);

                if (!villaExist)
                {
                    return ApiResponse<object>.BadRequest("Bad Request", $"Villa {villaAmenityDto.VillaId} does not exist.").ToActionResult();

                }

                _map.Map(villaAmenityDto, dbAmenity);

                dbAmenity.UpdatedDate = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                var villaResponseDto = _map.Map<AmenityDTo>(dbAmenity);

                var response = ApiResponse<AmenityDTo>.Ok(villaResponseDto, "Villa Updated Successfuly", null).ToActionResult();
                return Ok(response);

            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, "There is Somthing happend", ex.Message).ToActionResult();

            }

        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AmenityDTo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<object>>> DeleteVillaAmenity(int id)
        {
            try
            {

                if(id <= 0)
                {
                    return ApiResponse<object>.BadRequest("Id must be greater than Zero",null).ToActionResult();
                }

                var dbVillaAmenity = _db.VillaAmenities.FirstOrDefault(x => x.Id == id);

                if(dbVillaAmenity == null)
                {
                    return ApiResponse<object>.NotFound("Amenity To be Deleted Not Found").ToActionResult();

                }

                 _db.Remove(dbVillaAmenity);
                await _db.SaveChangesAsync();

                return ApiResponse<object>.NoContent("Operation Done Sucessfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Error(StatusCodes.Status500InternalServerError, "There is somthing happened",ex.Message).ToActionResult();
            }

        }
    }
}
