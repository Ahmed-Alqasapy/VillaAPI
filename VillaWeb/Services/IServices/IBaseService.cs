using Microsoft.AspNetCore.Mvc.ApiExplorer;
using VillaApi.Models;
using VillaWeb.Models;

namespace VillaWeb.Services.IServices
{
    public interface IBaseService
    {

        ApiResponse<object> ResponseModel { get; set; } 
     
         Task<T?> SendRequestAsync<T>(ApiRequest apiRequest);
    }
}
