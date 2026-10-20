using VillaApi.Models.DTOs.AuthDTOs;

namespace VillaApi.Services
{
    public interface IAuthService
    {
       Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

       Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);

        //VALIDATION ON BUINESS LAYER LEVEL 
       Task<bool> IsEmailExsistsAsync(string email);


    }
}
