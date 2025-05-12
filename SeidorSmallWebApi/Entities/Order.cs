namespace SeidorSmallWebApi.Entities
{
    public class Order
    {
        public string DocEntry { get; set; }
        public string U_CardCode { get; set; }
        public DateTime U_DocDate { get; set; }
        public DateTime U_DocDueDate { get; set; }
        public string U_Status { get; set; }
        public string U_ErrorMsg { get; set; }
        public List<OrderLine> Lines { get; set; }
    }
    public class OrderHeader
    {
        public string DocEntry { get; set; }
        public string U_CardCode { get; set; }
        public DateTime U_DocDate { get; set; }
        public DateTime U_DocDueDate { get; set; }
        public string U_Status { get; set; }
        public string U_ErrorMsg { get; set; }
    }
    public class OrderLine
    {
        public int DocEntry { get; set; }
        public int LineId { get; set; }
        public int U_LineNum { get; set; }
        public string U_ItemCode { get; set; }
        public int U_Quantity { get; set; }
        public Dictionary<string, int> U_Batch { get; set; }
    }
}
