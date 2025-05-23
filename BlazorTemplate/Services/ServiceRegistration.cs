using BlazorTemplate.Interfaces;
using BlazorTemplate.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorTemplate
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IOrderService, OrderService>();
            services.AddSingleton<ISelectedOrdersService, SelectedOrdersService>();
            services.AddScoped<IBatchService, BatchService>();

            return services;
        }
    }
}