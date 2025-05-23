using BlazorTemplate.Interfaces;
using BlazorTemplate.Models;
using BlazorTemplate.Processes;
using static BlazorTemplate.Components.Dialog.BatchAssignmentDialog;

namespace BlazorTemplate.Services
{
    public class BatchService : IBatchService
    {
        private readonly ISelectedOrdersService _selectedOrdersService;
        private readonly SQLManagement _sqlManagement;
        private Dictionary<(int OrderId, int LineNumber), List<BatchAssignment>> _batchAssignments;

        public BatchService(ISelectedOrdersService selectedOrdersService, SQLManagement sqlManagement)
        {
            _selectedOrdersService = selectedOrdersService;
            _sqlManagement = sqlManagement;
            _batchAssignments = new Dictionary<(int OrderId, int LineNumber), List<BatchAssignment>>();
        }

        // Implement the InitializeTestData() method even though we don't use it
        public void InitializeTestData()
        {
        }

        public async Task<List<Batch>> GetAvailableBatchesAsync()
        {
            // Get all batches from the database
            var batches = await _sqlManagement.GetAllBatchesAsync();

            // Filter only active batches
            var result = batches.Where(b => b.IsActive).ToList();
            return result;
        }

        public async Task<List<Batch>> GetAvailableBatchesForItemAsync(string itemCode)
        {
            // Get specific batches for an ItemCode from the database
            var batches = await _sqlManagement.GetBatchesForItemAsync(itemCode);

            // Filter only active batches with available quantity
            var result = batches
                .Where(b => b.IsActive && b.AvailableQuantity > 0)
                .ToList();

            return result;
        }

        // Method to update the processing status of an order
        public async Task<bool> UpdateOrderProcessedStatusAsync(int docEntry)
        {
            try
            {
                // Call the corresponding method in SQLManagement
                return await _sqlManagement.UpdateOrderProcessedStatusAsync(docEntry);
            }
            catch (Exception ex)
            {
                // Log error and re-throw
                Console.WriteLine($"Error in BatchService.UpdateOrderProcessedStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateOrderErrorStatusAsync(int docEntry, string errorMessage)
        {
            try
            {
                // Call the corresponding method in SQLManagement
                return await _sqlManagement.UpdateOrderErrorStatusAsync(docEntry, errorMessage);
            }
            catch (Exception ex)
            {
                // Log error and re-throw
                Console.WriteLine($"Error in BatchService.UpdateOrderErrorStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AssignBatchToOrderLineAsync(int orderId, int lineNumber, string batchId)
        {
            // Verify that the batch exists in the database
            var batches = await _sqlManagement.GetBatchesForItemAsync(null); // Get all
            var batch = batches.FirstOrDefault(b => b.BatchId == batchId);
            if (batch == null)
            {
                return false;
            }

            // Verify that the order and line exist
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order == null)
            {
                return false;
            }

            var line = order.LineItems.FirstOrDefault(l => l.LineNum == lineNumber);
            if (line == null)
            {
                return false;
            }

            // Create an assignment for the entire quantity
            var assignment = new BatchAssignment
            {
                BatchId = batchId,
                Quantity = line.Quantity
            };

            // Save the assignment
            var key = (orderId, lineNumber);
            if (_batchAssignments.ContainsKey(key))
            {
                _batchAssignments[key] = new List<BatchAssignment> { assignment };
            }
            else
            {
                _batchAssignments.Add(key, new List<BatchAssignment> { assignment });
            }

            // Update the order line with the assigned batch
            line.Batch = batchId;

            // Simulate process delay
            await Task.Delay(100);
            return true;
        }

        public async Task<List<AssignedBatch>> GetAssignedBatchesFromDatabaseAsync(int docEntry)
        {
            try
            {
                // Call the corresponding method in SQLManagement
                return await _sqlManagement.GetAssignedBatchesForOrderAsync(docEntry);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting assigned batches: {ex.Message}");
                return new List<AssignedBatch>();
            }
        }

        public async Task<string> GetAssignedBatchForOrderLineAsync(int orderId, int lineNumber)
        {
            await Task.Delay(50);
            var key = (orderId, lineNumber);

            // If there are partial assignments, return the first batch (for compatibility)
            if (_batchAssignments.TryGetValue(key, out var assignments) && assignments.Any())
            {
                var batchId = assignments.First().BatchId;
                return batchId;
            }

            // If not in our dictionary, check directly in the order
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order != null)
            {
                var line = order.LineItems.FirstOrDefault(l => l.LineNum == lineNumber);
                if (line != null && !string.IsNullOrEmpty(line.Batch))
                {
                    // Create a default assignment for the entire quantity
                    var assignment = new BatchAssignment
                    {
                        BatchId = line.Batch,
                        Quantity = line.Quantity
                    };

                    // Synchronize our dictionary
                    if (!_batchAssignments.ContainsKey(key))
                    {
                        _batchAssignments.Add(key, new List<BatchAssignment> { assignment });
                    }
                    return line.Batch;
                }
            }

            return string.Empty;
        }

        public async Task<List<BatchAssignment>> GetBatchAssignmentsForOrderLineAsync(int orderId, int lineNumber)
        {
            await Task.Delay(50);
            var key = (orderId, lineNumber);

            // If there are partial assignments, return them
            if (_batchAssignments.TryGetValue(key, out var assignments) && assignments.Any())
            {
                return assignments;
            }

            // If not in our dictionary, check directly in the order
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order != null)
            {
                var line = order.LineItems.FirstOrDefault(l => l.LineNum == lineNumber);
                if (line != null && !string.IsNullOrEmpty(line.Batch))
                {
                    // Create a default assignment for the entire quantity
                    var assignment = new BatchAssignment
                    {
                        BatchId = line.Batch,
                        Quantity = line.Quantity
                    };

                    return new List<BatchAssignment> { assignment };
                }
            }

            return new List<BatchAssignment>();
        }

        public async Task<bool> SaveBatchAssignmentsForOrderLineAsync(int orderId, int lineNumber, List<BatchAssignment> assignments)
        {
            // The case of empty assignments is now acceptable
            if (assignments == null)
            {
                assignments = new List<BatchAssignment>(); // Convert to empty list instead of failing
            }

            // Verify that the order and line exist
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order == null)
            {
                return false;
            }

            var line = order.LineItems.FirstOrDefault(l => l.LineNum == lineNumber);
            if (line == null)
            {
                return false;
            }

            //// Only verify quantities if there are assignments
            //if (assignments.Any())
            //{
            //    // Verify that all batches exist in the database
            //    var allBatches = await GetAvailableBatchesAsync();

            //    foreach (var assignment in assignments)
            //    {
            //        var batch = allBatches.FirstOrDefault(b => b.BatchId == assignment.BatchId);
            //        if (batch == null)
            //        {
            //            return false;
            //        }

            //        // Verify available quantity (taking into account current assignments)
            //        int assignedToOthers = GetTotalAssignedToBatch(batch.BatchId, orderId, lineNumber);
            //        if (batch.AvailableQuantity + assignedToOthers < assignment.Quantity)
            //        {
            //            return false;
            //        }
            //    }
            //}

            // Save the assignments
            var key = (orderId, lineNumber);
            if (_batchAssignments.ContainsKey(key))
            {
                _batchAssignments[key] = new List<BatchAssignment>(assignments);
            }
            else
            {
                _batchAssignments.Add(key, new List<BatchAssignment>(assignments));
            }

            // Update the order line with a summary of assigned batches
            if (assignments.Count == 0)
            {
                // If there are no assignments, clear the batch field
                line.Batch = string.Empty;
            }
            else if (assignments.Count == 1)
            {
                // If there's only one batch, use that as the main reference
                line.Batch = assignments[0].BatchId;
            }
            else
            {
                // If there are multiple batches, use a combined reference
                line.Batch = string.Join(", ", assignments.Select(a => $"{a.BatchId} ({a.Quantity})"));
            }

            // Simulate process delay
            await Task.Delay(100);
            return true;
        }

        public async Task<string> GetOrderErrorMessageAsync(int docEntry)
        {
            try
            {
                // Call the corresponding method in SQLManagement
                return await _sqlManagement.GetOrderErrorMessageAsync(docEntry);
            }
            catch (Exception ex)
            {
                // Log error and re-throw
                Console.WriteLine($"Error in BatchService.GetOrderErrorMessageAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AllOrderLinesHaveBatchesAsync(int orderId)
        {
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order == null)
            {
                return true; // No order, no problem
            }

            // Verify each line
            foreach (var line in order.LineItems)
            {
                var assignments = await GetBatchAssignmentsForOrderLineAsync(orderId, line.LineNum);
                int totalAssigned = assignments.Sum(a => a.Quantity);
                if (totalAssigned != line.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public async Task<(bool HasSufficientStock, int AvailableQuantity)> ValidateStockAvailabilityAsync(string itemCode, int requiredQuantity)
        {
            try
            {
                // Get all available batches for this item
                var batches = await GetAvailableBatchesForItemAsync(itemCode);

                // Calculate total available quantity
                int totalAvailable = batches.Sum(b => b.AvailableQuantity);

                // Check if there's sufficient stock
                bool hasSufficientStock = totalAvailable >= requiredQuantity;

                return (hasSufficientStock, totalAvailable);
            }
            catch (Exception)
            {
                // In case of error, assume there's not enough stock
                return (false, 0);
            }
        }

        public async Task<bool> SaveAssignedBatchesToDatabaseAsync(int docEntry, List<LineItem> lineItems)
        {
            try
            {
                // Simply call the method in SQLManagement
                return await _sqlManagement.SaveAssignedBatchesAsync(docEntry, lineItems);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving assigned batches: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AutoAssignBatchesToOrderAsync(int orderId)
        {
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order == null)
            {
                return false;
            }

            bool allAssigned = true;

            foreach (var line in order.LineItems)
            {
                var (hasSufficientStock, availableQuantity) = await ValidateStockAvailabilityAsync(line.ItemCode, line.Quantity);

                if (!hasSufficientStock)
                {
                    // If there's not enough stock, mark as not assigned and continue with the next line
                    allAssigned = false;
                    continue;
                }

                // Check if there are already assignments
                var assignments = await GetBatchAssignmentsForOrderLineAsync(orderId, line.LineNum);
                int totalAssigned = assignments.Sum(a => a.Quantity);
                int pendingQuantity = line.Quantity - totalAssigned;

                if (pendingQuantity <= 0)
                {
                    continue;
                }

                // Look for available batches for this item from the database
                var batches = await GetAvailableBatchesForItemAsync(line.ItemCode);
                if (batches.Any())
                {
                    // Sort batches by expiration date (FEFO)
                    batches = batches.OrderBy(b => b.EndDate).ToList();

                    // Create assignments to cover the pending quantity
                    List<BatchAssignment> newAssignments = new List<BatchAssignment>(assignments);
                    int remainingQuantity = pendingQuantity;

                    foreach (var batch in batches)
                    {
                        if (remainingQuantity <= 0)
                            break;

                        int quantityToAssign = Math.Min(remainingQuantity, batch.AvailableQuantity);
                        if (quantityToAssign <= 0)
                            continue;

                        newAssignments.Add(new BatchAssignment
                        {
                            BatchId = batch.BatchId,
                            Quantity = quantityToAssign
                        });

                        remainingQuantity -= quantityToAssign;
                    }

                    if (remainingQuantity <= 0)
                    {
                        // Save the assignments
                        bool success = await SaveBatchAssignmentsForOrderLineAsync(orderId, line.LineNum, newAssignments);
                        if (!success)
                        {
                            allAssigned = false;
                        }
                    }
                    else
                    {
                        allAssigned = false;
                    }
                }
                else
                {
                    // No available batches
                    allAssigned = false;
                }
            }

            return allAssigned;
        }
    }
}