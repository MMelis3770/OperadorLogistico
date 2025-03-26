using B1SLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console;
using OperadorLogistico.Console.Communication.Api;
using OperadorLogistico.Console.Models;

namespace Console.Processes
{
    public class InventorySync
    {
        private readonly SLConnection _slConnection;
        private readonly ISapDatabaseConnection _SAPdbConnection;
        private readonly ILogisticOperatorDatabaseConnection _LOdbConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InventorySync> _logger;
        private readonly List<Product> _inventory;
        private readonly List<Batch> _batches;

        public InventorySync(
            IConfiguration configuration,
            SLConnection slConnection,
            ISapDatabaseConnection SAPdbConnection,
            ILogisticOperatorDatabaseConnection LOdbConnection,
            ILogger<InventorySync> logger)
        {
            _configuration = configuration;
            _slConnection = slConnection;
            _SAPdbConnection = SAPdbConnection;
            _LOdbConnection = LOdbConnection;
            _logger = logger;
            _inventory = new List<Product>();
            _batches = new List<Batch>();
        }
        public void Execute()
        {
            //GetInventoryFromSAP();
            //SetInventoryToLogisticOperator();
        }
        private void GetInventoryFromSAP()
        {
            //Select to get all the products in SAP
            string InventoryQuery = "SELECT ItemCode, ItemName, UnitPrice, Quantity, Category FROM OITM";
            var inventory = _SAPdbConnection.Query<Product>(InventoryQuery);
            foreach (Product product in inventory)
            {
                if (product != null)
                {
                    _inventory.Add(product);
                }
            }
            _logger.LogInformation("Inventory loaded sucessfully: {InventoryCount}", _inventory.Count);
        }
        private void GetBatchesFromSAP()
        {
            string BatchQuery = "";
            var batches = _SAPdbConnection.Query<Batch>(BatchQuery);
            foreach(Batch batch in batches)
            {
                if(batch!= null)
                {
                    _batches.Add(batch);
                }
            }
            _logger.LogInformation("Batches loaded sucessfully: {Batches}", _batches.Count);
        }
        private void SetInventoryToLogisticOperator()
        {
            foreach (Product product in _inventory)
            {
                if (product != null)
                {
                    string InventoryInsert = $"INSERT INTO LogisticInventory (ItemCode, ItemName, UnitPrice, Quantity, Category) VALUES({product.ItemCode}, {product.ItemName}, {product.UnitPrice}, {product.InventoryUOM}, {product.Category})";
                    _LOdbConnection.Query<Product>(InventoryInsert);
                }
            }
            _logger.LogInformation("Inventory imported sucessfully: {InventoryCount}", _inventory.Count);
        }
        private void SetBatchesToLogisticOperator()
        {
            foreach (Product product in _inventory)
            {
                if (product != null)
                {
                    string InventoryInsert = $")";
                    _LOdbConnection.Query<Product>(InventoryInsert);
                }
            }
            _logger.LogInformation("Inventory imported sucessfully: {InventoryCount}", _inventory.Count);
        }
    }
}
