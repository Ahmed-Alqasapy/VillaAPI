using RoyalVilla.DTO;
using VillaWeb.Models;
using VillaWeb.Services.IServices;

namespace VillaWeb.Services
{
    public class VillaService:  BaseService,IVillaService 
    {
        private readonly IConfiguration _config;

        private readonly string villaApiUrl = "/api/Villa";



        public VillaService(IHttpClientFactory httpClient , IConfiguration configuration) : base(httpClient) 
        {
            _config = configuration;
            

        }



        public Task<T?> CreateAsync<T>(VillaCreateDTO villaCreateDTO, string token)
        {
            return SendRequestAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.POST,
                Data = villaCreateDTO,
                ApiUrl = $"{villaApiUrl}",
                Token = token
            });

        }

        public Task<T?> DeleteAsync<T>(int id, string token)
        {
            return SendRequestAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.DELETE,
                ApiUrl = $"{villaApiUrl}/{id}",
                Token = token
            });
        }

        public Task<T?> GetAllAsync<T>(string token)
        {
            return SendRequestAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                ApiUrl = $"{villaApiUrl}",
                Token = token
            });

        }

        public async Task<T?> GetAsync<T>(int id, string token)
        {
          
            return await SendRequestAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.GET,
                ApiUrl = $"{villaApiUrl}/{id}",
                Token = token
            });

        }

        public Task<T?> UpdateAsync<T>(VillaUpdateDTO villaUpdateDTO, string token)
        {
            return SendRequestAsync<T>(new ApiRequest
            {
                ApiType = SD.ApiType.PUT,
                Data = villaUpdateDTO,
                ApiUrl = $"{villaApiUrl}/{villaUpdateDTO.Id}",
                Token = token
            });

        }
    }
}
