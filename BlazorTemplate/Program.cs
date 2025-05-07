using BlazorTemplate.Interfaces;
using BlazorTemplate.Services;
using BlazorTemplateService;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FluentUI.AspNetCore.Components;
namespace BlazorTemplate;

public interface ILogisticOperatorDatabaseConnection : IDatabaseConnection { }

public class LogisticOperatorDatabaseConnection : SQLDatabaseConnection, ILogisticOperatorDatabaseConnection
{
    public LogisticOperatorDatabaseConnection(string server, string company, string user, string password)
        : base(server, company, user, password) { }
}

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        var host = CreateHost();
        var services = host.Services;
        var mainForm = services.GetRequiredService<BlazorForm>();
        AppDomain.CurrentDomain.UnhandledException += (sender, error) =>
        {
            System.Windows.Forms.MessageBox.Show(text: error.ExceptionObject.ToString(), caption: "Error");
        };
        Application.Run(mainForm);
    }
    static IHost CreateHost()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((context, builder) =>
                builder.SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true))
            .ConfigureServices((context, services) =>
            {
                services.AddWindowsFormsBlazorWebView();
                services.AddFluentUIComponents();
                services.AddPluginRepositories(context.Configuration);
                services.AddSingleton<BlazorForm>();
                string orderFilesPath = @"C:\Users\mmelis\OneDrive - SEIDOR SOLUTIONS S.L\Escritorio";
                services.AddSingleton<IOrderService>(provider =>
                    new OrderService(
                        provider.GetRequiredService<ILogger<OrderService>>(),
                        orderFilesPath
                    )
                );
                // Registrar el servicio de órdenes seleccionadas
                services.AddSingleton<ISelectedOrdersService, SelectedOrdersService>();

                // Registrar el servicio de batches
                services.AddSingleton<IBatchService, BatchService>();

                services.AddSingleton<ILogisticOperatorDatabaseConnection>(sp =>
                {
                    return new LogisticOperatorDatabaseConnection(
                        context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("ServiceUrl"),
                        context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("CompanyDb"),
                        context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Username"),
                        context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Password")
                    );
                });
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.AddFile(context.Configuration.GetSection("Serilog"));
            })
            .Build();
    }
}