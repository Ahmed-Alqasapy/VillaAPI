namespace VillaApi.Models.DTOs
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;

        public UserDTO? UserDto { get; set; }    
    }
}
