using B1SLayer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console;
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
        private readonly Dictionary<Product, bool> _inventory;
        private readonly Dictionary<Batch, bool> _batches;
        private int _ErrorCount;
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
            _inventory = new Dictionary<Product, bool>();
            _batches = new Dictionary<Batch, bool>();
            _ErrorCount = 0;
        }
        public void Execute()
        {
            GetInventory();
            SetInventory();
            GetBatches();
            ValidateBatches();
            SetBatches();

        }
        private void GetInventory()
        {
            //Select to get all the products in SAP
            string InventoryQuery = "SELECT ItemCode, ItemName, OnHand, UnitPrice FROM OITM Where OnHand < 0";
            var inventory = _SAPdbConnection.Query<Product>(InventoryQuery);
            foreach (Product product in inventory)
            {
                if (product != null)
                {
                    _inventory[product] = true;
                }
            }
            _logger.LogInformation("Inventory loaded sucessfully: {InventoryCount}", _inventory.Count);
        }
        private void SetInventory()
        {
            if (_ErrorCount == 0)
            {
                //Load the inventory to the Logistic Operator
                foreach (var kvp in _inventory)
                {
                    var product = kvp.Key;

                    if (product != null)
                    {
                        //Fix Insert
                        string InventoryInsert = $"INSERT INTO LogisticInventory (ItemCode, ItemName, OnHand, UnitPrice) VALUES('{product.ItemCode}', '{product.ItemName}', {product.OnHand}, {product.UnitPrice}')";
                        _LOdbConnection.Query<Product>(InventoryInsert);
                    }
                }
                _logger.LogInformation("Inventory imported sucessfully: {InventoryCount}", _inventory.Count);
            }
            else
            {
                _logger.LogInformation("Couldn't import inventory, check logging.");
            }

        }
        private void GetBatches()
        {
            //Select to get all the batches in SAP
            string BatchQuery = "SELECT ItemCode, BatchCode, PrdDate, ExpDate, Quantity FROM OIBT";
            var batches = _SAPdbConnection.Query<Batch>(BatchQuery);
            foreach (Batch batch in batches)
            {
                if (batch != null)
                {
                    _batches[batch] = true;
                }
            }
            _logger.LogInformation("Batches loaded sucessfully: {Batches}", _batches.Count);
        }
        private void ValidateBatches()
        {

            foreach (var kvp in _batches)
            {
                var batch = kvp.Key;

                var ItemCode = batch.ItemCode;
                var BatchCode = batch.BatchCode;
                var ExpDate = batch.ExpDate;
                var Quantity = batch.Quantity;

                //Validate if the ItemCode assigned to a batch exists.
                if (!_inventory.Keys.Any(product => product.ItemCode == ItemCode))
                {
                    batch.IsBlocked = true;
                    batch.BlockReason = "Invalid batch: ItemCode does not exist in inventory.";
                    _ErrorCount++;
                    _logger.LogInformation("Batch {BatchCode} blocked: Item {ItemCode} does not exist in inventory.", BatchCode, ItemCode);
                }

                //Validate if the batch have an product left.
                if (Quantity == 0)
                {
                    batch.IsBlocked = true;
                    batch.BlockReason = "Empty batch: No product left.";
                    _ErrorCount++;
                    _logger.LogInformation("Batch {BatchCode} blocked: No product left.", BatchCode);
                }

                //Validate if the products on the batch are expired.
                if (ExpDate < DateTime.Today)
                {
                    batch.IsBlocked = true;
                    batch.BlockReason = "Expired batch: Products are expired.";
                    _ErrorCount++;
                    _logger.LogInformation("Batch {BatchCode} blocked: Expired on {ExpDate}.", BatchCode, ExpDate);
                }
            }
        }
        private void SetBatches()
        {
            //Load all the batches to the Logistic Operator
            foreach (var kvp in _batches)
            {
                var batch = kvp.Key;
                if (_batches != null)
                {
                    string BatchInsert = $"INSERT INTO LogisticBatches (ItemCode, BatchCode, PrdDate, ExpDate, Quantity) VALUES('{batch.ItemCode}', '{batch.BatchCode}', '{batch.PrdDate}', '{batch.ExpDate}', {batch.Quantity})";

                    _LOdbConnection.Query<Product>(BatchInsert);
                }
            }
            _logger.LogInformation("Inventory imported sucessfully: {InventoryCount}", _inventory.Count);
        }
    }
}
