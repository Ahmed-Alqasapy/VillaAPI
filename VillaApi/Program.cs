using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RoyalVilla.DTO;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using VillaApi.Data;
using VillaApi.Filters;
using VillaApi.Middleware;
using VillaApi.Models;
using VillaApi.Models.DTOs.AuthDTOs;
using VillaApi.OpenApi;            // VersionedDocTransformer
using VillaApi.Reposatories;
using VillaApi.Services;

namespace VillaApi
{
    public class Program
    {
        // The API versions the app supports. Add "v3" here when you ship a v3 controller.
        private static readonly string[] ApiVersions = ["v1", "v2"];

        // The Angular origin allowed to call this API (move to appsettings later).
        private const string CorsPolicy = "AngularClient";

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── JWT signing key (guarded so a missing secret fails clearly at startup) ──
            var secret = builder.Configuration.GetSection("JwtSettings")["Secret_Key"]
                ?? throw new InvalidOperationException("JwtSettings:Secret_Key is missing.");
            var key = Encoding.UTF8.GetBytes(secret);

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            // ── Authentication (JWT) ──
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.SaveToken = true;
                    options.RequireHttpsMetadata = false;        // dev only
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // ── CORS (named policy for the Angular app) ──
            builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
                .WithOrigins("http://localhost:4200")   // your Angular dev URL
                .AllowAnyHeader()
                .AllowAnyMethod()));

            // ── Session (kept for learning; a pure JWT API usually wouldn't need it) ──
            builder.Services.AddSession(options =>
            {
                options.Cookie.Name = ".NetCookie";
                options.Cookie.IsEssential = true;
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
            });

            // ── Antiforgery (kept for learning; relevant to cookie auth, not header JWT) ──
            builder.Services.AddAntiforgery(o =>
            {
                o.Cookie.Name = ".MyApp.Antiforgery";    // distinct from the session cookie
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
                o.HeaderName = "X-XSRF-TOKEN";
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.SameSite = SameSiteMode.Strict;
            });

            // ── Database ──
            builder.Services.AddDbContext<ApplicationDbContext>(option =>
                option.UseSqlServer(builder.Configuration.GetConnectionString("myConnection")));

            // ── Controllers + validation envelope + JSON options ──
            builder.Services.AddControllers(f =>
            {
                f.Filters.AddService<ValidateAntiforgeryApiFilter>();   // validates POST/PUT/DELETE/PATCH only
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(kv => kv.Value!.Errors.Count > 0)
                        .ToDictionary(
                            kv => JsonNamingPolicy.CamelCase.ConvertName(kv.Key),
                            kv => kv.Value!.Errors
                                .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
                                .ToArray());

                    var response = ApiResponse<object>.BadRequest("Validation failed", errors);
                    return new BadRequestObjectResult(response);
                };
            })
            .AddJsonOptions(o =>
            {
                var j = o.JsonSerializerOptions;
                j.PropertyNameCaseInsensitive = true;
                j.Converters.Add(new JsonStringEnumConverter());
                j.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                j.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                j.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Arabic);
            });

            // ── API versioning ──
            builder.Services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
                o.ApiVersionReader = new UrlSegmentApiVersionReader();   // version from the URL path
            })
            .AddMvc()
            .AddApiExplorer(option =>
            {
                option.GroupNameFormat = "'v'VVV";         // 1.0 → "v1"
                option.SubstituteApiVersionInUrl = true;   // show v1/v2 in docs
            });

            // ── OpenAPI document per version (no BuildServiceProvider — uses the known list) ──
            foreach (var vName in ApiVersions)
            {
                builder.Services.AddOpenApi(vName, options =>
                    options.AddDocumentTransformer(new VersionedDocTransformer(vName)));
            }

            // ── AutoMapper ──
            builder.Services.AddAutoMapper(o =>
            {
                o.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
                o.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
                o.CreateMap<Villa, VillaDTo>().ReverseMap();
                o.CreateMap<VillaUpdateDTO, VillaDTo>().ReverseMap();
                o.CreateMap<User, UserDTO>().ReverseMap();
                o.CreateMap<VillaAmenityCreateDTo, VillaAmenities>()
                    .ForMember(d => d.CreatedDate, opt => opt.Ignore());
                o.CreateMap<VillaAmenities, VillaAmenityCreateDTo>();
                o.CreateMap<VillaAmenities, VillaAmenityUpdateDTo>();
                o.CreateMap<VillaAmenityUpdateDTo, VillaAmenities>()
                    .ForMember(d => d.UpdatedDate, opt => opt.Ignore());
                o.CreateMap<AmenityDTo, VillaAmenities>();
                o.CreateMap<VillaAmenities, AmenityDTo>()
                    .ForMember(dest => dest.VillaName,
                               opt => opt.MapFrom(src => src.Villa != null ? src.Villa.Name : null));
            });

            // ── Application services ──
            builder.Services.AddScoped<IdempotencyFilter>();
            builder.Services.AddScoped<ValidateAntiforgeryApiFilter>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IVillaService, VillaService>();
            builder.Services.AddScoped<VillaRepo>();
            builder.Services.AddScoped<IConfigurationManager, ConfigurationManager>();

            builder.Services.Configure<RouteOptions>(o => o.LowercaseUrls = true);

            // ─────────────────────────────── build ───────────────────────────────
            var app = builder.Build();

            app.UseExceptionHandler();
            await SeedDataAsync(app);

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi("/openapi/{documentName}.json");

                app.MapScalarApiReference(option =>
                {
                    option.Title = "Demo _ Royal Villa API";

                    // real container here (app is built) → safe to enumerate versions
                    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var version in provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion))
                    {
                        var versionName = version.GroupName;
                        var displayName = $"Demo_API_{versionName}";
                        var isDefault = version.ApiVersion.Equals(new ApiVersion(1, 0));
                        option.AddDocument(versionName, displayName, $"/openapi/{versionName}.json", isDefault);
                    }
                });
            }

            // ── Pipeline (order matters) ──
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(CorsPolicy);        // after routing, before auth
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
            await context.Database.MigrateAsync();
        }
    }
}