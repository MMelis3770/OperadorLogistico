using DatabaseConnection;
using BlazorTemplateService.Handlers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEIDOR_SLayer;

namespace BlazorTemplateService;

public static class ServiceCollectionExtension
{
    public static void AddPluginRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IDatabaseConnection>(sp => new SQLDatabaseConnection(
                    configuration.GetSection("SqlConnection").GetValue<string>("Server"),
                    configuration.GetSection("SqlConnection").GetValue<string>("Database"),
                    configuration.GetSection("SqlConnection").GetValue<string>("User"),
                    configuration.GetSection("SqlConnection").GetValue<string>("Password")));

        services.AddSingleton(sp => new SLConnection(
            configuration.GetSection("ServiceLayer").GetValue<string>("serviceLayerRoot"),
            configuration.GetSection("ServiceLayer").GetValue<string>("companyDB"),
            configuration.GetSection("ServiceLayer").GetValue<string>("userName"),
            configuration.GetSection("ServiceLayer").GetValue<string>("password")));

        services.AddSingleton<SLHandler>();
        services.AddSingleton<DatabaseHandler>();  
    }
}
