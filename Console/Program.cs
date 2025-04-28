using System.Reflection;
using B1SLayer;
using Console.Processes;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OperadorLogistico.Console
{
    public interface ISapDatabaseConnection : IDatabaseConnection { }
    public interface ILogisticOperatorDatabaseConnection : IDatabaseConnection { }

    public class SapDatabaseConnection : SQLDatabaseConnection, ISapDatabaseConnection
    {
        public SapDatabaseConnection(string server, string company, string user, string password)
            : base(server, company, user, password) { }
    }

    public class LogisticOperatorDatabaseConnection : SQLDatabaseConnection, ILogisticOperatorDatabaseConnection
    {
        public LogisticOperatorDatabaseConnection(string server, string company, string user, string password)
            : base(server, company, user, password) { }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            using var mutex = new Mutex(true, Assembly.GetEntryAssembly().GetName().Name);

            if (!mutex.WaitOne(TimeSpan.Zero, true)) return;

            try
            {
                Host.CreateDefaultBuilder(args)
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

                        services.AddSingleton<ISapDatabaseConnection>(sp =>
                        {
                            return new SapDatabaseConnection(
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("ServiceUrl"),
                                context.Configuration.GetSection("SapSettings").GetValue<string>("CompanyDb"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Username"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Password")
                            );
                        });

                        services.AddSingleton<ILogisticOperatorDatabaseConnection>(sp =>
                        {
                            return new LogisticOperatorDatabaseConnection(
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("ServiceUrl"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("CompanyDb"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Username"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Password")
                            );
                        });

                        //services.AddSingleton<OrderManagement>();
                        services.AddSingleton<InventorySync>();
                        services.AddHostedService<ProcessWorker>();
                    })
                    .Build().Run();
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
}