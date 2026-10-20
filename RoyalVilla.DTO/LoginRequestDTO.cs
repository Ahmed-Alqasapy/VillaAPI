using System.ComponentModel.DataAnnotations;

namespace VillaApi.Models.DTOs.AuthDTOs
{
    public class LoginRequestDTO
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
