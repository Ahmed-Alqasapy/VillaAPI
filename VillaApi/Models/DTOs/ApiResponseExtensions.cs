using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;

namespace VillaApi.Models.DTOs
{
    public static class ApiResponseExtensions
    {
     

        public static ObjectResult ToActionResult<T>(this ApiResponse<T> reponse)=>
            new(reponse) { StatusCode= reponse.StatusCode };
    }
}
