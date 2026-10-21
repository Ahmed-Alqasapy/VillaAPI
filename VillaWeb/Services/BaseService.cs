using System.Text.Json;
using VillaApi.Models;
using VillaWeb.Models;
using VillaWeb.Services.IServices;

namespace VillaWeb.Services
{
    public class BaseService : IBaseService
    {

        public ApiResponse<object> ResponseModel {  get; set; }
        public IHttpClientFactory _http {  get; set; }

        public static readonly JsonSerializerOptions jsonOptions = new()
        {

            PropertyNameCaseInsensitive = true,
        };
        public BaseService(IHttpClientFactory httpClient)
        {
            _http = httpClient;
            this.ResponseModel = new();

        }
        public async Task<T?> SendRequestAsync<T>(ApiRequest apiRequest)
        {
            try
            {
                var client = _http.CreateClient("RoyalApi");
                var message = new HttpRequestMessage
                {
                    RequestUri = new Uri(apiRequest.ApiUrl),
                    Method = GetHttpMethod(apiRequest.ApiType),

                };
                if(apiRequest.Data !=null)
                {

                     message.Content = JsonContent.Create(apiRequest.Data,options: jsonOptions);
                }

                

                var response = await client.SendAsync(message);

                var apiresponse =   await response.Content.ReadFromJsonAsync<T>(jsonOptions);

                return apiresponse;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected Error Happens {ex.Message}");
                return default;
            }
           
        }

        public static HttpMethod GetHttpMethod(SD.ApiType sd)
        {
            switch(sd)
            {
                  case SD.ApiType.GET:
                    return HttpMethod.Get;
                  case SD.ApiType.POST:
                    return HttpMethod.Post;
                  case SD.ApiType.PUT:
                    return HttpMethod.Put;
                  default:    
                    return HttpMethod.Get;


            }
        }
    }
}
