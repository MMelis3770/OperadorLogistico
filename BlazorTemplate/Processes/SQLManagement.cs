using System.Data.Common;
using BlazorTemplate.Models;
using DatabaseConnection;
using DatabaseConnection.Enum;
using Microsoft.Extensions.Configuration;

namespace BlazorTemplate.Processes
{
    public class SQLManagement
    {
        private readonly IConfiguration _configuration;
        private readonly IDatabaseConnection _connection;
        private IEnumerable<Client> clients;
        private IEnumerable<OrderData> ordersData;
        private IEnumerable<LineItem> items;
        private IEnumerable<Batch> batches;

        public SQLManagement(IConfiguration configuration, IDatabaseConnection connection)
        {
            _configuration = configuration;
            _connection = connection;
            clients = new List<Client>();
            ordersData = new List<OrderData>();
            items = new List<LineItem>();
            batches = new List<Batch>();
        }

        public bool ValidateActiveClients(string cardCode)
        {
            try
            {
                string validateClientsQuery = "SELECT CardCode FROM dbo.Client WHERE CardCode = @CardCode";
                var parameters = new { CardCode = cardCode };

                // Usar Query correctamente con los parámetros
                clients = _connection.Query<Client>(validateClientsQuery, parameters);

                return clients.Any();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error validating client {cardCode}: {ex.Message}", ex);
            }
        }

        public void ValidateOrderDate(DateTime orderDate, DateTime docDueDate)
        {
            // PENDIENTE REVISAR PARA BATCH
            //try
            //{
            //    var currentDate = DateTime.Now;
            //    return currentDate >= orderDate && currentDate <= docDueDate;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception($"Error validating order dates: {ex.Message}", ex);
            //}
        }

        public bool GetItems(string itemCode)
        {
            try
            {
                string validateItemCode = "SELECT ItemCode FROM dbo.Inventory";
                items = _connection.Query<LineItem>(validateItemCode);

                // Verificar si el item existe
                return items.Any(c => c.ItemCode == itemCode);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting items: {ex.Message}", ex);
            }
        }

        public bool GetBatches(string itemCode)
        {
            // Escapar caracteres para prevenir SQL injection
            if (string.IsNullOrWhiteSpace(itemCode))
                throw new ArgumentException("ItemCode cannot be null or empty");

            // Escapar comillas simples
            itemCode = itemCode.Replace("'", "''");

            string batchQuery = $"SELECT * FROM dbo.Batches WHERE ItemCode = '{itemCode}'";
            batches = _connection.Query<Batch>(batchQuery).ToList();

            return batches != null && batches.Any();
        }

        public void LoadOrdersToSQL(List<OrderData> ordersToSQL)
        {
            if (ordersToSQL == null || !ordersToSQL.Any())
                throw new ArgumentException("No orders to insert");

            try
            {
                // Iniciar transacción
                _connection.StartTransaction();

                try
                {
                    foreach (var order in ordersToSQL)
                    {
                        // Insertar en la tabla Orders
                        string insertOrderQuery = @"
                        INSERT INTO dbo.Orders (DocEntry, CardCode, OrderDate, DocDueDate) 
                        VALUES (@DocEntry, @CardCode, @OrderDate, @DocDueDate)";

                        var orderParameters = new List<object>
                        {
                            new
                            {
                                DocEntry = order.DocEntry,
                                CardCode = order.CardCode,
                                OrderDate = order.OrderDate,
                                DocDueDate = order.DocDueDate
                            }
                        };

                        _connection.Execute(insertOrderQuery, orderParameters);

                        // Insertar las líneas de la orden
                        if (order.LineItems != null && order.LineItems.Any())
                        {
                            string insertLineQuery = @"
                            INSERT INTO dbo.OrderLines (DocEntry, LineNum, ItemCode, Quantity) 
                            VALUES (@DocEntry, @LineNum, @ItemCode, @Quantity)";

                            var lineItemParameters = new List<LineItem>();

                            foreach (LineItem line in order.LineItems)
                            {
                                var item = new LineItem
                                {
                                    DocEntry = order.DocEntry,  // Usar el DocEntry de la orden padre
                                    LineNum = line.LineNum,
                                    ItemCode = line.ItemCode,
                                    Quantity = line.Quantity
                                };
                                lineItemParameters.Add(item);
                            }

                            

                            //var lineItemsParameters = order.LineItems.Select(line => new
                            //{
                            //    DocEntry = order.DocEntry,  // Usar el DocEntry de la orden padre
                            //    LineNum = line.LineNum,
                            //    ItemCode = line.ItemCode,
                            //    Quantity = line.Quantity
                            //}).ToList();

                            _connection.Execute(insertLineQuery, lineItemParameters);
                        }
                    }

                    // Commit de la transacción si todo salió bien
                    _connection.EndTransaction(EndTransactionType.Commit);
                }
                catch (Exception)
                {
                    // Rollback en caso de error
                    if (_connection.InTransaction)
                    {
                        _connection.EndTransaction(EndTransactionType.Rollback);
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading orders to SQL: {ex.Message}", ex);
            }
        }

        // Método para obtener todas las órdenes desde la base de datos
        public List<OrderData> GetOrdersFromDatabase()
        {
            try
            {
                string query = @"
        SELECT o.DocEntry, o.CardCode, o.OrderDate, o.DocDueDate, 
               COUNT(l.LineNum) as LineCount 
        FROM dbo.Orders o 
        LEFT JOIN dbo.OrderLines l ON o.DocEntry = l.DocEntry 
        GROUP BY o.DocEntry, o.CardCode, o.OrderDate, o.DocDueDate";

                // Definir una clase específica para el resultado
                var ordersWithCount = _connection.Query<OrderDataWithCount>(query);

                List<OrderData> result = new List<OrderData>();

                foreach (var item in ordersWithCount)
                {
                    result.Add(new OrderData
                    {
                        DocEntry = item.DocEntry,
                        CardCode = item.CardCode,
                        OrderDate = item.OrderDate,
                        DocDueDate = item.DocDueDate,
                        LineItems = new List<LineItem>() // Se llenarán si es necesario con GetOrderLinesFromDatabase
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting orders from database: {ex.Message}", ex);
            }
        }

        public async Task<List<Batch>> GetAllBatchesAsync()
        {
            try
            {
                string query = @"
            SELECT BatchNum as BatchId, ItemCode, PrdDate as StartDate, 
                   ExpDate as EndDate, Quantity as AvailableQuantity 
            FROM dbo.Batches 
            ORDER BY ExpDate ASC";

                await Task.Delay(50); // Simular operació asíncrona

                var batchesData = _connection.Query<dynamic>(query);

                List<Batch> batches = new List<Batch>();

                foreach (var item in batchesData)
                {
                    batches.Add(new Batch(
                        batchId: item.BatchId,
                        startDate: (DateTime)item.StartDate,
                        endDate: (DateTime)item.EndDate,
                        itemCode: item.ItemCode,
                        availableQuantity: item.AvailableQuantity
                    ));
                }

                return batches;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting all batches from database: {ex.Message}", ex);
            }
        }

        // Afegir aquest mètode per obtenir batches per ItemCode
        public async Task<List<Batch>> GetBatchesForItemAsync(string itemCode)
        {
            try
            {
                string query;
                object parameters = null;

                if (string.IsNullOrEmpty(itemCode))
                {
                    // Si no s'especifica ItemCode, obtenir tots els batches
                    query = @"
                SELECT BatchNum as BatchId, ItemCode, PrdDate as StartDate, 
                       ExpDate as EndDate, Quantity as AvailableQuantity 
                FROM dbo.Batches 
                ORDER BY ExpDate ASC";
                }
                else
                {
                    // Filtrar per ItemCode específic
                    query = @"
                SELECT BatchNum as BatchId, ItemCode, PrdDate as StartDate, 
                       ExpDate as EndDate, Quantity as AvailableQuantity 
                FROM dbo.Batches 
                WHERE ItemCode = @ItemCode 
                ORDER BY ExpDate ASC";
                    parameters = new { ItemCode = itemCode };
                }

                await Task.Delay(50); // Simular operació asíncrona

                var batchesData = _connection.Query<dynamic>(query, parameters);

                List<Batch> batches = new List<Batch>();

                foreach (var item in batchesData)
                {
                    batches.Add(new Batch(
                        batchId: item.BatchId,
                        startDate: (DateTime)item.StartDate,
                        endDate: (DateTime)item.EndDate,
                        itemCode: item.ItemCode,
                        availableQuantity: item.AvailableQuantity
                    ));
                }

                return batches;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting batches for item {itemCode} from database: {ex.Message}", ex);
            }
        }


        // Método para obtener las líneas de una orden específica
        public List<LineItem> GetOrderLinesFromDatabase(int docEntry)
        {
            try
            {
                string query = @"
                SELECT LineNum, ItemCode, Quantity 
                FROM dbo.OrderLines 
                WHERE DocEntry = @DocEntry 
                ORDER BY LineNum";

                var parameters = new { DocEntry = docEntry };

                var lines = _connection.Query<LineItem>(query, parameters);

                return lines.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order lines from database: {ex.Message}", ex);
            }
        }
    }
}