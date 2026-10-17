using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VillaApi.Data;
using VillaApi.Filters;
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

        [HttpPost]
        [Route("/Register")]
        [ServiceFilter(typeof(IdempotencyFilter))]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register(RegistrationRequestDTO registrationRequestDTO)
        {
            if( await _auth.IsEmailExsistsAsync(registrationRequestDTO.Email))
            {
                return ApiResponse<object>.Conflict("Email already exists", "Please use a different email").ToActionResult();
             }

            if(registrationRequestDTO == null)
            {
                return ApiResponse<object>.BadRequest("Invalid request", "Registration request cannot be null").ToActionResult();
            }

            var user = await _auth.RegisterAsync(registrationRequestDTO);

            if (user == null)
            {
                var errorResponse = ApiResponse<UserDTO>.Error(500,"User registration failed", "An error occurred while creating the user");
                return StatusCode(500, errorResponse);
            }

            var response = ApiResponse<UserDTO>.Created(user, "User Registred Sucessfully").ToActionResult();

            return CreatedAtAction(nameof(Register),response);
        }





        [HttpPost]
        [Route("/Login")]
        [ServiceFilter(typeof(IdempotencyFilter))]
        [ProducesResponseType(typeof(ApiResponse<LoginResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>),StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<ApiResponse<LoginResponseDTO>>> Login(LoginRequestDTO LoginRequestDTO)
        {
            //if (await _auth.IsEmailExsistsAsync(LoginRequestDTO.Email))
            //{
            //    return Conflict(ApiResponse<object>.Conflict("Email already exists", "Please use a different email"));
            //}

            if (LoginRequestDTO == null)
            {
                return ApiResponse<object>.BadRequest("Invalid request", "Registration request cannot be null").ToActionResult();
            }

            var user = await _auth.LoginAsync(LoginRequestDTO);

            if (user == null)
            {
                var errorResponse = ApiResponse<UserDTO>.Error(500, "User Login failed", "An error occurred while Login").ToActionResult();
                return StatusCode(500, errorResponse);
            }

            var response = ApiResponse<LoginResponseDTO>.Ok(user, "User Login Sucessfully", null).ToActionResult();

            return Ok(response);
        }

    }
}
