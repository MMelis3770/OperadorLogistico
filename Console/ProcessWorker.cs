using Console.Processes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OperadorLogistico.Console
{
    public class ProcessWorker : BackgroundService
    {
        private readonly InventorySync _inventorySync;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<ProcessWorker> _logger;

        public ProcessWorker(IHostApplicationLifetime hostApplicationLifetime,
            InventorySync inventorySync,
            ILogger<ProcessWorker> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _inventorySync = inventorySync;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
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