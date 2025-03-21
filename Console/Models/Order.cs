namespace OperadorLogistico.Console.Models
{
    public class Order
    {
        public string OrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string Customer { get; set; }
        public string DeliveryAddress { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderLine> Lines { get; set; } = new List<OrderLine>();
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
