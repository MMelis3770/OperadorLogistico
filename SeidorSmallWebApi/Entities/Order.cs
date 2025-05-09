namespace SeidorSmallWebApi.Entities
{
    public class Order
    {
        public int id { get; set; }
        public string client { get; set; }
        public string orderDate { get; set; }
        public string dueDate { get; set; }
        public List<OrderLine> lines { get; set; }
    }

    public class OrderLine
    {
        public int lineNumber { get; set; }
        public string itemCode { get; set; }
        public int quantity { get; set; }
        public string batch { get; set; }
    }
}
