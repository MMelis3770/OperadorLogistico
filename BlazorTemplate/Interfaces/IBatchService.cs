using BlazorTemplate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using static BlazorTemplate.Components.Dialog.BatchAssignmentDialog;

namespace BlazorTemplate.Interfaces
{
    public interface IBatchService
    {
        // Obtener todos los batches disponibles
        Task<List<Batch>> GetAvailableBatchesAsync();

        // Obtener batches disponibles para un artículo específico
        Task<List<Batch>> GetAvailableBatchesForItemAsync(string itemCode);

        // Asignar un batch a una línea de orden (todo el lote)
        Task<bool> AssignBatchToOrderLineAsync(int orderId, int lineNumber, string batchId);

        // Obtener el batch asignado a una línea de orden (para compatibilidad)
        Task<string> GetAssignedBatchForOrderLineAsync(int orderId, int lineNumber);

        // Obtener todas las asignaciones de batch para una línea de orden
        Task<List<BatchAssignment>> GetBatchAssignmentsForOrderLineAsync(int orderId, int lineNumber);

        // Guardar asignaciones de batch para una línea de orden
        Task<bool> SaveBatchAssignmentsForOrderLineAsync(int orderId, int lineNumber, List<BatchAssignment> assignments);

        // Verificar si todas las líneas de una orden tienen batches asignados
        Task<bool> AllOrderLinesHaveBatchesAsync(int orderId);

        // Asignar automáticamente batches a líneas sin asignar
        Task<bool> AutoAssignBatchesToOrderAsync(int orderId);

        // Inicializar datos de ejemplo para pruebas
        void InitializeTestData();
    }
}