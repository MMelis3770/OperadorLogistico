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

                // Use Query correctly with parameters
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

                // Create an anonymous object for parameters - this works with the Query method
                var parameters = new
                {
                    DocEntry = docEntry,
                    IsProcessed = isProcessed ? 1 : 0
                };

                // For UPDATE operations that don't return results, we need to use Execute with a list
                // Create a list with a single element for the parameter object
                var paramList = new List<object> { parameters };

                // Execute the update query
                _connection.Execute(updateQuery, paramList);

                // Simulate an asynchronous operation
                await Task.Delay(50);

                // Since we can't directly know how many rows were affected (Execute doesn't return a value)
                // verify if the order exists after the update
                var verifyQuery = "SELECT COUNT(1) FROM dbo.Orders WHERE DocEntry = @DocEntry AND IsProcessed = @IsProcessed";
                var result = _connection.Query<int>(verifyQuery, parameters).FirstOrDefault();

                // Return true if at least one record was found with the updated values
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

                // Create an anonymous object for parameters and wrap it in a list
                var paramObj = new
                {
                    DocEntry = docEntry,
                    ErrorMessage = errorMessage
                };

                var parametersList = new List<object> { paramObj };

                // Execute the update query
                _connection.Execute(updateQuery, parametersList);

                // Verify if the update was successful by querying afterwards
                var verifyQuery = "SELECT COUNT(1) FROM dbo.Orders WHERE DocEntry = @DocEntry AND HasError = 1";
                var result = _connection.Query<int>(verifyQuery, new { DocEntry = docEntry }).FirstOrDefault();

                // Simulate an asynchronous operation
                await Task.Delay(50);

                // Return true if the updated record was found
                return result > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating error status for order {docEntry}: {ex.Message}", ex);
            }
        }

        // Method to get an order with its error information
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

                // Get basic order data
                var order = _connection.Query<OrderData>(query, parameters).FirstOrDefault();

                if (order == null)
                    throw new Exception($"Order with DocEntry {docEntry} not found");

                // Get order lines
                order.LineItems = GetOrderLinesFromDatabase(docEntry);

                return order;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order {docEntry} with error information: {ex.Message}", ex);
            }
        }

        public bool GetItems(string itemCode)
        {
            try
            {
                string validateItemCode = "SELECT ItemCode FROM dbo.Inventory";
                items = _connection.Query<LineItem>(validateItemCode);

                // Check if the item exists
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
                // Start transaction
                _connection.StartTransaction();

                try
                {
                    foreach (var order in ordersToSQL)
                    {
                        // Insert into Orders table
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

                        // Insert order lines
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
                                    DocEntry = order.DocEntry,  // Use the parent order's DocEntry
                                    LineNum = line.LineNum,
                                    ItemCode = line.ItemCode,
                                    Quantity = line.Quantity
                                };
                                lineItemParameters.Add(item);
                            }



                            //var lineItemsParameters = order.LineItems.Select(line => new
                            //{
                            //    DocEntry = order.DocEntry,  // Use the parent order's DocEntry
                            //    LineNum = line.LineNum,
                            //    ItemCode = line.ItemCode,
                            //    Quantity = line.Quantity
                            //}).ToList();

                            _connection.Execute(insertLineQuery, lineItemParameters);
                        }
                    }

                    // Commit transaction if everything went well
                    _connection.EndTransaction(EndTransactionType.Commit);
                }
                catch (Exception)
                {
                    // Rollback on error
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

                // Simulate an asynchronous operation
                await Task.Delay(50);

                return result ?? "No error message found for this order";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting error message for order {docEntry}: {ex.Message}", ex);
            }
        }

        // Method to get all orders from the database
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

                // Define a specific class for the result
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
                        LineItems = new List<LineItem>() // Will be filled if necessary with GetOrderLinesFromDatabase
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

                await Task.Delay(50); // Simulate asynchronous operation

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

        // Direct method to get assigned batches
        public List<AssignedBatch> GetBatchesDirectFromDatabase(int docEntry, int lineNumber)
        {
            try
            {
                // Make a simple and direct query, only filtering by DocEntry
                string query = @"
        SELECT DocEntry, LineNum, BatchNum, Quantity 
        FROM [TestOperadorLogistic].[dbo].[AssignedBatches] 
        WHERE DocEntry = @DocEntry";

                var parameters = new { DocEntry = docEntry };

                // Execute the query and get the results
                var results = _connection.Query<AssignedBatch>(query, parameters).ToList();

                // Print all results without further filtering
                Console.WriteLine($"*** DIRECT QUERY: Batches for DocEntry={docEntry}: {results.Count} ***");
                foreach (var batch in results)
                {
                    Console.WriteLine($"Batch found: DocEntry={batch.DocEntry}, LineNum={batch.LineNum}, BatchNum={batch.BatchNum}, Quantity={batch.Quantity}");
                }

                return results;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in direct query: {ex.Message}");
                return new List<AssignedBatch>();
            }
        }

        // Method to get assigned batches for an order from the AssignedBatches table
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

                // Simulate asynchronous operation
                await Task.Delay(50);

                return assignedBatches.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting assigned batches for order {docEntry}: {ex.Message}", ex);
            }
        }

        // Add this method to get batches by ItemCode
        public async Task<List<Batch>> GetBatchesForItemAsync(string itemCode)
        {
            try
            {
                string query;
                object parameters = null;

                if (string.IsNullOrEmpty(itemCode))
                {
                    // If no ItemCode is specified, get all batches
                    query = @"
                SELECT BatchNum as BatchId, ItemCode, PrdDate as StartDate, 
                       ExpDate as EndDate, Quantity as AvailableQuantity 
                FROM dbo.Batches 
                ORDER BY ExpDate ASC";
                }
                else
                {
                    // Filter by specific ItemCode
                    query = @"
                SELECT BatchNum as BatchId, ItemCode, PrdDate as StartDate, 
                       ExpDate as EndDate, Quantity as AvailableQuantity 
                FROM dbo.Batches 
                WHERE ItemCode = @ItemCode 
                ORDER BY ExpDate ASC";
                    parameters = new { ItemCode = itemCode };
                }

                await Task.Delay(50); // Simulate asynchronous operation

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

        // Method to get all orders from the database, including processed and with errors
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
ORDER BY o.DocEntry DESC";  // Order by descending ID to see the most recent first

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
                        LineItems = new List<LineItem>() // Will be filled if necessary with GetOrderLinesFromDatabase
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting all orders from database: {ex.Message}", ex);
            }
        }

        // Method to get all orders with errors from the database
        public List<OrderData> GetAllOrdersWithErrorsFromDatabase()
        {
            try
            {
                string query = @"
SELECT o.DocEntry, o.CardCode, o.OrderDate, o.DocDueDate, o.IsProcessed, o.HasError, o.ErrorMessage,
       COUNT(l.LineNum) as LineCount 
FROM dbo.Orders o 
LEFT JOIN dbo.OrderLines l ON o.DocEntry = l.DocEntry 
WHERE o.HasError = 1
GROUP BY o.DocEntry, o.CardCode, o.OrderDate, o.DocDueDate, o.IsProcessed, o.HasError, o.ErrorMessage
ORDER BY o.DocEntry DESC";

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
                        LineItems = new List<LineItem>() // Will be filled if necessary with GetOrderLinesFromDatabase
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting all orders from database: {ex.Message}", ex);
            }
        }

        // Method to insert batch assignments when an order is processed
        public async Task<bool> SaveAssignedBatchesAsync(int docEntry, List<LineItem> lineItems)
        {
            try
            {
                // Filter lines with assigned batches
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

                await Task.Delay(50); // Simulate asynchronous operation
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving assigned batches for order {docEntry}: {ex.Message}", ex);
            }
        }

        // Method to get lines for a specific order
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