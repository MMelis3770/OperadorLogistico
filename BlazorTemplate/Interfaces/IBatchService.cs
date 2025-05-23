using BlazorTemplate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BlazorTemplate.Components.Dialog.BatchAssignmentDialog;

namespace BlazorTemplate.Interfaces
{
    public interface IBatchService
    {
        // Get all available batches
        Task<List<Batch>> GetAvailableBatchesAsync();

        // Get available batches for a specific item
        Task<List<Batch>> GetAvailableBatchesForItemAsync(string itemCode);

        // Assign a batch to an order line (entire batch)
        Task<bool> AssignBatchToOrderLineAsync(int orderId, int lineNumber, string batchId);

        // Get the assigned batch for an order line (for compatibility)
        Task<string> GetAssignedBatchForOrderLineAsync(int orderId, int lineNumber);

        // Get all batch assignments for an order line
        Task<List<BatchAssignment>> GetBatchAssignmentsForOrderLineAsync(int orderId, int lineNumber);

        // Save batch assignments for an order line
        Task<bool> SaveBatchAssignmentsForOrderLineAsync(int orderId, int lineNumber, List<BatchAssignment> assignments);

        // Check if all order lines have assigned batches
        Task<bool> AllOrderLinesHaveBatchesAsync(int orderId);

        // Automatically assign batches to unassigned lines
        Task<bool> AutoAssignBatchesToOrderAsync(int orderId);

        // Check if there is sufficient quantity for the line
        Task<(bool HasSufficientStock, int AvailableQuantity)> ValidateStockAvailabilityAsync(string itemCode, int requiredQuantity);

        // Method to update the processing status of an order
        Task<bool> UpdateOrderProcessedStatusAsync(int docEntry);
        Task<bool> UpdateOrderErrorStatusAsync(int docEntry, string errorMessage);
        Task<string> GetOrderErrorMessageAsync(int docEntry);

        // Method to save assigned batches in the AssignedBatches table
        Task<bool> SaveAssignedBatchesToDatabaseAsync(int docEntry, List<LineItem> lineItems);

        // Method to get assigned batches from the database
        Task<List<AssignedBatch>> GetAssignedBatchesFromDatabaseAsync(int docEntry);

        // Initialize sample data for testing
        void InitializeTestData();
    }
}