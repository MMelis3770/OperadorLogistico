using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OperadorLogistico.Console;

namespace Console.Processes
{
    public class InventorySync
    {
        private readonly ISqlServerConnection _dbConnection;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InventorySync> _logger;
        private readonly string _sapDbName;
        private readonly string _logisticDbName;

        public InventorySync(
            IConfiguration configuration,
            ISqlServerConnection dbConnection,
            ILogger<InventorySync> logger)
        {
            _configuration = configuration;
            _dbConnection = dbConnection;
            _logger = logger;
            _sapDbName = _dbConnection.GetSapDbName();
            _logisticDbName = _dbConnection.GetLogisticDbName();
        }

        public void Execute()
        {
            try
            {
                SyncClients();
                SyncInventory();
                SyncBatches();
            }
            catch (Exception ex)
            {
                _logger.LogError("Error executing synchronization: {ErrorMsg}", ex.Message);
            }
        }

        private void SyncClients()
        {
            try
            {
                _logger.LogInformation("Starting client synchronization...");

                // DELETE - Eliminar només els clients que ja no existeixen a SAP
                string deleteClientsQuery = $@"
                    DELETE FROM {_logisticDbName}.dbo.Client
                    WHERE CardCode IN (
                        SELECT CardCode
                        FROM {_logisticDbName}.dbo.Client
                        EXCEPT
                        SELECT o.CardCode COLLATE SQL_Latin1_General_CP1_CI_AS
                        FROM {_sapDbName}.dbo.OCRD o
                    )";

                _dbConnection.Query<object>(deleteClientsQuery);
                _logger.LogInformation("Clients deleted successfully");

                // UPDATE - Actualitzar els clients existents
                string updateClientsQuery = $@"
                    UPDATE i
                    SET i.CardName = s.CardName
                    FROM {_logisticDbName}.dbo.Client i
                    INNER JOIN {_sapDbName}.dbo.OCRD s ON i.CardCode COLLATE SQL_Latin1_General_CP1_CI_AS = s.CardCode
                    WHERE i.CardName COLLATE SQL_Latin1_General_CP1_CI_AS <> s.CardName";

                _dbConnection.Query<object>(updateClientsQuery);
                _logger.LogInformation("Clients updated successfully");

                // INSERT - Inserir només els nous clients
                string insertClientsQuery = $@"
                    INSERT INTO {_logisticDbName}.dbo.Client (CardCode, CardName)
                    SELECT CardCode, CardName
                    FROM (
                        SELECT CardCode COLLATE SQL_Latin1_General_CP1_CI_AS AS CardCode, 
                               CardName COLLATE SQL_Latin1_General_CP1_CI_AS AS CardName
                        FROM {_sapDbName}.dbo.OCRD
                        EXCEPT
                        SELECT CardCode, CardName
                        FROM {_logisticDbName}.dbo.Client
                    ) AS ClientsToInsert";

                _dbConnection.Query<object>(insertClientsQuery);
                _logger.LogInformation("Clients inserted successfully");

                _logger.LogInformation("Client synchronization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error synchronizing clients: {ErrorMsg}", ex.Message);
            }
        }

        private void SyncInventory()
        {
            try
            {
                _logger.LogInformation("Starting inventory synchronization...");

                // DELETE - Eliminar només l'inventari que ja no existeix a SAP
                string deleteInventoryQuery = $@"
                    DELETE FROM {_logisticDbName}.dbo.Inventory
                    WHERE ItemCode IN (
                        SELECT ItemCode
                        FROM {_logisticDbName}.dbo.Inventory
                        EXCEPT
                        SELECT o.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS
                        FROM {_sapDbName}.dbo.OITM o
                        WHERE o.OnHand > 0
                        AND EXISTS (
                            SELECT 1 
                            FROM {_sapDbName}.dbo.OIBT b
                            WHERE b.ItemCode = o.ItemCode
                            AND b.Quantity > 0
                        )
                    )";

                _dbConnection.Query<object>(deleteInventoryQuery);
                _logger.LogInformation("Products deleted successfully");

                // UPDATE - Actualitzar l'inventari existent
                string updateInventoryQuery = $@"
                    UPDATE i
                    SET i.ItemName = s.ItemName,
                        i.OnHand = s.OnHand,
                        i.PriceUnit = s.PriceUnit
                    FROM {_logisticDbName}.dbo.Inventory i
                    INNER JOIN {_sapDbName}.dbo.OITM s ON i.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS = s.ItemCode
                    WHERE i.ItemName COLLATE SQL_Latin1_General_CP1_CI_AS <> s.ItemName
                       OR i.OnHand <> s.OnHand
                       OR i.PriceUnit <> s.PriceUnit";

                _dbConnection.Query<object>(updateInventoryQuery);
                _logger.LogInformation("Products updated successfully");

                // INSERT - Inserir només el nou inventari
                string insertInventoryQuery = $@"
                    INSERT INTO {_logisticDbName}.dbo.Inventory (ItemCode, ItemName, OnHand, PriceUnit)
                    SELECT o.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS AS ItemCode,
                        o.ItemName COLLATE SQL_Latin1_General_CP1_CI_AS AS ItemName,
                        o.OnHand,
                        o.PriceUnit
                    FROM {_sapDbName}.dbo.OITM o
                    WHERE o.OnHand > 0
                    AND EXISTS (
                        SELECT 1 
                        FROM {_sapDbName}.dbo.OIBT b
                        WHERE b.ItemCode = o.ItemCode
                        AND b.Quantity > 0
                    )
                    AND NOT EXISTS (
                        SELECT 1
                        FROM {_logisticDbName}.dbo.Inventory i
                        WHERE i.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS = o.ItemCode
                    )";

                _dbConnection.Query<object>(insertInventoryQuery);
                _logger.LogInformation("Products inserted successfully");

                _logger.LogInformation("Inventory synchronization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error synchronizing inventory: {ErrorMsg}", ex.Message);
            }
        }

        private void SyncBatches()
        {
            try
            {
                _logger.LogInformation("Starting batch synchronization...");

                // DELETE - Eliminar només els lots que ja no existeixen a SAP
                string deleteBatchesQuery = $@"
                    DELETE FROM {_logisticDbName}.dbo.Batches
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM {_sapDbName}.dbo.OIBT o
                        WHERE o.Quantity > 0
                        AND {_logisticDbName}.dbo.Batches.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS = o.ItemCode
                        AND {_logisticDbName}.dbo.Batches.BatchNum COLLATE SQL_Latin1_General_CP1_CI_AS = o.BatchNum
                    )";

                _dbConnection.Query<object>(deleteBatchesQuery);
                _logger.LogInformation("Batches deleted successfully");

                // UPDATE - Actualitzar els lots existents
                string updateBatchesQuery = $@"
                    UPDATE i
                    SET i.PrdDate = s.PrdDate,
                        i.ExpDate = s.ExpDate,
                        i.Quantity = s.Quantity
                    FROM {_logisticDbName}.dbo.Batches i
                    INNER JOIN {_sapDbName}.dbo.OIBT s 
                    ON i.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS = s.ItemCode 
                    AND i.BatchNum COLLATE SQL_Latin1_General_CP1_CI_AS = s.BatchNum
                    WHERE i.PrdDate <> s.PrdDate
                       OR i.ExpDate <> s.ExpDate
                       OR i.Quantity <> s.Quantity";

                _dbConnection.Query<object>(updateBatchesQuery);
                _logger.LogInformation("Batches updated successfully");

                // INSERT - Inserir només els nous lots
                string insertBatchesQuery = $@"
                    INSERT INTO {_logisticDbName}.dbo.Batches (ItemCode, BatchNum, PrdDate, ExpDate, Quantity)
                    SELECT DISTINCT
                        b.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS AS ItemCode, 
                        b.BatchNum COLLATE SQL_Latin1_General_CP1_CI_AS AS BatchNum,
                        b.PrdDate, 
                        b.ExpDate, 
                        b.Quantity
                    FROM {_sapDbName}.dbo.OIBT b
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM {_logisticDbName}.dbo.Batches lb
                        WHERE lb.ItemCode COLLATE SQL_Latin1_General_CP1_CI_AS = b.ItemCode
                        AND lb.BatchNum COLLATE SQL_Latin1_General_CP1_CI_AS = b.BatchNum
                    )";

                _dbConnection.Query<object>(insertBatchesQuery);
                _logger.LogInformation("Batches inserted successfully");

                _logger.LogInformation("Batch synchronization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error synchronizing batches: {ErrorMsg}", ex.Message);
            }
        }
    }
}