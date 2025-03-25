using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SeidorSmallWebApi.Routes;
using System.Text;

namespace SeidorSmallWebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static void ConfigureIISIntegration(this IServiceCollection services)
    {
        services.Configure<IISOptions>(options =>
        {
        });
    }

    public static void ConfigureAuthentication(this IServiceCollection services, string authKey)
    {
        var encodedAuthKey = Encoding.ASCII.GetBytes(authKey);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(x =>
        {
            x.RequireHttpsMetadata = false;
            x.SaveToken = true;
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(encodedAuthKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(ApiRoutes.Version, new OpenApiInfo { Title = "SeidorAPI", Version = ApiRoutes.Version });
            c.CustomSchemaIds(type => type.ToString());
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                        },
                        new List<string>()
                    }
                });
        });
    }

    public static void ConfigureMvcBuilder(this IServiceCollection services)
    {
        // Configure MvcBuilder: Suffix Async is trimmed from methods on CreatedAtAction, disable it
        services.AddMvc(options =>
        {
            options.SuppressAsyncSuffixInActionNames = false;

        }).AddJsonOptions(jsonOptions =>
        {
            // camelCase is json formatting by default.
            // Set to null, have PascalCase as defined at class level
            jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = null;
        });
    }
}
