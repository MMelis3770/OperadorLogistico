using B1SLayer;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console.Communication.Api;
using OperadorLogistico.Console.Communication.FileProcessing;

namespace OperadorLogistico.Console
{
    public class Program
    {
        private readonly SLConnection _slConnection;
        private readonly IDatabaseConnection _dbConnection;
        private readonly IConfiguration _configuration;
        public Program(
           IConfiguration configuration,
           SLConnection slConnection,
           IDatabaseConnection dbConnection
           )
        {
            _configuration = configuration;
            _slConnection = slConnection;
            _dbConnection = dbConnection;
        }
        static async Task Main(string[] args)
        {
            try
            {
                // Configuración
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development"}.json", optional: true)
                    .Build();

                // Configurar servicios
                var serviceProvider = ConfigureServices(configuration);
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Menú principal
                bool exit = false;
                while (!exit)
                {
                    System.Console.Clear();
                    System.Console.WriteLine("=== SISTEMA OPERADOR LOGÍSTICO ===");
                    System.Console.WriteLine("1. Procesar nuevas comandas");
                    System.Console.WriteLine("2. Ver estado de comandas");
                    System.Console.WriteLine("3. Enviar confirmaciones pendientes");
                    System.Console.WriteLine("0. Salir");
                    System.Console.Write("\nSeleccione una opción: ");

                    string option = System.Console.ReadLine();

                    switch (option)
                    {
                        case "1":
                            await ProcessNewOrdersAsync(serviceProvider);
                            break;
                        case "2":
                            await ViewOrderStatusAsync(serviceProvider);
                            break;
                        case "3":
                            await SendPendingConfirmationsAsync(serviceProvider);
                            break;
                        case "0":
                            exit = true;
                            break;
                        default:
                            System.Console.WriteLine("Opción no válida");
                            break;
                    }

                    if (!exit)
                    {
                        System.Console.WriteLine("\nPresione cualquier tecla para continuar...");
                        System.Console.ReadKey();
                    }
                }

                System.Console.WriteLine("Aplicación finalizada correctamente");
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error fatal: {ex.Message}");
                System.Console.WriteLine("Presione cualquier tecla para salir...");
                System.Console.ReadKey();
            }
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();

            //services.Configure<AppSettings>(config =>
            //{
            //    config.InputDirectory = configuration["InputDirectory"];
            //    config.OutputDirectory = configuration["OutputDirectory"];
            //});

            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddFile("logs/app.log", minimumLevel: LogLevel.Information);
            });

            // Communication - API REST
            services.AddSingleton(sp =>
            {
                var config = new ApiConfig
                {
                    BaseUrl = configuration["ApiSettings:BaseUrl"],
                    ApiKey = configuration["ApiSettings:ApiKey"],
                    TimeoutSeconds = int.Parse(configuration["ApiSettings:TimeoutSeconds"] ?? "30")
                };
                return new ApiClient(config);
            });

            // Communication - File Processing
            services.AddSingleton(sp => new OrderFileReader(configuration["InputDirectory"]));
            services.AddSingleton(sp => new ConfirmationFileWriter(configuration["OutputDirectory"]));

            // Communication - SAP Service Layer
            services.AddSingleton<SLConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<Program>>();

                try
                {
                    var client = new SLConnection(
                        configuration["SapSettings:ServiceUrl"],
                        configuration["SapSettings:CompanyDb"],
                        configuration["SapSettings:Username"],
                        configuration["SapSettings:Password"]
                    );

                    logger.LogInformation("Conexión con SAP Service Layer inicializada correctamente");
                    return client;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error al inicializar la conexión con SAP Service Layer");
                    throw;
                }
            });

            return services.BuildServiceProvider();
        }

        private static async Task ProcessNewOrdersAsync(IServiceProvider serviceProvider)
        {
            try
            {

                var orderReader = serviceProvider.GetRequiredService<OrderFileReader>();
                var newOrders = await orderReader.ReadPendingOrdersAsync();

                if (newOrders.Count == 0)
                {
                    System.Console.WriteLine("No hay nuevas comandas para procesar.");
                    return;
                }

                System.Console.WriteLine($"Se encontraron {newOrders.Count} comandas para procesar.");

                // Aquí irá el código para procesar las comandas 
                System.Console.WriteLine("Simulando procesamiento de comandas...");
                await Task.Delay(2000); // Simulación

                System.Console.WriteLine("Procesamiento completado.");
            }
            catch (Exception ex)
            {

            }
        }



        // -----------------------------------------------------------------
        //private static async Task SyncInventoryWithSapAsync(IServiceProvider serviceProvider, Logger logger, IDatabaseConnection _dbConnection)
        //{
        //    try
        //    {
        //        logger.LogInfo("Iniciando sincronización de inventario con SAP");

        //        var sapClient = serviceProvider.GetRequiredService<SLConnection>();

        //        System.Console.WriteLine("Obteniendo productos desde SAP...");

        //        try
        //        {

        //            string itemsQuery = "SELECT ItemCode, ItemName, InventoryUOM where Valid eq 'tYES'";
        //            var items = _dbConnection.Query<Product>(itemsQuery);

        //            // Convertir a nuestro modelo
        //            var products = new List<Product>();
        //            foreach (var item in items)
        //            {
        //                products.Add(new Product
        //                {
        //                    ItemCode = item.ItemCode,
        //                    ItemName = item.ItemName,
        //                    InventoryUOM = item.InventoryUOM
        //                });
        //            }

        //            System.Console.WriteLine($"Se obtuvieron {products.Count} productos.");

        //            // Ahora obtenemos los lotes
        //            System.Console.WriteLine("Obteniendo lotes desde SAP...");

        //            // Usando B1SLayer para consultar lotes

        //            string batchQuery = "SELECT ItemCode, ItemName, InventoryUOM where Valid eq 'tYES'";

        //            var batchNumbers = await sapClient.GetAsync<List<dynamic>>("BatchNumbers",
        //                select: "ItemCode,BatchNumber,Quantity,ManufacturingDate,ExpirationDate",
        //                filter: "Quantity gt 0");

        //            // Convertir a nuestro modelo
        //            var batches = new List<Batch>();
        //            foreach (var batch in batchNumbers)
        //            {
        //                batches.Add(new Batch
        //                {
        //                    BatchCode = batch.BatchNumber,
        //                    ProductCode = batch.ItemCode,
        //                    AvailableQuantity = batch.Quantity,
        //                    ProductionDate = batch.ManufacturingDate,
        //                    ExpiryDate = batch.ExpirationDate
        //                });
        //            }

        //            System.Console.WriteLine($"Se obtuvieron {batches.Count} lotes.");

        //            // Aquí guardarías estos datos en tu base de datos local
        //            System.Console.WriteLine("Simulando actualización de base de datos local...");
        //            await Task.Delay(2000); // Simulación

        //            System.Console.WriteLine("Sincronización completada.");
        //        }
        //        catch (Exception ex)
        //        {
        //            logger.LogError($"Error en la comunicación con SAP: {ex.Message}");
        //            System.Console.WriteLine($"Error en la comunicación con SAP: {ex.Message}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError("Error al sincronizar inventario con SAP", ex);
        //    }
        //}

        private static async Task ViewOrderStatusAsync(IServiceProvider serviceProvider)
        {
            System.Console.WriteLine("Funcionalidad pendiente de implementar.");
            await Task.CompletedTask;
        }

        private static async Task SendPendingConfirmationsAsync(IServiceProvider serviceProvider)
        {
            try
            {

                var apiClient = serviceProvider.GetRequiredService<ApiClient>();

                // Aquí obtendrías las confirmaciones pendientes de tu base de datos local
                System.Console.WriteLine("Simulando obtención de confirmaciones pendientes...");
                await Task.Delay(1000); // Simulación

                System.Console.WriteLine("No hay confirmaciones pendientes para enviar.");
            }
            catch (Exception ex)
            {

            }
        }
    }

    // Extensión simple para añadir logging a archivos
    public static class FileLoggerExtensions
    {
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, string filePath,
            LogLevel minimumLevel = LogLevel.Information)
        {
            builder.AddProvider(new FileLoggerProvider(filePath, minimumLevel));
            return builder;
        }
    }

    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _path;
        private readonly LogLevel _minimumLevel;

        public FileLoggerProvider(string path, LogLevel minimumLevel)
        {
            _path = path;
            _minimumLevel = minimumLevel;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_path, categoryName, _minimumLevel);
        }

        public void Dispose() { }
    }

    public class FileLogger : ILogger
    {
        private readonly string _path;
        private readonly string _categoryName;
        private readonly LogLevel _minimumLevel;

        public FileLogger(string path, string categoryName, LogLevel minimumLevel)
        {
            _path = path;
            _categoryName = categoryName;
            _minimumLevel = minimumLevel;

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logLevel}] [{_categoryName}] {formatter(state, exception)}";
            if (exception != null)
            {
                logEntry += $"{Environment.NewLine}Exception: {exception}";
            }

            try
            {
                File.AppendAllText(_path, logEntry + Environment.NewLine);
            }
            catch { /* Ignorar errores al escribir en el log */ }
        }
    }
}