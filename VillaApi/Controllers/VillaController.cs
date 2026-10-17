using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaApi.Data;
using VillaApi.Filters;
using VillaApi.Models;
using VillaApi.Models.DTOs;

namespace VillaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        ApplicationDbContext _db;
        IMapper _map;

        public VillaController(ApplicationDbContext db , IMapper map)
        {
                _db = db;
                _map = map;
        }

        [HttpGet]
        [Route("/Villas")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTo>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task <ActionResult<ApiResponse<IEnumerable<VillaDTo>>>> Get_Villas()
        {
      
            var villas = await _db.Villas.ToListAsync();
            if(villas.Count == 0)
                return NotFound(ApiResponse<object>.NotFound("Villas not found"));


            var responseVillaDto = _map.Map<List<VillaDTo>>(villas);
            var response = ApiResponse<IEnumerable<VillaDTo>>.Ok(responseVillaDto, "Villas retrieved successfully",null);
        
            return Ok(response);
        }


        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDTo>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task <ActionResult<ApiResponse<VillaDTo>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0) {
  
                    return ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero").ToActionResult();
                }

                var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (villa == null)
                {


                    //return NotFound(ApiResponse<object>.NotFound("villa not found"));
                    return ApiResponse<VillaDTo>.NotFound($"Villa With id = {id} Not Found").ToActionResult();
                }
                else
                {

                    var responseVillaDto = _map.Map<VillaDTo>(villa);
                    var response = ApiResponse<VillaDTo>.Ok(responseVillaDto,"Villa Retrieved Sucessfully",null);

                    return Ok(response);
                }

            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while retreving villa with Id:{id}", ex.Message).ToActionResult();
                return StatusCode(StatusCodes.Status500InternalServerError, errorResponse);
            }
            
        }


        [HttpPost()]
        [ServiceFilter(typeof(IdempotencyFilter))]
        [ProducesResponseType(typeof(ApiResponse<VillaDTo>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VillaDTo>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse< VillaDTo>>> CraeteVilla(VillaCreateDTO  VillaDto)
        {
            try
            {
                if (VillaDto == null)
                {
                    return ApiResponse<object>.BadRequest("Bad Request", "There is somthing with the data ").ToActionResult();
                }

                var dublicateVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Name == VillaDto.Name);

                if (dublicateVilla != null)
                {

                    return ApiResponse<VillaDTo>.Conflict("There is Conflict", $"A villa with '{VillaDto.Name}' already exists").ToActionResult();
                }

                Villa villa = _map.Map<Villa>(VillaDto);

                await _db.Villas.AddAsync(villa);
                await _db.SaveChangesAsync();

                var responseVillaDto = _map.Map<VillaDTo>(villa);
                var resposne = ApiResponse<VillaDTo>.Created(responseVillaDto, "Villa created sucessfully ").ToActionResult();

                return CreatedAtAction(nameof(GetVillaById),new { villa.Id} ,resposne);

                
                

            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<object>.Error(500, "An error occured while created  a villa", ex.Message).ToActionResult();
                return StatusCode(500,errorResponse);
            }

        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(IdempotencyFilter))]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VillaDTo>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<VillaDTo>), StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse<VillaDTo>>> UpdateVilla(int id, VillaUpdateDTO villaDto)
        {
            if(id<=0)
            {
                return ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero ").ToActionResult();
            }

            if (id!=villaDto.Id)
            {
                return ApiResponse<object>.BadRequest("Bad Request", "id dosen`t match ").ToActionResult();
            }

            if (villaDto == null) return ApiResponse<object>.BadRequest("Bad Request", "there is no data in the body to be updated").ToActionResult();
            

            var dublicateVilla = await _db.Villas.FirstOrDefaultAsync(v=>v.Name == villaDto.Name &&  v.Id != id);

            if(dublicateVilla != null)
            {
                return ApiResponse<VillaDTo>.Conflict("There is Conflict", $"A villa with '{villaDto.Name}' already exists").ToActionResult();



            }

            var dbVilla = await _db.Villas.FirstOrDefaultAsync(v=>v.Id==id);

            if(dbVilla == null) return ApiResponse<object>.NotFound("villa not found").ToActionResult();


            _map.Map(villaDto, dbVilla);

            dbVilla.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();

            var villaResponseDto = _map.Map<VillaDTo>(villaDto);

            var response = ApiResponse<VillaDTo>.Ok(villaResponseDto, "Villa Updated Successfuly", null).ToActionResult();
            return Ok(response);

        }


        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<VillaDTo>), StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteVilla(int id)
        {
            if (id <= 0)
            {
                return ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero ").ToActionResult();
            }



            var dbVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if (dbVilla == null) return ApiResponse<object>.NotFound("Villa not found").ToActionResult();


            _db.Villas.Remove(dbVilla);

            await _db.SaveChangesAsync();

            var response = ApiResponse<object>.NoContent();


            return Ok(response);

        }





    }

}
