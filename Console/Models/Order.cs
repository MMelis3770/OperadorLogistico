namespace OperadorLogistico.Models
{
    public class Order
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Customer { get; set; }
        public string DeliveryAddress { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
        public string Notes { get; set; }
        public string SAPReference { get; set; }
        public bool SentToSAP { get; set; }
        public DateTime? ConfirmationDate { get; set; }
        public string ErrorMessage { get; set; }
    }

    public enum OrderStatus
    {
        Received,
        InProcess,
        PendingConfirmation,
        Confirmed,
        Cancelled,
        Error
    }
}
