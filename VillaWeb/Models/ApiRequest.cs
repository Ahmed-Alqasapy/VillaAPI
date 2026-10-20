using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Contracts;
using static VillaWeb.Models.SD;

namespace VillaWeb.Models
{
    public class ApiRequest
    {
        public ApiType ApiType { get; set; } = ApiType.GET;

        public string? ApiUrl { get; set; }

        public object? Data { get; set; }

        public string? Token { get; set; }
    }
}
