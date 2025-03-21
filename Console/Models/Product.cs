namespace OperadorLogistico.Console.Models
{
    public class Product
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string InventoryUOM { get; set; }
        public decimal UnitPrice { get; set; }

        //SAP DATA
        public string Category { get; set; }
    }
}
