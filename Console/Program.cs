using B1SLayer;
using OperadorLogistico.Console.Processes;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console;
using OperadorLogistico.Console.Communication.Api;
using System.Reflection;

using var mutex = new Mutex(true, Assembly.GetEntryAssembly().GetName().Name);

if (!mutex.WaitOne(TimeSpan.Zero, true)) return;

Host.CreateDefaultBuilder()
    .ConfigureAppConfiguration((builder) =>
    {
        builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging(configure =>
        {
            configure.AddConfiguration(context.Configuration.GetSection("Logging"));
            configure.AddConsole();
            configure.AddDebug();
        });
        services.AddSingleton<SLConnection>(sp =>
        {
            return new SLConnection(
                context.Configuration.GetSection("SapSettings").GetValue<string>("ServiceUrl"),
                context.Configuration.GetSection("SapSettings").GetValue<string>("CompanyDb"),
                context.Configuration.GetSection("SapSettings").GetValue<string>("Username"),
                context.Configuration.GetSection("SapSettings").GetValue<string>("Password")
            );
        });
        services.AddSingleton<IDatabaseConnection>(sp =>
        {
            return new SQLDatabaseConnection(
                context.Configuration.GetSection("SQLConnection").GetValue<string>("Server"),
                context.Configuration.GetSection("SQLConnection").GetValue<string>("Company"),
                context.Configuration.GetSection("SQLConnection").GetValue<string>("User"),
                context.Configuration.GetSection("SQLConnection").GetValue<string>("Password")
            );
        });
        services.AddSingleton(sp =>
        {
            var config = new ApiConfig
            {
                BaseUrl = context.Configuration.GetSection("ApiSettings").GetValue<string>("BaseUrl"),
                ApiKey = context.Configuration.GetSection("ApiSettings").GetValue<string>("ApiKey"),
                TimeoutSeconds = context.Configuration.GetSection("ApiSettings").GetValue<int>("TimeoutSeconds")
            };
            return new ApiClient(config);
        });

        services.AddSingleton<OrderManagement>();
        services.AddHostedService<ProcessWorker>();
    })
    .Build().Run();

mutex.ReleaseMutex();