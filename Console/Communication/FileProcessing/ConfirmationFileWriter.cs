using System.Text;
using OperadorLogistico.Console.Models;

namespace OperadorLogistico.Console.Communication.FileProcessing
{
    public class ConfirmationFileWriter
    {
        private readonly string _outputDirectory;

        public ConfirmationFileWriter(string outputDirectory)
        {
            _outputDirectory = outputDirectory;
            Directory.CreateDirectory(_outputDirectory);
        }

        public async Task WriteConfirmationAsync(Order order)
        {
            var fileName = $"CONF_{order.OrderNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            var filePath = Path.Combine(_outputDirectory, fileName);

            var sb = new StringBuilder();

            // Escribir cabecera
            sb.AppendLine($"{order.OrderNumber}|{order.Customer}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}|{order.Status}");

            // Escribir líneas con lotes asignados
            foreach (var line in order.Lines)
            {
                //sb.AppendLine($"{line.LineNumber}|{line.ProductCode}|{line.RequestedQuantity}|{line.AssignedQuantity}|{line.Status}");

                // Escribir los lotes asignados a esta línea
                foreach (var batch in line.AssignedBatches)
                {
                    sb.AppendLine($"BATCH|{batch.Key}|{batch.Value}");
                }
            }

            await File.WriteAllTextAsync(filePath, sb.ToString());
        }
    }
}
