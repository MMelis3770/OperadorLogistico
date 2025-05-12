namespace SeidorSmallWebApi.Entities
{
    public class Order
    {
        public string DocEntry { get; set; }
        public string CardCode { get; set; }
        public string DocDate { get; set; }
        public string DocDueDate { get; set; }
        public bool Status { get; set; }
        public string ErrorMsg { get; set; }
        public List<OrderLine> lines { get; set; }
    }

    public class OrderLine
    {
        public int DocEntry { get; set; }
        public int LineNum { get; set; }
        public string ItemCode { get; set; }
        public int Quantity { get; set; }
        public string Batch { get; set; }
    }
}
