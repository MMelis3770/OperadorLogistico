using B1SLayer;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console.Communication.Api;
using OperadorLogistico.Console.Communication.FileProcessing;

namespace OperadorLogistico.Console.Processes
{
    public class OrderManagement
    {
        private readonly SLConnection _slConnection;
        private readonly ISapDatabaseConnection _SAPdbConnection;
        private readonly ILogisticOperatorDatabaseConnection _LOdbConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OrderManagement> _logger;
        private readonly ApiClient _apiClient;
        private readonly OrderFileReader _orderReader;

        public OrderManagement(
            IConfiguration configuration,
            SLConnection slConnection,
            ISapDatabaseConnection SAPdbConnection,
            ILogisticOperatorDatabaseConnection LOdbConnection,
            ILogger<OrderManagement> logger,
            ApiClient apiClient,
            OrderFileReader orderReader)
        {
            _configuration = configuration;
            _slConnection = slConnection;
            _SAPdbConnection = SAPdbConnection;
            _LOdbConnection = LOdbConnection;
            _logger = logger;
            _apiClient = apiClient;
            _orderReader = orderReader;
        }

        public void Execute()
        {
            try
            {
                _logger.LogInformation("Iniciando proceso de gestión de órdenes");

                // Process new orders
                ProcessNewOrdersAsync().GetAwaiter().GetResult();

                // Send any pending confirmations
                SendPendingConfirmationsAsync().GetAwaiter().GetResult();

                _logger.LogInformation("Proceso de gestión de órdenes completado");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la ejecución del proceso: {Message}", ex.Message);
                throw;
            }
        }

        public async Task RunConsoleMenuAsync()
        {
            try
            {
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
                            await ProcessNewOrdersAsync();
                            break;
                        case "2":
                            await ViewOrderStatusAsync();
                            break;
                        case "3":
                            await SendPendingConfirmationsAsync();
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
                _logger.LogError(ex, "Error fatal: {Message}", ex.Message);
                System.Console.WriteLine($"Error fatal: {ex.Message}");
                System.Console.WriteLine("Presione cualquier tecla para salir...");
                System.Console.ReadKey();
            }
        }

        private async Task ProcessNewOrdersAsync()
        {
            try
            {
                var newOrders = await _orderReader.ReadPendingOrdersAsync();

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
                _logger.LogError(ex, "Error al procesar nuevas órdenes: {Message}", ex.Message);
            }
        }

        private async Task ViewOrderStatusAsync()
        {
            System.Console.WriteLine("Funcionalidad pendiente de implementar.");
            await Task.CompletedTask;
        }

        private async Task SendPendingConfirmationsAsync()
        {
            try
            {
                // Aquí obtendrías las confirmaciones pendientes de tu base de datos local
                System.Console.WriteLine("Simulando obtención de confirmaciones pendientes...");
                await Task.Delay(1000); // Simulación

                System.Console.WriteLine("No hay confirmaciones pendientes para enviar.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar confirmaciones pendientes: {Message}", ex.Message);
            }
        }
    }
}


        // -----------------------------------------------------------------
        //private async Task SyncInventoryWithSapAsync()
        //{
        //    try
        //    {
        //        _logger.LogInformation("Iniciando sincronización de inventario con SAP");

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

        //            var batchNumbers = await _slConnection.GetAsync<List<dynamic>>("BatchNumbers",
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
        //            _logger.LogError("Error en la comunicación con SAP: {Message}", ex.Message);
        //            System.Console.WriteLine($"Error en la comunicación con SAP: {ex.Message}");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error al sincronizar inventario con SAP");
        //    }
        //}
