using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VillaApi.Models;

namespace RoyalVilla.DTO
{
    public class ApiPagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = [];

        //Those For Pagination
        public int Page {  get; set; }

        public int PageSize { get; set; }


        public int TotalItems {  get; set; }

        public bool IsFirst => Page <= 1;

        public bool IsLast => Page>=TotalPages;

        public bool HasNext  => Page < TotalPages;

        public bool HasPrevious => Page > 1;

        public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);




    }
}
