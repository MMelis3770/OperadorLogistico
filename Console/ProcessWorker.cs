using Console.Processes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
//using OperadorLogistico.Console.Processes;

namespace OperadorLogistico.Console
{
    public class ProcessWorker : BackgroundService
    {
        //private readonly OrderManagement _orderManagement;
        private readonly InventorySync _inventorySync;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<ProcessWorker> _logger;

        public ProcessWorker(IHostApplicationLifetime hostApplicationLifetime,
            //OrderManagement orderManagement,
            InventorySync inventorySync,
            ILogger<ProcessWorker> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            //_orderManagement = orderManagement;
            _inventorySync = inventorySync;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                //await Task.Run(() => _orderManagement.Execute());
                await Task.Run(() => _inventorySync.Execute());
                _logger.LogInformation("Fin del proceso.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error no controlado {ErrorMsg}", ex.Message);
            }

            _hostApplicationLifetime.StopApplication();
        }
    }
}