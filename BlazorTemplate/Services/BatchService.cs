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

        // Implementar el método InitializeTestData() aunque no lo usemos
        public void InitializeTestData()
        {
        }

        public async Task<List<Batch>> GetAvailableBatchesAsync()
        {
            // Obtenir tots els batches de la base de dades
            var batches = await _sqlManagement.GetAllBatchesAsync();

            // Filtrar només els batches actius
            var result = batches.Where(b => b.IsActive).ToList();
            return result;
        }

        public async Task<List<Batch>> GetAvailableBatchesForItemAsync(string itemCode)
        {
            // Obtenir batches específics per a un ItemCode des de la base de dades
            var batches = await _sqlManagement.GetBatchesForItemAsync(itemCode);

            // Filtrar només els batches actius amb quantitat disponible
            var result = batches
                .Where(b => b.IsActive && b.AvailableQuantity > 0)
                .ToList();

            return result;
        }

        // Método para actualizar el estado de procesamiento de una orden
        public async Task<bool> UpdateOrderProcessedStatusAsync(int docEntry)
        {
            try
            {
                // Llama al método correspondiente en SQLManagement
                return await _sqlManagement.UpdateOrderProcessedStatusAsync(docEntry);
            }
            catch (Exception ex)
            {
                // Log del error y re-lanzamiento
                Console.WriteLine($"Error en BatchService.UpdateOrderProcessedStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> UpdateOrderErrorStatusAsync(int docEntry, string errorMessage)
        {
            try
            {
                // Llama al método correspondiente en SQLManagement
                return await _sqlManagement.UpdateOrderErrorStatusAsync(docEntry, errorMessage);
            }
            catch (Exception ex)
            {
                // Log del error y re-lanzamiento
                Console.WriteLine($"Error en BatchService.UpdateOrderErrorStatusAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AssignBatchToOrderLineAsync(int orderId, int lineNumber, string batchId)
        {
            // Verificar que el batch existe en la base de datos
            var batches = await _sqlManagement.GetBatchesForItemAsync(null); // Obtener todos
            var batch = batches.FirstOrDefault(b => b.BatchId == batchId);
            if (batch == null)
            {
                return false;
            }

            // Verificar que la orden y línea existen
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

            // Crear una asignación para toda la cantidad
            var assignment = new BatchAssignment
            {
                BatchId = batchId,
                Quantity = line.Quantity
            };

            // Guardar la asignación
            var key = (orderId, lineNumber);
            if (_batchAssignments.ContainsKey(key))
            {
                _batchAssignments[key] = new List<BatchAssignment> { assignment };
            }
            else
            {
                _batchAssignments.Add(key, new List<BatchAssignment> { assignment });
            }

            // Actualizar la línea de orden con el batch asignado
            line.Batch = batchId;

            // Simular delay de proceso
            await Task.Delay(100);
            return true;
        }

        public async Task<string> GetAssignedBatchForOrderLineAsync(int orderId, int lineNumber)
        {
            await Task.Delay(50);
            var key = (orderId, lineNumber);

            // Si hay asignaciones parciales, devolver el primer batch (para compatibilidad)
            if (_batchAssignments.TryGetValue(key, out var assignments) && assignments.Any())
            {
                var batchId = assignments.First().BatchId;
                return batchId;
            }

            // Si no está en nuestro diccionario, verificar en la orden directamente
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order != null)
            {
                var line = order.LineItems.FirstOrDefault(l => l.LineNum == lineNumber);
                if (line != null && !string.IsNullOrEmpty(line.Batch))
                {
                    // Crear una asignación por defecto para toda la cantidad
                    var assignment = new BatchAssignment
                    {
                        BatchId = line.Batch,
                        Quantity = line.Quantity
                    };

                    // Sincronizar nuestro diccionario
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

            // Si hay asignaciones parciales, devolverlas
            if (_batchAssignments.TryGetValue(key, out var assignments) && assignments.Any())
            {
                return assignments;
            }

            // Si no está en nuestro diccionario, verificar en la orden directamente
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order != null)
            {
                var line = order.LineItems.FirstOrDefault(l => l.LineNum == lineNumber);
                if (line != null && !string.IsNullOrEmpty(line.Batch))
                {
                    // Crear una asignación por defecto para toda la cantidad
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
            // El caso de asignaciones vacías ahora es aceptable
            if (assignments == null)
            {
                assignments = new List<BatchAssignment>(); // Convertir a lista vacía en lugar de fallar
            }

            // Verificar que la orden y línea existen
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

            //// Solo verificar las cantidades si hay asignaciones
            //if (assignments.Any())
            //{
            //    // Verificar que todos los batches existen en la base de datos
            //    var allBatches = await GetAvailableBatchesAsync();

            //    foreach (var assignment in assignments)
            //    {
            //        var batch = allBatches.FirstOrDefault(b => b.BatchId == assignment.BatchId);
            //        if (batch == null)
            //        {
            //            return false;
            //        }

            //        // Verificar la cantidad disponible (teniendo en cuenta las asignaciones actuales)
            //        int assignedToOthers = GetTotalAssignedToBatch(batch.BatchId, orderId, lineNumber);
            //        if (batch.AvailableQuantity + assignedToOthers < assignment.Quantity)
            //        {
            //            return false;
            //        }
            //    }
            //}

            // Guardar las asignaciones
            var key = (orderId, lineNumber);
            if (_batchAssignments.ContainsKey(key))
            {
                _batchAssignments[key] = new List<BatchAssignment>(assignments);
            }
            else
            {
                _batchAssignments.Add(key, new List<BatchAssignment>(assignments));
            }

            // Actualizar la línea de orden con un resumen de los batches asignados
            if (assignments.Count == 0)
            {
                // Si no hay asignaciones, limpiar el campo de batch
                line.Batch = string.Empty;
            }
            else if (assignments.Count == 1)
            {
                // Si solo hay un batch, usamos ese como referencia principal
                line.Batch = assignments[0].BatchId;
            }
            else
            {
                // Si hay múltiples batches, usar una referencia combinada
                line.Batch = string.Join(", ", assignments.Select(a => $"{a.BatchId} ({a.Quantity})"));
            }

            // Simular delay de proceso
            await Task.Delay(100);
            return true;
        }

        public async Task<string> GetOrderErrorMessageAsync(int docEntry)
        {
            try
            {
                // Llama al método correspondiente en SQLManagement
                return await _sqlManagement.GetOrderErrorMessageAsync(docEntry);
            }
            catch (Exception ex)
            {
                // Log del error y re-lanzamiento
                Console.WriteLine($"Error en BatchService.GetOrderErrorMessageAsync: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> AllOrderLinesHaveBatchesAsync(int orderId)
        {
            var orders = _selectedOrdersService.SelectedOrders;
            var order = orders.FirstOrDefault(o => o.DocEntry == orderId);
            if (order == null)
            {
                return true; // No hay orden, no hay problema
            }

            // Verificar cada línea
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
                // Obtenir tots els batches disponibles per a aquest article
                var batches = await GetAvailableBatchesForItemAsync(itemCode);

                // Calcular la quantitat total disponible
                int totalAvailable = batches.Sum(b => b.AvailableQuantity);

                // Verificar si hi ha suficient stock
                bool hasSufficientStock = totalAvailable >= requiredQuantity;

                return (hasSufficientStock, totalAvailable);
            }
            catch (Exception)
            {
                // En cas d'error, assumim que no hi ha stock suficient
                return (false, 0);
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
                    // Si no hi ha suficient stock, marcar com a no assignat i continuar amb la següent línia
                    allAssigned = false;
                    continue;
                }

                // Verificar si ja hay asignaciones
                var assignments = await GetBatchAssignmentsForOrderLineAsync(orderId, line.LineNum);
                int totalAssigned = assignments.Sum(a => a.Quantity);
                int pendingQuantity = line.Quantity - totalAssigned;

                if (pendingQuantity <= 0)
                {
                    continue;
                }

                // Buscar batches disponibles para este artículo desde la base de datos
                var batches = await GetAvailableBatchesForItemAsync(line.ItemCode);
                if (batches.Any())
                {
                    // Ordenar los batches por fecha de vencimiento (FEFO)
                    batches = batches.OrderBy(b => b.EndDate).ToList();

                    // Crear asignaciones para cubrir la cantidad pendiente
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
                        // Guardar las asignaciones
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
                    // No hay batches disponibles
                    allAssigned = false;
                }
            }

            return allAssigned;
        }
    }
}