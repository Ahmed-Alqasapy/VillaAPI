using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaApi.Data;
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
        public async Task <ActionResult< IEnumerable<Villa>>> Get_Villas()
        {
            //return "This is a GET request for Villas";
            var villas = await _db.Villas.ToListAsync();
            if(villas.Count == 0)
            {
                return  NotFound();
            }

            return Ok(villas);
        }


        [HttpGet("{id:int}")]
        public async Task <ActionResult<Villa>> GetVillaById(int id)
        {
            try
            {
                //return $"this is the villa id you want:  {id}";
                if (id <= 0) { return BadRequest(); }

                var villa = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);
                if (villa == null)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(villa);
                }

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occured while retreving villa with Id:{id}");
            }
            
        }


        [HttpPost()]
        public async Task<ActionResult<Villa>> CraeteVilla(VillaCreateDTO  VillaDto)
        {
            try
            {
                if (VillaDto == null) { return BadRequest(); }

                Villa villa = _map.Map<Villa>(VillaDto);

                await _db.Villas.AddAsync(villa);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetVillaById), new {  villa.Id },villa);
                
                

            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occured while Creating villa");
            }

        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> UpdateVilla(int id, VillaUpdateDTO villaDto)
        {
            if(id<=0) { return BadRequest(); }

            if(id!=villaDto.Id) { return BadRequest(); }

            if (villaDto == null) return BadRequest();

            var dbVilla = await _db.Villas.FirstOrDefaultAsync(v=>v.Id==id);

            if(dbVilla == null) return NotFound();

            _map.Map(villaDto, dbVilla);

            dbVilla.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();

            return NoContent();

        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> DeleteVilla(int id)
        {
            if (id <= 0) { return BadRequest(); }



            var dbVilla = await _db.Villas.FirstOrDefaultAsync(v => v.Id == id);

            if (dbVilla == null) return NotFound();

             _db.Villas.Remove(dbVilla);

            await _db.SaveChangesAsync();

            return NoContent();

        }





    }

}
