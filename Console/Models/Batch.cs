namespace OperadorLogistico.Models
{
    public class Batch
    {
        public string BatchCode { get; set; }
        public string ProductCode { get; set; }
        public Product Product { get; set; }
        public decimal AvailableQuantity { get; set; }
        public DateTime ProductionDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string Location { get; set; }
        public bool IsBlocked { get; set; }
        public string BlockReason { get; set; }
    }
}
