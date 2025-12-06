using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;


namespace Shared.Configuration;


public static class ApiVersioningConfiguration
{
    
    public static IServiceCollection AddApiVersioningConfiguration(
        this IServiceCollection services
    )
    {
        services.AddApiVersioning(options =>
        {
            // Default version if not specified
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Report API Versions in Response Headers
            options.ReportApiVersions = true;

            // Support multiple versioning strategies
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new HeaderApiVersionReader("X-API-Version"),
                new QueryStringApiVersionReader("api-version")
            );

        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

}