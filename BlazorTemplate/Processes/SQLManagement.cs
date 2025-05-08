using BlazorTemplate.Models;
using DatabaseConnection;
using Microsoft.Extensions.Configuration;

namespace BlazorTemplate.Processes
{
    public class SQLManagement
    {
        private readonly IConfiguration _configuration;
        private static IDatabaseConnection _connection;
        private static IEnumerable<Client> clients;
        private static IEnumerable<OrderData> ordersDate;
        private static IEnumerable<LineItem> items;

        // Static constructor to ensure static fields are initialized
        static SQLManagement()
        {
            clients = new List<Client>();
            ordersDate = new List<OrderData>();
            items = new List<LineItem>();
        }

        public SQLManagement(IConfiguration configuration, IDatabaseConnection connection)
        {
            _configuration = configuration;
            _connection = connection; // Ensure the static connection is set
            clients = new List<Client>();
        }

        public static void ValidateActiveClients()
        {
            string validateClientsQuery = $"SELECT CardCode FROM dbo.Client;"; // WHERE ACTIVE = 'Y'
            clients = _connection.Query<Client>(validateClientsQuery);
        }

        private void ValidateOrderDate()
        {
            // ---------------------------------------
            // MODIFICAR QUERY PARA QUE FILTRE POR EL CURRENT DATE
            // ---------------------------------------
            string validateOrderDateQuery = $"SELECT DocEntry, CardCode, OrderDate and DocDueDate FROM dbo.Orders;";
            ordersDate = _connection.Query<OrderData>(validateOrderDateQuery);
        }

        private void GetItems()
        {
            string validateItemCode = $"SELECT ItemCode FROM dbo.Inventory";
            items = _connection.Query<LineItem>(validateItemCode);

            // POSTERIOR A ESTO VERIFICAR QUE COINCIDA O PASARLO POR ARGUMENTO
        }

        private void GetBatches()
        {
            // BUSCAR LOTE POR ARGUMENTO DE ITEMCODE Y VALIDAR CON FECHA CURRENT
        }
    }
}