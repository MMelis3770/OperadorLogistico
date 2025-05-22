namespace SeidorSmallWebApi.Entities
{
    public class RecivedOrder
    {
        public int id { get; set; }
        public string client { get; set; }
        public DateTime orderDate { get; set; }
        public DateTime dueDate { get; set; }
        public int isProcessed { get; set; }
        public int hasError { get; set; }
        public string errorMessage { get; set; }
        public List<RecivedOrderLines> lines { get; set; }
    }
    public class RecivedOrderLines
    {
        public int lineNumber { get; set; }
        public string itemCode { get; set; }
        public int quantity { get; set; }
        public string batch {  get; set; }
    }
}
