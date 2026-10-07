using System.ComponentModel.DataAnnotations;
using System.Security.Policy;

namespace VillaApi.Models.DTOs
{
    public class VillaDTo
    {
        
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Details { get; set; }

        public double Rate { get; set; }

        public int Sqft { get; set; }

        public int Occupancy { get; set; }

        public string? ImageUrl { get; set; }
   

    }
}
