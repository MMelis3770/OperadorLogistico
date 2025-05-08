using BlazorTemplate.Models;
using BlazorTemplate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BlazorTemplate.Components.Dialog.BatchAssignmentDialog;

namespace BlazorTemplate.Services
{
    public class BatchService : IBatchService
    {
        private readonly ISelectedOrdersService _selectedOrdersService;
        private List<Batch> _availableBatches;
        private Dictionary<(int OrderId, int LineNumber), List<BatchAssignment>> _batchAssignments;

        public BatchService(ISelectedOrdersService selectedOrdersService)
        {
            _selectedOrdersService = selectedOrdersService;
            _availableBatches = new List<Batch>();
            _batchAssignments = new Dictionary<(int OrderId, int LineNumber), List<BatchAssignment>>();

            // Inicializar datos de ejemplo
            InitializeTestData();
        }

        public void InitializeTestData()
        {
            // Limpiar datos existentes
            _availableBatches.Clear();
            _batchAssignments.Clear();

            // Fecha actual para referencia
            var today = DateTime.Today;

            // Crear lotes para el producto A00001 (3 lotes)
            _availableBatches.Add(new Batch("B1001", today.AddDays(-30), today.AddDays(30), "A00001", 100));
            _availableBatches.Add(new Batch("B1002", today.AddDays(-20), today.AddDays(40), "A00001", 150));
            _availableBatches.Add(new Batch("B1003", today.AddDays(-10), today.AddDays(50), "A00001", 200));

            // Crear lotes para el producto A00002 (3 lotes)
            _availableBatches.Add(new Batch("B2001", today.AddDays(-25), today.AddDays(35), "A00002", 120));
            _availableBatches.Add(new Batch("B2002", today.AddDays(-15), today.AddDays(45), "A00002", 180));
            _availableBatches.Add(new Batch("B2003", today.AddDays(-5), today.AddDays(55), "A00002", 90));

            // Crear lotes para el producto A00003 (3 lotes)
            _availableBatches.Add(new Batch("B3001", today.AddDays(-40), today.AddDays(20), "A00003", 80));
            _availableBatches.Add(new Batch("B3002", today.AddDays(-30), today.AddDays(30), "A00003", 110));
            _availableBatches.Add(new Batch("B3003", today.AddDays(-20), today.AddDays(40), "A00003", 140));

            // Crear lotes para el producto A00004 (3 lotes)
            _availableBatches.Add(new Batch("B4001", today.AddDays(-35), today.AddDays(25), "A00004", 90));
            _availableBatches.Add(new Batch("B4002", today.AddDays(-25), today.AddDays(35), "A00004", 120));
            _availableBatches.Add(new Batch("B4003", today.AddDays(-15), today.AddDays(45), "A00004", 150));

            // Crear lotes para el producto A00005 (3 lotes)
            _availableBatches.Add(new Batch("B5001", today.AddDays(-30), today.AddDays(30), "A00005", 100));
            _availableBatches.Add(new Batch("B5002", today.AddDays(-20), today.AddDays(40), "A00005", 130));
            _availableBatches.Add(new Batch("B5003", today.AddDays(-10), today.AddDays(50), "A00005", 160));

        }

        public async Task<List<Batch>> GetAvailableBatchesAsync()
        {
            // Simular llamada asíncrona
            await Task.Delay(100);
            var result = _availableBatches.Where(b => b.IsActive).ToList();
            return result;
        }

        public async Task<List<Batch>> GetAvailableBatchesForItemAsync(string itemCode)
        {
            await Task.Delay(50);

            var result = _availableBatches
                .Where(b => b.ItemCode == itemCode && b.IsActive && b.AvailableQuantity > 0)
                .ToList();

            return result;
        }

        public async Task<bool> AssignBatchToOrderLineAsync(int orderId, int lineNumber, string batchId)
        {
            // Esta función asigna todo el lote de una vez
            // Para asignaciones parciales, usar SaveBatchAssignmentsForOrderLineAsync

            // Verificar que el batch existe
            var batch = _availableBatches.FirstOrDefault(b => b.BatchId == batchId);
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

            // Solo verificar las cantidades si hay asignaciones
            if (assignments.Any())
            {
                // Verificar que la suma de las cantidades coincide con la cantidad de la línea
                int totalAssigned = assignments.Sum(a => a.Quantity);

                // Verificar que todos los batches existen y tienen suficiente cantidad
                foreach (var assignment in assignments)
                {
                    var batch = _availableBatches.FirstOrDefault(b => b.BatchId == assignment.BatchId);
                    if (batch == null)
                    {
                        return false;
                    }

                    // Verificar la cantidad disponible (teniendo en cuenta las asignaciones actuales)
                    int assignedToOthers = GetTotalAssignedToBatch(batch.BatchId, orderId, lineNumber);
                    if (batch.AvailableQuantity + assignedToOthers < assignment.Quantity)
                    {
                        return false;
                    }
                }
            }

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

        private int GetTotalAssignedToBatch(string batchId, int excludeOrderId = 0, int excludeLineNumber = 0)
        {
            int total = 0;
            foreach (var kvp in _batchAssignments)
            {
                // Excluir la línea específica si se indica
                if (kvp.Key.OrderId == excludeOrderId && kvp.Key.LineNumber == excludeLineNumber)
                    continue;

                // Sumar las cantidades asignadas a este batch
                total += kvp.Value.Where(a => a.BatchId == batchId).Sum(a => a.Quantity);
            }
            return total;
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
                // Verificar si ya hay asignaciones
                var assignments = await GetBatchAssignmentsForOrderLineAsync(orderId, line.LineNum);
                int totalAssigned = assignments.Sum(a => a.Quantity);
                int pendingQuantity = line.Quantity - totalAssigned;

                if (pendingQuantity <= 0)
                {
                    continue;
                }

                // Buscar batches disponibles para este artículo
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