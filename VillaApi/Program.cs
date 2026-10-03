
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Threading.Tasks;
using VillaApi.Data;
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

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddAutoMapper(o =>
            {
                o.CreateMap<Villa,VillaCreateDTO>().ReverseMap();
                o.CreateMap<Villa,VillaUpdateDTO>().ReverseMap();
            });

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
