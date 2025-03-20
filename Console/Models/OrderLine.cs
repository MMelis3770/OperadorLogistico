namespace OperadorLogistico.Models
{
    public class OrderLine
    {
        public int LineNumber { get; set; }
        public string OrderNumber { get; set; }
        public string ProductCode { get; set; }
        public Product Product { get; set; }
        public decimal RequestedQuantity { get; set; }
        public decimal AssignedQuantity { get; set; }

        // Key: Batch Code, Value: Quantity assigned
        public Dictionary<string, decimal> AssignedBatches { get; set; } = new Dictionary<string, decimal>();

        public OrderLineStatus Status { get; set; }
    }

    public enum OrderLineStatus
    {
        Pending,
        PartiallyAssigned,
        FullyAssigned,
        NoStock,
        Cancelled
    }
}
