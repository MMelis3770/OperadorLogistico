using BlazorTemplate.Models;
using DatabaseConnection;
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
            string insertQuery = "INSERT INTO dbo.Comandas"
        }
    }
}