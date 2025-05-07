using System.Reflection;
using Console.Processes;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OperadorLogistico.Console
{
    public interface ISqlServerConnection : IDatabaseConnection
    {
        string GetSapDbName();
        string GetLogisticDbName();
    }

    public class SqlServerConnection : SQLDatabaseConnection, ISqlServerConnection
    {
        private readonly string _sapDbName;
        private readonly string _logisticDbName;

        public SqlServerConnection(string server, string sapDbName, string logisticDbName, string user, string password)
            : base(server, "", user, password)
        {
            _sapDbName = sapDbName;
            _logisticDbName = logisticDbName;
        }

        public string GetSapDbName() => _sapDbName;
        public string GetLogisticDbName() => _logisticDbName;
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

                        services.AddSingleton<ISqlServerConnection>(sp =>
                        {
                            return new SqlServerConnection(
                                context.Configuration.GetSection("DBSettings").GetValue<string>("ServiceUrl"),
                                context.Configuration.GetSection("DBSettings").GetValue<string>("SAPCompanyDb"),
                                context.Configuration.GetSection("DBSettings").GetValue<string>("LOCompanyDb"),
                                context.Configuration.GetSection("DBSettings").GetValue<string>("Username"),
                                context.Configuration.GetSection("DBSettings").GetValue<string>("Password")
                            );
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