using B1SLayer;
using Console.Models;
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
        private readonly Dictionary<Product, bool> _inventoryLO;
        private readonly Dictionary<Product, bool> _inventoryToUpdate;
        private readonly Dictionary<Product, bool> _inventoryToDelete;

        private readonly Dictionary<Batch, bool> _batches;
        private readonly Dictionary<Batch, bool> _batchesLO;
        private readonly Dictionary<Batch, bool> _batchesToUpdate;
        private readonly Dictionary<Batch, bool> _batchesToDelete;

        private readonly Dictionary<Client, bool> _clients;
        private readonly Dictionary<Client, bool> _clientsLO;
        private readonly Dictionary<Client, bool> _clientsToUpdate;
        private readonly Dictionary<Client, bool> _clientsToDelete;

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

            // Inventory dictionaries
            _inventory = new Dictionary<Product, bool>();
            _inventoryLO = new Dictionary<Product, bool>();
            _inventoryToUpdate = new Dictionary<Product, bool>();
            _inventoryToDelete = new Dictionary<Product, bool>();

            // Batches dictionaries
            _batches = new Dictionary<Batch, bool>();
            _batchesLO = new Dictionary<Batch, bool>();
            _batchesToUpdate = new Dictionary<Batch, bool>();
            _batchesToDelete = new Dictionary<Batch, bool>();

            // Clients dictionaries
            _clients = new Dictionary<Client, bool>();
            _clientsLO = new Dictionary<Client, bool>();
            _clientsToUpdate = new Dictionary<Client, bool>();
            _clientsToDelete = new Dictionary<Client, bool>();

            _ErrorCount = 0;
        }

        public void Execute()
        {
            GetClientDifferences();
            if ( _clientsToUpdate.Count > 0 || _clientsToDelete.Count > 0)
            {
                SetClients();
            }
            GetInventoryDifferences();
            if (_inventoryToUpdate.Count > 0 || _inventoryToDelete.Count > 0)
            {
                SetInventory();
            }
            GetBatchDifferences();
            if (_batchesToUpdate.Count > 0 || _batchesToDelete.Count > 0)
            {
                SetBatches();
            }
        }

        private void GetClientDifferences()
        {
            GetClientsSAP();
            GetClientsLO();
            ProcessClientDifferences();
        }
        private void GetInventoryDifferences()
        {
            GetInventorySAP();
            GetInventoryLO();
            ProcessInventoryDifferences();
        }
        private void GetBatchDifferences()
        {
            GetBatchesSAP();
            GetBatchesLO();
            ProcessBatchDifferences();
        }
        private void GetClientsSAP()
        {
            try
            {
                string clientQuery = "SELECT CardCode, CardName FROM OCRD";
                var clients = _SAPdbConnection.Query<Client>(clientQuery);
                foreach (Client client in clients)
                {
                    if (client != null)
                    {
                        _clients[client] = true;
                        System.Console.WriteLine(client);
                    }
                }
                _logger.LogInformation("Clients loaded successfully: {ClientsCount}", _clients.Count);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to get clients from SAP. Error message: " + ex.Message);
            }
        }
        private void GetClientsLO()
        {
            try
            {
                string clientQuery = "SELECT CardCode, CardName FROM Client";
                var clients = _LOdbConnection.Query<Client>(clientQuery);
                foreach (Client client in clients)
                {
                    if (client != null)
                    {
                        _clientsLO[client] = true;
                        System.Console.WriteLine(client);
                    }
                }
                _logger.LogInformation("Clients loaded successfully: {ClientsCount}", _clientsLO.Count);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to get clients from Logistic Operator DB. Error message: " + ex.Message);
            }
        }
        private void ProcessClientDifferences()
        {
            _clientsToUpdate.Clear();
            _clientsToDelete.Clear();

            foreach (var sapClient in _clients.Keys)
            {
                var loClient = _clientsLO.Keys.FirstOrDefault(c => c.CardCode == sapClient.CardCode);
                if (loClient == null || !ClientsAreEqual(sapClient, loClient))
                {
                    _clientsToUpdate[sapClient] = true;
                }
            }

            foreach (var loClient in _clientsLO.Keys)
            {
                var sapClient = _clients.Keys.FirstOrDefault(c => c.CardCode == loClient.CardCode);
                if (sapClient == null)
                {
                    _clientsToDelete[loClient] = true;
                }
            }

            _logger.LogInformation("Clients to update/add: {Count}", _clientsToUpdate.Count);
            _logger.LogInformation("Clients to delete: {Count}", _clientsToDelete.Count);
        }
        private bool ClientsAreEqual(Client c1, Client c2)
        {
            return c1.CardCode == c2.CardCode && c1.CardName == c2.CardName;
        }
        private void SetClients()
        {

            try
            {
                if (_clientsToDelete.Count != 0)
                {
                    foreach (var kvp in _clientsToDelete)
                    {
                        string ClientDelete = $"DELETE FROM Client WHERE CardCode = {kvp.Key.CardCode})";
                        _LOdbConnection.Query<Client>(ClientDelete);
                    }
                }
                foreach (var kvp in _clientsToUpdate)
                {
                    if (_clientsLO.ContainsKey(kvp.Key))
                    {
                        string ClientUpdate = $"UPDATE Client SET CardName = {kvp.Key.CardName} WHERE CardCode = {kvp.Key.CardCode}";
                        _LOdbConnection.Query<Client>(ClientUpdate);
                    }
                    else
                    {
                        string ClientInsert = $"INSERT INTO Client (CardCode, CardName) VALUES('{kvp.Key.CardCode}', '{kvp.Key.CardName}')";
                        _LOdbConnection.Query<Client>(ClientInsert);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An error occurred while updating the database. Error message: {message}", ex.Message);
            }
        }
        private void GetInventorySAP()
        {
            try
            {
                string inventoryQuery = "SELECT ItemCode, ItemName, OnHand, PriceUnit FROM OITM WHERE OnHand > 0";
                var inventory = _SAPdbConnection.Query<Product>(inventoryQuery);
                foreach (Product product in inventory)
                {
                    if (product != null)
                    {
                        _inventory[product] = true;
                        System.Console.WriteLine(product);
                    }
                }
                _logger.LogInformation("Inventory loaded successfully: {InventoryCount}", _inventory.Count);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to get inventory from SAP. Error message: " + ex.Message);
            }
        }
        private void GetInventoryLO()
        {
            try
            {
                string inventoryQuery = "SELECT ItemCode, ItemName, OnHand, PriceUnit FROM Inventory WHERE OnHand > 0";
                var inventory = _LOdbConnection.Query<Product>(inventoryQuery);
                foreach (Product product in inventory)
                {
                    if (product != null)
                    {
                        _inventoryLO[product] = true;
                        System.Console.WriteLine(product);
                    }
                }
                _logger.LogInformation("Inventory loaded successfully: {InventoryCount}", _inventoryLO.Count);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to get inventory from Logistic Operator DB. Error message: " + ex.Message);
            }
        }
        private void ProcessInventoryDifferences()
        {
            _inventoryToUpdate.Clear();
            _inventoryToDelete.Clear();

            foreach (var sapProduct in _inventory.Keys)
            {
                var loProduct = _inventoryLO.Keys.FirstOrDefault(p => p.ItemCode == sapProduct.ItemCode);
                if (loProduct == null || !ProductsAreEqual(sapProduct, loProduct))
                {
                    _inventoryToUpdate[sapProduct] = true;
                }
            }

            foreach (var loProduct in _inventoryLO.Keys)
            {
                var sapProduct = _inventory.Keys.FirstOrDefault(p => p.ItemCode == loProduct.ItemCode);
                if (sapProduct == null)
                {
                    _inventoryToDelete[loProduct] = true;
                }
            }

            _logger.LogInformation("Products to update/add: {Count}", _inventoryToUpdate.Count);
            _logger.LogInformation("Products to delete: {Count}", _inventoryToDelete.Count);
        }
        private bool ProductsAreEqual(Product p1, Product p2)
        {
            return p1.ItemCode == p2.ItemCode &&
                   p1.ItemName == p2.ItemName &&
                   p1.OnHand == p2.OnHand &&
                   p1.PriceUnit == p2.PriceUnit;
        }
        private void SetInventory()
        {
            try
            {
                if (_inventoryToDelete.Count != 0)
                {
                    foreach (var kvp in _inventoryToDelete)
                    {
                        string InventoryDelete = $"DELETE FROM Inventory WHERE ItemCode = '{kvp.Key.ItemCode}'";
                        _LOdbConnection.Query<Product>(InventoryDelete);
                    }
                }
                foreach (var kvp in _inventoryToUpdate)
                {
                    if (_inventoryLO.ContainsKey(kvp.Key))
                    {
                        string InventoryUpdate = $"UPDATE Inventory SET ItemName = '{kvp.Key.ItemName}', OnHand = {kvp.Key.OnHand}, PriceUnit = {kvp.Key.PriceUnit} WHERE ItemCode = '{kvp.Key.ItemCode}'";
                        _LOdbConnection.Query<Product>(InventoryUpdate);
                    }
                    else
                    {
                        string InventoryInsert = $"INSERT INTO Inventory (ItemCode, ItemName, OnHand, PriceUnit) VALUES('{kvp.Key.ItemCode}', '{kvp.Key.ItemName}', {kvp.Key.OnHand}, {kvp.Key.PriceUnit})";
                        _LOdbConnection.Query<Product>(InventoryInsert);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An error occurred while updating the database. Error message: {message}", ex.Message);
            }

        }
        private void GetBatchesSAP()
        {
            try
            {
                string batchQuery = "SELECT ItemCode, BatchNum, PrdDate, ExpDate, Quantity, WhsCode FROM OIBT";
                var batches = _SAPdbConnection.Query<Batch>(batchQuery);
                foreach (Batch batch in batches)
                {
                    if (batch != null)
                    {
                        _batches[batch] = true;
                        System.Console.WriteLine(batch);
                    }
                }
                _logger.LogInformation("Batches loaded successfully: {Batches}", _batches.Count);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to get batches from SAP. Error message: " + ex.Message);
            }
        }
        private void GetBatchesLO()
        {
            try
            {
                string batchQuery = "SELECT ItemCode, BatchNum, PrdDate, ExpDate, Quantity FROM Batches";
                var batches = _LOdbConnection.Query<Batch>(batchQuery);
                foreach (Batch batch in batches)
                {
                    if (batch != null)
                    {
                        _batchesLO[batch] = true;
                        System.Console.WriteLine(batch);
                    }
                }
                _logger.LogInformation("Batches loaded successfully: {Batches}", _batchesLO.Count);
            }
            catch (Exception ex)
            {
                _logger.LogInformation("Failed to get batches from Logistic Operator DB. Error message: " + ex.Message);
            }
        }
        private void ProcessBatchDifferences()
        {
            _batchesToUpdate.Clear();
            _batchesToDelete.Clear();

            foreach (var sapBatch in _batches.Keys)
            {
                var loBatch = _batchesLO.Keys.FirstOrDefault(b =>
                    b.ItemCode == sapBatch.ItemCode &&
                    b.BatchNum == sapBatch.BatchNum);

                if (loBatch == null || !BatchesAreEqual(sapBatch, loBatch))
                {
                    _batchesToUpdate[sapBatch] = true;
                }
            }

            foreach (var loBatch in _batchesLO.Keys)
            {
                var sapBatch = _batches.Keys.FirstOrDefault(b =>
                    b.ItemCode == loBatch.ItemCode &&
                    b.BatchNum == loBatch.BatchNum);

                if (sapBatch == null)
                {
                    _batchesToDelete[loBatch] = true;
                }
            }

            _logger.LogInformation("Batches to update/add: {Count}", _batchesToUpdate.Count);
            _logger.LogInformation("Batches to delete: {Count}", _batchesToDelete.Count);
        }
        private bool BatchesAreEqual(Batch b1, Batch b2)
        {
            return b1.ItemCode == b2.ItemCode &&
                   b1.BatchNum == b2.BatchNum &&
                   b1.PrdDate == b2.PrdDate &&
                   b1.ExpDate == b2.ExpDate &&
                   b1.Quantity == b2.Quantity;
        }
        private void SetBatches()
        {
            try
            {
                if (_batchesToDelete.Count != 0)
                {
                    foreach (var kvp in _batchesToDelete)
                    {
                        string BatchDelete = $"DELETE FROM Batches WHERE ItemCode = '{kvp.Key.ItemCode}' AND BatchNum = '{kvp.Key.BatchNum}'";
                        _LOdbConnection.Query<Batch>(BatchDelete);
                    }
                }
                foreach (var kvp in _batchesToUpdate)
                {
                    if (_batchesLO.ContainsKey(kvp.Key))
                    {
                        string BatchUpdate = $"UPDATE Batches SET PrdDate = '{kvp.Key.PrdDate:yyyy-MM-dd}', ExpDate = '{kvp.Key.ExpDate:yyyy-MM-dd}', Quantity = {kvp.Key.Quantity} " +
                                             $"WHERE ItemCode = '{kvp.Key.ItemCode}' AND BatchNum = '{kvp.Key.BatchNum}'";
                        _LOdbConnection.Query<Batch>(BatchUpdate);
                    }
                    else
                    {
                        string BatchInsert = $"INSERT INTO Batches (ItemCode, BatchNum, PrdDate, ExpDate, Quantity) VALUES(" +
                                             $"'{kvp.Key.ItemCode}', '{kvp.Key.BatchNum}', '{kvp.Key.PrdDate:yyyy-MM-dd}', '{kvp.Key.ExpDate:yyyy-MM-dd}', {kvp.Key.Quantity})";
                        _LOdbConnection.Query<Batch>(BatchInsert);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation("An error occurred while updating the database. Error message: {message}", ex.Message);
            }

        }

    }
}
