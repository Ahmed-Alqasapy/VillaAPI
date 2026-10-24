using RoyalVilla.DTO;
using System.Net;
using VillaApi.Models.DTOs.AuthDTOs;
using VillaWeb.Services;
using VillaWeb.Services.IServices;

namespace VillaWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IVillaService, VillaService>();

            builder.Services.AddAutoMapper(o =>
            {
                o.CreateMap<VillaDTo, VillaCreateDTO>().ReverseMap();
                o.CreateMap<VillaDTo, VillaUpdateDTO>().ReverseMap();
               



            });


            //Here we made a httpclinet in the DI container , it knows the base address and here we teach him how to deal with the cookie 
            //we gave him a cookie jar to carry cookie through the mutable requests.
            builder.Services.AddHttpClient("RoyalApi", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("ServiceUrl:VillaApi"));
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
              {
                UseCookies = true,                      // turn the cookie jar ON
                CookieContainer = new CookieContainer() // the jar itself
              });



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }
    }
}
