using System;

namespace BlazorTemplate.Models
{
    public class Batch
    {
        public string BatchId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ItemCode { get; set; }
        public int AvailableQuantity { get; set; }

        // Propiedad calculada para determinar si el lote está activo
        public bool IsActive => true; // Para pruebas, siempre activo
                                      // En producción: DateTime.Now >= StartDate && DateTime.Now <= EndDate;

        public Batch()
        {
        }

        public Batch(string batchId, DateTime startDate, DateTime endDate, string itemCode, int availableQuantity)
        {
            BatchId = batchId;
            StartDate = startDate;
            EndDate = endDate;
            ItemCode = itemCode;
            AvailableQuantity = availableQuantity;
        }

        // Para mostrar en la interfaz
        public string DisplayInfo => $"{BatchId} ({StartDate:yyyy-MM-dd} a {EndDate:yyyy-MM-dd})";
    }
}