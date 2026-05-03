using Asp.Versioning.ApiExplorer;
using HospitalApp.Infrastructure.Shared.Settings;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HospitalApp.WebAPI.Extensions.Swagger;

public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider, IOptions<BusinessInfo> businessInfo) : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider = provider;
    private readonly BusinessInfo _businessInfo = businessInfo.Value;
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="options"></param>
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateInfoForApiVersion(description)
            );
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="description"></param>
    /// <returns></returns>
    public OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Version = description.ApiVersion.ToString(),
            Title = $"{_businessInfo.Name} API",
            Description = $"{_businessInfo.Description}",
            Contact = new OpenApiContact
            {
                Email = _businessInfo.Email,
                Name = _businessInfo.Name
            },
        };

        if (description.IsDeprecated)
        {
            info.Description += " This API version has been deprecated.";
        }
        return info;
    }
}