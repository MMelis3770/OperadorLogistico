using System.Reflection;
using B1SLayer;
using Console.Processes;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console.Communication.Api;

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
                                context.Configuration.GetSection("SapSettings").GetValue<string>("Server"),
                                context.Configuration.GetSection("SapSettings").GetValue<string>("Company"),
                                context.Configuration.GetSection("SapSettings").GetValue<string>("User"),
                                context.Configuration.GetSection("SapSettings").GetValue<string>("Password")
                            );
                        });

                        services.AddSingleton<ILogisticOperatorDatabaseConnection>(sp =>
                        {
                            return new LogisticOperatorDatabaseConnection(
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Server"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Company"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("User"),
                                context.Configuration.GetSection("LogisticOperatorSettings").GetValue<string>("Password")
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