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
        [Range(1,int.MaxValue , ErrorMessage ="Page must be 1 or greater")]
        public int Page { get; set; }


        [Range(1, 100, ErrorMessage = "PageSize should be between 1 and 100")]
        public int PageSize { get; set; } = 20;
    }
}
