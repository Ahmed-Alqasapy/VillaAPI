using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaApi.Data;
using VillaApi.Filters;
using VillaApi.Models;
using VillaApi.Helper;
using RoyalVilla.DTO;
using VillaApi.Models.DTOs.AuthDTOs;
using VillaApi.Services;
using Asp.Versioning;

namespace VillaApi.Controllers.v1
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        IMapper _map;
        IVillaService _villaService;
        public VillaController(IMapper map , IVillaService villaService)
        {
                _map = map;
            _villaService = villaService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTo>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task <ActionResult<ApiResponse<ApiPagedResult<VillaDTo>>>> Get_Villas([FromQuery]VillaQueryParams q)
        {
            

            var responseVillaDto =await _villaService.GetAllVillas(q);

                return ApiResponse<ApiPagedResult<VillaDTo>>
              .Ok(responseVillaDto, responseVillaDto.Items.Count() == 0 ? "No villas found." : "Villas retrieved successfully.",null)
              .ToActionResult();

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

                var villa = await _villaService.GetVillaById(id);

                if (villa == null)
                {

                    return ApiResponse<VillaDTo>.NotFound($"Villa With id = {id} Not Found").ToActionResult();
                }

                else
                {

                    var responseVillaDto = _map.Map<VillaDTo>(villa);
                    var response = ApiResponse<VillaDTo>.Ok(responseVillaDto,"Villa Retrieved Sucessfully",null).ToActionResult();

                    return response;
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


                var foundVillaDublicate = await _villaService.GetVillaByName(VillaDto.Name);    

                if (foundVillaDublicate)
                {

                    return ApiResponse<VillaDTo>.Conflict("There is Conflict", $"A villa with '{VillaDto.Name}' already exists").ToActionResult();
                }

                var villa =await _villaService.SaveVilla(VillaDto);

                var responseVillaDto = _map.Map<VillaDTo>(villa);
                var resposne = ApiResponse<VillaDTo>.Created(responseVillaDto, "Villa created sucessfully ").ToActionResult();

                return CreatedAtAction(nameof(GetVillaById),new { villa.Id} ,resposne);

                
                

            }
            catch(DbUpdateException ex) when (DbExceptionHelper.IsUniqueViolation(ex))
            {
                return ApiResponse<VillaDTo>.Conflict("This villa was just created by someone else.",null);

            }
            catch (Exception ex)
            {

                return ApiResponse<object>.Error(500, "An error occured while created  a villa", ex.Message).ToActionResult();
                
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
            

            var dublicateVilla = await _villaService.GetVillaByNameAndId(villaDto.Name , id);

            if(dublicateVilla)
            {
                return ApiResponse<VillaDTo>.Conflict("There is Conflict", $"A villa with '{villaDto.Name}' already exists").ToActionResult();

            }
            var dbVilla = await _villaService.GetVillaById(id);

            if(dbVilla == null) return ApiResponse<object>.NotFound("villa not found").ToActionResult();


            _map.Map(villaDto, dbVilla);


            await _villaService.UpdateVilla(dbVilla);

            var villaResponseDto = _map.Map<VillaDTo>(dbVilla);

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

            var dbVilla = await _villaService.GetVillaById(id);

            if (dbVilla == null) return ApiResponse<object>.NotFound("Villa not found").ToActionResult();

           //Delete Villa
           await _villaService.DeleteVilla(dbVilla);

            return ApiResponse<object>.Ok(null!, "Villa Deleted Sucessfully",null).ToActionResult();

        }



    }

}
