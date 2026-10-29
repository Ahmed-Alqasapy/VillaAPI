using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoyalVilla.DTO
{
    public class VillaQueryParams
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page must be 1 or greater")]
        public int Page { get; set; } = 1;


        [Range(1, 100, ErrorMessage = "PageSize should be between 1 and 100")]
        public int PageSize { get; set; } = 20;
        [Range(1,int.MaxValue , ErrorMessage ="Occupancy shpuld be greater than 1")]
        public int? Occupancy { get; set; }


        // ── Sorting ──
        [RegularExpression("^(rate|name|created|occupancy)$",
            ErrorMessage = "SortBy must be one of: rate, name, created, occupancy.")]
        public string? SortBy { get; set; }                      // optional → nullable

        [RegularExpression("^(asc|desc)$", ErrorMessage = "SortDir must be 'asc' or 'desc'.")]
        public string? SortDir { get; set; }                     // optional → nullable



        [Range(0, double.MaxValue)] public double? MinRate { get; set; }
        [Range(0, double.MaxValue)] public double? MaxRate { get; set; }
        public string? Search { get; set; }
    }
}
