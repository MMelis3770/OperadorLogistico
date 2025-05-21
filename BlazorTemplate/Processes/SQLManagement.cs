using System.Data.Common;
using BlazorTemplate.Models;
using DatabaseConnection;
using DatabaseConnection.Enum;
using Microsoft.Extensions.Configuration;
using static Microsoft.FluentUI.AspNetCore.Components.Icons.Filled.Size20;

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

        public async Task<bool> UpdateOrderProcessedStatusAsync(int docEntry, bool isProcessed = true)
        {
            try
            {
                string updateQuery = @"
            UPDATE dbo.Orders 
            SET IsProcessed = @IsProcessed
            WHERE DocEntry = @DocEntry";

                // Crear un objeto anónimo para los parámetros - esto funciona con el método Query
                var parameters = new
                {
                    DocEntry = docEntry,
                    IsProcessed = isProcessed ? 1 : 0
                };

                // Para operaciones UPDATE que no devuelven resultados, necesitamos usar Execute con una lista
                // Creamos una lista con un solo elemento para el objeto de parámetros
                var paramList = new List<object> { parameters };

                // Ejecutar la consulta de actualización
                _connection.Execute(updateQuery, paramList);

                // Simular una operación asíncrona
                await Task.Delay(50);

                // Como no podemos saber directamente cuántas filas se afectaron (Execute no devuelve un valor)
                // verificamos si la orden existe después de la actualización
                var verifyQuery = "SELECT COUNT(1) FROM dbo.Orders WHERE DocEntry = @DocEntry AND IsProcessed = @IsProcessed";
                var result = _connection.Query<int>(verifyQuery, parameters).FirstOrDefault();

                // Devolver true si se encontró al menos un registro con los valores actualizados
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating processed status for order {docEntry}: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateOrderErrorStatusAsync(int docEntry, string errorMessage)
        {
            try
            {
                string updateQuery = @"
            UPDATE dbo.Orders 
            SET HasError = 1,
                ErrorMessage = @ErrorMessage
            WHERE DocEntry = @DocEntry";

                // Crear un objeto anónimo para los parámetros y envolverlo en una lista
                var paramObj = new
                {
                    DocEntry = docEntry,
                    ErrorMessage = errorMessage
                };

                var parametersList = new List<object> { paramObj };

                // Ejecutar la consulta de actualización
                _connection.Execute(updateQuery, parametersList);

                // Verificar si la actualización fue exitosa consultando después
                var verifyQuery = "SELECT COUNT(1) FROM dbo.Orders WHERE DocEntry = @DocEntry AND HasError = 1";
                var result = _connection.Query<int>(verifyQuery, new { DocEntry = docEntry }).FirstOrDefault();

                // Simular una operación asíncrona
                await Task.Delay(50);

                // Devolver true si se encontró el registro actualizado
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating error status for order {docEntry}: {ex.Message}", ex);
            }
        }

        // Método para obtener una orden con su información de error
        public OrderData GetOrderWithErrorInfoFromDB(int docEntry)
        {
            try
            {
                string query = @"
            SELECT DocEntry, CardCode, OrderDate, DocDueDate, 
                   IsProcessed, HasError, ErrorMessage 
            FROM dbo.Orders 
            WHERE DocEntry = @DocEntry";

                var parameters = new { DocEntry = docEntry };

                // Obtener datos básicos de la orden
                var order = _connection.Query<OrderData>(query, parameters).FirstOrDefault();

                if (order == null)
                    throw new Exception($"No se encontró la orden con DocEntry {docEntry}");

                // Obtener las líneas de la orden
                order.LineItems = GetOrderLinesFromDatabase(docEntry);

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo la orden {docEntry} con información de error: {ex.Message}", ex);
            }
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

        public async Task<string> GetOrderErrorMessageAsync(int docEntry)
        {
            try
            {
                string query = @"
            SELECT ErrorMessage 
            FROM dbo.Orders 
            WHERE DocEntry = @DocEntry AND HasError = 1";

                var parameters = new { DocEntry = docEntry };

                var result = _connection.Query<string>(query, parameters).FirstOrDefault();

                // Simular una operación asíncrona
                await Task.Delay(50);

                return result ?? "No se encontró mensaje de error para esta orden";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error obteniendo mensaje de error para orden {docEntry}: {ex.Message}", ex);
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
    WHERE o.IsProcessed = 0 AND o.HasError = 0
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

        // Método directo para obtener lotes asignados
        public List<AssignedBatch> GetBatchesDirectFromDatabase(int docEntry, int lineNumber)
        {
            try
            {
                // Hacer una consulta simple y directa, solo filtrando por DocEntry
                string query = @"
        SELECT DocEntry, LineNum, BatchNum, Quantity 
        FROM [TestOperadorLogistic].[dbo].[AssignedBatches] 
        WHERE DocEntry = @DocEntry";

                var parameters = new { DocEntry = docEntry };

                // Ejecutar la consulta y obtener los resultados
                var results = _connection.Query<AssignedBatch>(query, parameters).ToList();

                // Imprimir todos los resultados sin filtrar más
                Console.WriteLine($"*** CONSULTA DIRECTA: Lotes para DocEntry={docEntry}: {results.Count} ***");
                foreach (var batch in results)
                {
                    Console.WriteLine($"Lote encontrado: DocEntry={batch.DocEntry}, LineNum={batch.LineNum}, BatchNum={batch.BatchNum}, Quantity={batch.Quantity}");
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en consulta directa: {ex.Message}");
                return new List<AssignedBatch>();
            }
        }

        // Método para obtener los lotes asignados a una orden desde la tabla AssignedBatches
        public async Task<List<AssignedBatch>> GetAssignedBatchesForOrderAsync(int docEntry)
        {
            try
            {
                string query = @"
            SELECT DocEntry, LineNum, BatchNum, Quantity 
            FROM [TestOperadorLogistic].[dbo].[AssignedBatches] 
            WHERE DocEntry = @DocEntry 
            ORDER BY LineNum, BatchNum";

                var parameters = new { DocEntry = docEntry };

                var assignedBatches = _connection.Query<AssignedBatch>(query, parameters);

                // Simular operación asíncrona
                await Task.Delay(50);

                return assignedBatches.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assigned batches for order {docEntry}: {ex.Message}", ex);
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

        // Método para obtener todas las órdenes desde la base de datos, incluyendo procesadas y con errores
        public List<OrderData> GetAllOrdersFromDatabase()
        {
            try
            {
                string query = @"
SELECT o.DocEntry, o.CardCode, o.OrderDate, o.DocDueDate, o.IsProcessed, o.HasError, o.ErrorMessage,
       COUNT(l.LineNum) as LineCount 
FROM dbo.Orders o 
LEFT JOIN dbo.OrderLines l ON o.DocEntry = l.DocEntry 
GROUP BY o.DocEntry, o.CardCode, o.OrderDate, o.DocDueDate, o.IsProcessed, o.HasError, o.ErrorMessage
ORDER BY o.DocEntry DESC";  // Ordenar por ID descendente para ver las más recientes primero

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
                        IsProcessed = item.IsProcessed,
                        HasError = item.HasError,
                        ErrorMessage = item.ErrorMessage,
                        LineItems = new List<LineItem>() // Se llenarán si es necesario con GetOrderLinesFromDatabase
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting all orders from database: {ex.Message}", ex);
            }
        }

        // Método para insertar asignaciones de batch cuando una orden es procesada
        public async Task<bool> SaveAssignedBatchesAsync(int docEntry, List<LineItem> lineItems)
        {
            try
            {
                // Filtrar líneas con lotes asignados
                var linesWithBatches = lineItems.Where(li => !string.IsNullOrEmpty(li.Batch)).ToList();

                foreach (var line in linesWithBatches)
                {
                    string insertQuery = @"
                INSERT INTO dbo.AssignedBatches (DocEntry, LineNum, BatchNum, Quantity) 
                VALUES (@DocEntry, @LineNum, @BatchNum, @Quantity)";

                    var parameters = new List<object>
            {
                new
                {
                    DocEntry = docEntry,
                    LineNum = line.LineNum,
                    BatchNum = line.Batch,
                    Quantity = line.Quantity
                }
            };

                    _connection.Execute(insertQuery, parameters);
                }

                await Task.Delay(50); // Simular operación asíncrona
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving assigned batches for order {docEntry}: {ex.Message}", ex);
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