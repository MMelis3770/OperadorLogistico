using OperadorLogistico.Models;

namespace Console.Communication.FileProcessing
{
    public class OrderFileReader
    {
        private readonly string _inputDirectory;

        public OrderFileReader(string inputDirectory)
        {
            _inputDirectory = inputDirectory;
        }

        public async Task<List<Order>> ReadPendingOrdersAsync()
        {
            var orders = new List<Order>();
            var files = Directory.GetFiles(_inputDirectory, "*.txt");

            foreach (var file in files)
            {
                try
                {
                    var order = await ParseOrderFileAsync(file);
                    if (order != null)
                    {
                        orders.Add(order);
                        // Opcionalmente, mover el archivo a una carpeta de procesados
                        MoveToProcessedFolder(file);
                    }
                }
                catch (Exception ex)
                {
                    // Loguear el error
                    System.Console.WriteLine($"Error al procesar el archivo {file}: {ex.Message}");
                    // Opcionalmente, mover el archivo a una carpeta de errores
                    MoveToErrorFolder(file);
                }
            }

            return orders;
        }

        private async Task<Order> ParseOrderFileAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length < 2)
                return null;

            // Ejemplo básico de formato: 
            // [0]: Cabecera: OrderNumber|CustomerCode|Date|DeliveryAddress
            // [1..n]: Líneas: LineNumber|ProductCode|Quantity

            var headerParts = lines[0].Split('|');
            if (headerParts.Length < 4)
                return null;

            var order = new Order
            {
                OrderNumber = headerParts[0],
                Customer = headerParts[1],
                OrderDate = DateTime.Parse(headerParts[2]),
                DeliveryAddress = headerParts[3],
                Status = OrderStatus.Received,
                Lines = new List<OrderLine>()
            };

            for (int i = 1; i < lines.Length; i++)
            {
                var lineParts = lines[i].Split('|');
                if (lineParts.Length < 3)
                    continue;

                var orderLine = new OrderLine
                {
                    LineNumber = int.Parse(lineParts[0]),
                    OrderNumber = order.OrderNumber,
                    ProductCode = lineParts[1],
                    RequestedQuantity = decimal.Parse(lineParts[2]),
                    Status = OrderLineStatus.Pending
                };

                order.Lines.Add(orderLine);
            }

            return order;
        }

        private void MoveToProcessedFolder(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var processedFolder = Path.Combine(_inputDirectory, "Processed");
            Directory.CreateDirectory(processedFolder);

            File.Move(filePath, Path.Combine(processedFolder, fileName), true);
        }

        private void MoveToErrorFolder(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var errorFolder = Path.Combine(_inputDirectory, "Error");
            Directory.CreateDirectory(errorFolder);

            File.Move(filePath, Path.Combine(errorFolder, fileName), true);
        }
    }
}
