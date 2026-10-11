using System.ComponentModel.DataAnnotations;

namespace VillaApi.Models.DTOs
{
    public class RegistrationRequestDTO
    {
      

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        [Required]
        public required string Password { get; set; }
        [Required]
        [MaxLength(50)]
        public required string Role { get; set; } = "Customer";

     
    }
}
