using VillaApi.Models.DTOs;

namespace VillaApi.Services
{
    public interface IAuthService
    {
       Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

       Task<LoginResponseDTO> LoginAsync(LoginRequestDTO loginRequestDTO);

       Task<bool> IsEmailExsistsAsync(string email);


    }
}
