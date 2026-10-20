using Microsoft.AspNetCore.Hosting.Server;
using System.ComponentModel.DataAnnotations;

namespace RoyalVilla.DTO
{
    public class VillaAmenityCreateDTo
    {



        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        public string Description { get; set; } = string.Empty;

        [Required]
        public int VillaId { get; set; }
    }
}
