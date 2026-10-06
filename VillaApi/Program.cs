
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using VillaApi.Data;
using VillaApi.Filters;
using VillaApi.Models;
using VillaApi.Models.DTOs;

namespace VillaApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //DI
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<ApplicationDbContext>(option =>
            {
            option.UseSqlServer(builder.Configuration.GetConnectionString("myConnection"));

                #region .Net 9 Way To Seed Data into Database instead of HasData() way
                //NEw Way To Seed Data inot Database sync and async 
                //option.UseSeeding((ApplicationDbContext context, _) ={
                //        if (!context.Set<Villa>().Any())
                //        {
                //            context.Set<Villa>().AddRange(
                //                new Villa() {},
                //                new Villa() { Name}
                //                ); 
                //        }
                //}); 
                #endregion

            });

            //Configuration For Serlizations.
            builder.Services.AddControllers().AddJsonOptions(o =>
            {
                var j = o.JsonSerializerOptions;
                j.Converters.Add(new JsonStringEnumConverter());                               // Case File #6
                j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;               // smaller responses (optional)
                j.ReferenceHandler = ReferenceHandler.IgnoreCycles;                           // safety net for #5
                j.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic);   // #8
            });


            builder.Services.AddScoped<IdempotencyFilter>();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddAutoMapper(o =>
            {
                o.CreateMap<Villa,VillaCreateDTO>().ReverseMap();
                o.CreateMap<Villa,VillaUpdateDTO>().ReverseMap();
                o.CreateMap<Villa, VillaDTo>().ReverseMap();
                o.CreateMap<VillaUpdateDTO, VillaDTo>().ReverseMap();
            });

            //ROUTING VERSIONING
            builder.Services.AddApiVersioning(o=>
            {
                o.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;

            }).AddMvc();

            //FOR MAKING URL LOWER CASE ROUTING
            builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

            var app = builder.Build();

            await SeedDataAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }

       public static async Task SeedDataAsync(WebApplication app)
       {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

       }
    }
}
