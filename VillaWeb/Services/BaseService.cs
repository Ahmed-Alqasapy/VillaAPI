using Microsoft.AspNetCore.Components.Forms;
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
        private readonly string AntiForgeryToeknURL = "/api/Antiforgery";


        private string? _antiforgeryToken;

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

                bool isMutating = apiRequest.ApiType is SD.ApiType.POST
                                     or SD.ApiType.PUT
                                     or SD.ApiType.DELETE;

                // one key for this logical request; reused if we retry below
                var idempotencyKey = Guid.NewGuid().ToString();


                var client = _http.CreateClient("RoyalApi");
                var message = new HttpRequestMessage
                {
                    RequestUri = new Uri(apiRequest.ApiUrl,uriKind:UriKind.Relative),
                    Method = GetHttpMethod(apiRequest.ApiType),

                };
                if(apiRequest.Data !=null)
                {

                     message.Content = JsonContent.Create(apiRequest.Data,options: jsonOptions);
                }

                if (isMutating)
                {
                    var token = _antiforgeryToken ?? await GetAntiforgeryTokenAsync(client);
                    if (token != null)
                        message.Headers.Add("X-XSRF-TOKEN", token);
                    message.Headers.Add("Idempotency-Key", idempotencyKey);
                }

                var response = await client.SendAsync(message);
                // If the antiforgery check failed, our ticket/cookie pair is stale.
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest
                    && apiRequest.ApiType != SD.ApiType.GET)
                {
                    _antiforgeryToken = null;                   // forget the old ticket
                    await GetAntiforgeryTokenAsync(client);           // get a fresh pair

                    // rebuild the request (a sent HttpRequestMessage can't be reused)
                    // re-add header + content, then SendAsync once more
                }

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

        private async Task<string?> GetAntiforgeryTokenAsync(HttpClient client)
        {
            // This GET also makes the API send the .MyApp.Session cookie,
            // which the jar (Step A) stores automatically.
            var response = await client.GetAsync("/api/antiforgery");
            if (!response.IsSuccessStatusCode) return null;

            // body looks like: { "requestToken": "CfDJ8..." }
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            _antiforgeryToken = doc.RootElement.GetProperty("requestToken").GetString();
            return _antiforgeryToken;
        }
    }
}
