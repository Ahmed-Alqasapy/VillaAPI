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
        public async Task <ActionResult<ApiResponse<IEnumerable<VillaDTo>>>> Get_Villas()
        {
      
            //return "This is a GET request for Villas";
            var villas = await _db.Villas.ToListAsync();
            if(villas.Count == 0)
            {
                return  NotFound();
            }

            var responseVillaDto = _map.Map<List<VillaDTo>>(villas);
            var response = ApiResponse<IEnumerable<VillaDTo>>.Ok(responseVillaDto, "Villas retrieved successfully");
            string mysession = "My name is ahmed";

            HttpContext.Session.SetString("MySession", mysession);

            return Ok(response);
        }


        [HttpGet("{id:int}")]
        public async Task <ActionResult<ApiResponse<VillaDTo>>> GetVillaById(int id)
        {
            try
            {
                if (id <= 0) {
  
                    return BadRequest(  ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero"));
                }

                var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

                if (villa == null)
                {


                    return NotFound(ApiResponse<object>.NotFound("Villas not found"));
                }
                else
                {

                    var responseVillaDto = _map.Map<VillaDTo>(villa);
                    var response = ApiResponse<VillaDTo>.Ok(responseVillaDto,"Villa Retrieved Sucessfully");

                    return Ok(response);
                }

            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<object>.Error(500, $"An error occured while retreving villa with Id:{id}", ex.Message);
                return StatusCode(500, errorResponse);
            }
            
        }


        [HttpPost()]
        [ServiceFilter(typeof(IdempotencyFilter))]
        public async Task<ActionResult<ApiResponse< VillaDTo>>> CraeteVilla(VillaCreateDTO  VillaDto)
        {
            try
            {
                if (VillaDto == null)
                {
                    return BadRequest( ApiResponse<object>.BadRequest("Bad Request", "There is somthing with the data "));
                }

                var dublicateVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Name == VillaDto.Name);

                if (dublicateVilla != null)
                {
                    //return Conflict($"A villa with '{VillaDto.Name}' already exists");

                    return Conflict(ApiResponse<VillaDTo>.Conflict("There is Conflict", $"A villa with '{VillaDto.Name}' already exists"));
                }

                Villa villa = _map.Map<Villa>(VillaDto);

                await _db.Villas.AddAsync(villa);
                await _db.SaveChangesAsync();

                var responseVillaDto = _map.Map<VillaDTo>(villa);
                var resposne = ApiResponse<VillaDTo>.CreatedAt(responseVillaDto, "Villa created sucessfully ");

                return CreatedAtAction( nameof(GetVillaById),new { villa.Id} ,resposne);

                //return CreatedAtAction(nameof(GetVillaById), new { villa.Id }, _map.Map<VillaDTo>(villa));
                
                

            }
            catch (Exception ex)
            {

                var errorResponse = ApiResponse<object>.Error(500, "An error occured while created  a villa", ex.Message);
                return StatusCode(500,errorResponse);
            }

        }

        [HttpPut("{id:int}")]
        [ServiceFilter(typeof(IdempotencyFilter))]
        public async Task<ActionResult<ApiResponse<VillaDTo>>> UpdateVilla(int id, VillaUpdateDTO villaDto)
        {
            if(id<=0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero "));
            }

            if (id!=villaDto.Id)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Bad Request", "id dosen`t match "));
            }

            if (villaDto == null) return BadRequest(ApiResponse<object>.BadRequest("Bad Request", "there is no data in the body to be updated"));
            

            var dublicateVilla = await _db.Villas.FirstOrDefaultAsync(v=>v.Name == villaDto.Name &&  v.Id != id);

            if(dublicateVilla != null)
            {
                return Conflict(ApiResponse<VillaDTo>.Conflict("There is Conflict", $"A villa with '{villaDto.Name}' already exists"));



            }

            var dbVilla = await _db.Villas.FirstOrDefaultAsync(v=>v.Id==id);

            if(dbVilla == null) return NotFound(ApiResponse<object>.NotFound("villa not found"));


            _map.Map(villaDto, dbVilla);

            dbVilla.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();

            var villaResponseDto = _map.Map<VillaDTo>(villaDto);

            var response = ApiResponse<VillaDTo>.Ok(villaResponseDto,"Villa Updated Successfuly");
            return Ok(response);

        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteVilla(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Bad Request", "Id must be greater than zero "));
            }



            var dbVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if (dbVilla == null) return NotFound(ApiResponse<object>.NotFound("Villa not found"));


            _db.Villas.Remove(dbVilla);

            await _db.SaveChangesAsync();

            var response = ApiResponse<object>.NoContent();


            return Ok(response);

        }





    }

}
