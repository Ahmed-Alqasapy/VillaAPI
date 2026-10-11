using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VillaApi.Data;
using VillaApi.Models;
using VillaApi.Models.DTOs;
using VillaApi.Services;

namespace VillaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
       readonly IAuthService _auth;
        public AuthController(IAuthService auth)
        {
         _auth = auth; 
        }

        [HttpGet]
        [Route("/Register")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDTo>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            if(_auth.IsEmailExsistsAsync(registrationRequestDTO.Email).Result)
            {
                return Conflict(ApiResponse<object>.Conflict("Email already exists", "Please use a different email"));
            }

            if(registrationRequestDTO == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest("Invalid request", "Registration request cannot be null"));
            }

            var user = await _auth.RegisterAsync(registrationRequestDTO);

            if (user == null)
            {
                var errorResponse = ApiResponse<UserDTO>.Error(500,"User registration failed", "An error occurred while creating the user");
                return StatusCode(500, errorResponse);
            }

            var response = ApiResponse<UserDTO>.CreatedAt(user, "User Registred Sucessfully");

            return CreatedAtAction(nameof(Register),response);
        }
    }
}
