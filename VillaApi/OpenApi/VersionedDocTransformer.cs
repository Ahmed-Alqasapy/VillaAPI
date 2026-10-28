using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace VillaApi.OpenApi;

/// <summary>
/// Fills each version's OpenAPI document with its title/version/contact
/// and adds the JWT "Bearer" security scheme (the Authorize box in Scalar).
/// One instance per version is passed in from Program.cs.
/// </summary>
public sealed class VersionedDocTransformer(string versionName) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Demo Api Royal",
            Version = versionName,
            Description = $"Demo_API_{versionName}",
            Contact = new OpenApiContact { Email = "Ahmed@hmail.com", Name = "AlQasapy" }
        };

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
    }
}