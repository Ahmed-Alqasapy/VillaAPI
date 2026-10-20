using System.ComponentModel.DataAnnotations;

namespace VillaApi.Models.DTOs.AuthDTOs
{
    public class RegistrationRequestDTO
    {
        //HERE THE PART OF TRANSFORMATION.
        //CONTROLLER / DTO LEVEL VALIDATION.

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
