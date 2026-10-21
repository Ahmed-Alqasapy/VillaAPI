
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoyalVilla.DTO;
using Scalar.AspNetCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.Tasks;
using VillaApi.Data;
using VillaApi.Filters;
using VillaApi.Middleware;
using VillaApi.Migrations;
using VillaApi.Models;
using VillaApi.Models.DTOs.AuthDTOs;
using VillaApi.Reposatories;
using VillaApi.Services;

namespace VillaApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //DI
            var builder = WebApplication.CreateBuilder(args);

            var key = Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JwtSettings")["Secret_Key"]);


            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();



            //Configue the Authentication Middleware
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters() { 
                
                IssuerSigningKey= new SymmetricSecurityKey(key),
                ValidateAudience=false,
                ValidateIssuerSigningKey=true,
                ValidateIssuer=false,
                ValidateLifetime=true,
                ClockSkew=TimeSpan.Zero


                
                
                
                };

            }




            );    
                ;



            //Add Sesstion To make the HTTP Requests StateFull or to make the Server Stateful
            //Rule for cookie naming , the cookie name must not have any spaces.        
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".NetCookie";
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(1);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //for HTTPS
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            //apply Anti Forgery Token
            //Header name is what is sent as a key with each request so the server find if this request came from another forgery page "Not from the our domain"
            builder.Services.AddAntiforgery(o =>
            {
                o.Cookie.Name = ".MyApp.Session";
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
                o.HeaderName = "X-XSRF-TOKEN";
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.SameSite = SameSiteMode.Strict;
            });


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
            //Here we apply filters with each controller we have except the GET Endpoints.
            builder.Services.AddControllers(f =>
            {
                f.Filters.AddService<ValidateAntiforgeryApiFilter>(); // validate only POST,PUT,DELETE,PATCH
            }).ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Where(kv => kv.Value!.Errors.Count > 0)
                    .ToDictionary(kv=>JsonNamingPolicy.CamelCase.ConvertName(kv.Key),
                    
                    kv=>kv.Value!.Errors.Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                            .ToArray());

                    var response = ApiResponse<object>.BadRequest("Validation failed", errors);

                    return new BadRequestObjectResult(response);
                };
            })
             .AddJsonOptions(o =>
            {
                
                var j = o.JsonSerializerOptions;
                j.PropertyNameCaseInsensitive = true;
                j.Converters.Add(new JsonStringEnumConverter());                               // Case File #6
                j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;               // smaller responses (optional)
                j.ReferenceHandler = ReferenceHandler.IgnoreCycles;                           // safety net for #5
                j.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic);   // #8
            });


            //Add Authentication add bearer JWT in Scalar
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Components ??= new OpenApiComponents();
                    document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",                
                        BearerFormat = "JWT",
                        Description = "Enter your JWT token (without the word 'Bearer')"
                    };

                    document.SecurityRequirements.Add(new OpenApiSecurityRequirement
                    {
                        [new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        }] = Array.Empty<string>()
                    });

                    return Task.CompletedTask;   
                });
            });

            //Make here automapping for the DTOs we made with the DB Entites.
            builder.Services.AddAutoMapper(o =>
            {
                o.CreateMap<Villa,VillaCreateDTO>().ReverseMap();
                o.CreateMap<Villa,VillaUpdateDTO>().ReverseMap();
                o.CreateMap<Villa, VillaDTo>().ReverseMap();
                o.CreateMap<VillaUpdateDTO,VillaDTo>().ReverseMap();
                o.CreateMap<User,UserDTO>().ReverseMap();
                o.CreateMap<VillaAmenityCreateDTo, VillaAmenities>()
                       .ForMember(d => d.CreatedDate, o => o.Ignore());
                o.CreateMap<VillaAmenities, VillaAmenityCreateDTo>();

                o.CreateMap<VillaAmenities, VillaAmenityUpdateDTo>();
                
                o.CreateMap<VillaAmenityUpdateDTo, VillaAmenities>()
                        .ForMember(d => d.UpdatedDate, o => o.Ignore());
                o.CreateMap<AmenityDTo, VillaAmenities>();

                //There is a property in the Amentity Dto that will cause a problem whihle map from the DB class to the DTO one 
                o.CreateMap<VillaAmenities, AmenityDTo>()
                .ForMember(dest => dest.VillaName, opt => opt.MapFrom(src=>src.Villa! != null? src.Villa.Name : null));

                

            });


            //DI
            builder.Services.AddScoped<IdempotencyFilter>();
            builder.Services.AddScoped<ValidateAntiforgeryApiFilter>();
            builder.Services.AddScoped<IAuthService,AuthService>();
            builder.Services.AddScoped<IVillaService, VillaService>();
            builder.Services.AddScoped<VillaRepo>();
            builder.Services.AddScoped<IConfigurationManager,ConfigurationManager>();


            //ROUTING VERSIONING
            //Here if we made versioning we will catch this later.
            builder.Services.AddApiVersioning(o=>
            {
                o.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;

            }).AddMvc();

            //FOR MAKING URL LOWER CASE ROUTING
            //Configure the Routeoptions to be lower cases.
            builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

            var app = builder.Build();
            app.UseExceptionHandler();
            //Seed data dynimacally if there are pending migration and it is applied to "update-database"
            await SeedDataAsync(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                //Configure the scalar api instead of swagger.
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();

            app.MapControllers();

            app.Run();
        }

       public static async Task SeedDataAsync(WebApplication app)
       {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            //This is what i meant by if there are any pending migration that are made 
            await context.Database.MigrateAsync();

       }
    }
}
