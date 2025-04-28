namespace OperadorLogistico.Console.Models
{
    public class Product
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int OnHand { get; set; }
        public decimal PriceUnit { get; set; }

        public override string ToString()
        {
            return $"Product [ItemCode={ItemCode}, ItemName={ItemName}, OnHand={OnHand}, PriceUnit={PriceUnit}]";
        }
    }
}
