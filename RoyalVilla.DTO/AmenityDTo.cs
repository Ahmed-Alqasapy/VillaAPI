using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoyalVilla.DTO
{
    public class AmenityDTo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public  string? Name { get; set; }


        public string? Description { get; set; }


        public int VillaId { get; set; }

        public string? VillaName { get; set; }
    }
}
