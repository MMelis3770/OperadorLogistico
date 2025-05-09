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
        private IEnumerable<OrderData> ordersDate;
        private IEnumerable<LineItem> items;
        private IEnumerable<Batch> batches;

        public SQLManagement(IConfiguration configuration, IDatabaseConnection connection)
        {
            _configuration = configuration;
            _connection = connection;
            clients = new List<Client>();
            ordersDate = new List<OrderData>();
            items = new List<LineItem>();
            batches = new List<Batch>();
        }

        public bool ValidateActiveClients(string cardCode)
        {
            try
            {
                string validateClientsQuery = "SELECT CardCode FROM dbo.Client";
                clients = _connection.Query<Client>(validateClientsQuery);

                // Verificar si el cliente existe
                return clients.Any(c => c.CardCode == cardCode);
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

                            var lineItemsParameters = order.LineItems.Select(line => new
                            {
                                DocEntry = order.DocEntry,  // Usar el DocEntry de la orden padre
                                LineNum = line.LineNum,
                                ItemCode = line.ItemCode,
                                Quantity = line.Quantity
                            }).ToList();

                            _connection.Execute(insertLineQuery, lineItemsParameters);
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
        }
    }