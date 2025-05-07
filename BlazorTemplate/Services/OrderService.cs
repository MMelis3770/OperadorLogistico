using BlazorTemplate.Interfaces;
using BlazorTemplate.Models;
using Microsoft.Extensions.Logging;
namespace BlazorTemplate.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly string _orderFilesPath;
        public OrderService(ILogger<OrderService> logger, string orderFilesPath)
        {
            _logger = logger;
            _orderFilesPath = orderFilesPath;
        }
        public async Task<List<OrderData>> GetOrdersAsync()
        {
            try
            {
                // Process files in the specified directory
                var processor = new OrderFileProcessor(_orderFilesPath);
                var orders = processor.ProcessFiles();
                _logger.LogInformation($"Retrieved {orders.Count} orders from files");
                return orders;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders from files");
                throw;
            }
        }
        public async Task<bool> ConfirmOrderToSAP(int orderId)
        {
            try
            {
                _logger.LogInformation($"Confirming order {orderId} to SAP");
                // Then send confirmation but for now simulate a successful confirmation
                await Task.Delay(500); // Simulate network delay
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming order {orderId} to SAP");
                return false;
            }
        }
    }
}